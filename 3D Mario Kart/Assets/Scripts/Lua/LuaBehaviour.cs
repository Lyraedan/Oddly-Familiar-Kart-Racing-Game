using UnityEngine;
using MoonSharp.Interpreter;
using System.Collections.Generic;
using Coroutine = MoonSharp.Interpreter.Coroutine;
using static LuaCoroutineEntry;
using System.Collections;

public class LuaBehaviour : MonoBehaviour
{
    public TextAsset luaScript;

    private Script lua;
    private Table luaObject;

    private DynValue startFunc;
    private DynValue updateFunc;
    private DynValue updateFixedFunc;
    private DynValue updateLateFunc;

    private List<LuaCoroutineEntry> coroutines = new();

    private LuaGameObject _actor;

    public LuaGameObject actor
    {
        get
        {
            if (_actor == null)
                _actor = new LuaGameObject(gameObject);
            return _actor;
        }
    }

    private void Awake()
    {
        //UserData.RegisterAssembly();
        LuaAPI.Initialize();
        lua = new Script();
        LuaGlobals.Register(lua, this);

        LoadScript();
        BindLuaComponents();
    }

    private void Start()
    {
        CallLuaFunction(startFunc);
    }

    private void Update()
    {
        CallLuaFunction(updateFunc, Time.deltaTime);
    }

    private void FixedUpdate()
    {
        CallLuaFunction(updateFixedFunc, Time.fixedDeltaTime);
    }

    private void LateUpdate()
    {
        CallLuaFunction(updateLateFunc, Time.deltaTime);
    }

    private void CallLuaFunction(DynValue func, params object[] args)
    {
        if (func.IsNotNil() && func.Type == DataType.Function && luaObject != null)
        {
            if (args == null || args.Length == 0)
            {
                lua.Call(func, luaObject);
            }
            else
            {
                var fullArgs = new object[args.Length + 1];
                fullArgs[0] = luaObject; // Insert self
                System.Array.Copy(args, 0, fullArgs, 1, args.Length);

                lua.Call(func, fullArgs);
            }
        }
    }

    void LoadScript()
    {
        if (luaScript == null)
        {
            Debug.LogError($"LuaBehaviour on {name} has no script assigned!");
            return;
        }

        DynValue result = lua.DoString(luaScript.text);
        if (result.Type != DataType.Table)
        {
            Debug.LogError($"Lua script in {name} must return a table!");
            return;
        }

        luaObject = result.Table;

        startFunc = luaObject.Get("Start");

        updateFunc = luaObject.Get("Update");
        updateFixedFunc = luaObject.Get("UpdateFixed");
        updateLateFunc = luaObject.Get("UpdateLate");
    }

    void BindLuaComponents()
    {
        if (luaObject == null) return;

        luaObject["actor"] = actor;

        LuaComponent[] components = GetComponentsInChildren<LuaComponent>(true);

        foreach (var comp in components)
        {
            string key = string.IsNullOrEmpty(comp.luaName) ? comp.name : comp.luaName;

            // Handle duplicates
            int suffix = 1;
            string originalKey = key;
            while (luaObject.Get(key).Type != DataType.Nil)
                key = $"{originalKey}_{suffix++}";

            DynValue compValue = DynValue.FromObject(lua, comp);

            luaObject[key] = compValue;
        }
    }

    public void StartLuaCoroutine(DynValue fn)
    {
        if (fn.Type != DataType.Function)
            return;

        // Create a MoonSharp coroutine
        Coroutine co = lua.CreateCoroutine(fn).Coroutine;
        StartCoroutine(RunLuaCoroutine(co));
    }

    private IEnumerator RunLuaCoroutine(Coroutine co)
    {
        while (co.State != CoroutineState.Dead)
        {
            // Resume the Lua coroutine
            DynValue result = co.Resume(luaObject);

            if (result.Type == DataType.Tuple && result.Tuple.Length >= 1 && result.Tuple[0].Type == DataType.String)
            {
                string instr = result.Tuple[0].String;

                switch (instr)
                {
                    case "yield":
                        yield return null;
                        break;

                    case "wait":
                        yield return new WaitForSeconds((float)result.Tuple[1].Number);
                        break;

                    case "waitFrames":
                        int frames = (int)result.Tuple[1].Number;
                        for (int i = 0; i < frames; i++)
                            yield return null;
                        break;

                    case "waitUntil":
                        DynValue cond = result.Tuple[1];
                        yield return new WaitUntil(() => lua.Call(cond).CastToBool());
                        break;

                    default:
                        yield return null;
                        break;
                }
            }
            else
            {
                // If nothing was yielded, just wait a frame
                yield return null;
            }
        }
    }
}
