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

    private Dictionary<string, DynValue> luaFuncs = new();

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

    private void Start() => CallLua("Start");

    private void Update() => CallLua("Update", Time.deltaTime);

    private void FixedUpdate() => CallLua("FixedUpdate", Time.fixedDeltaTime);

    private void LateUpdate() => CallLua("LateUpdate", Time.deltaTime);

    private void OnCollisionEnter(Collision other) => CallLua("OnCollisionEnter", new LuaGameObject(other.gameObject));

    private void OnTriggerEnter(Collider other) => CallLua("OnTriggerEnter", new LuaGameObject(other.gameObject));

    private void CallLua(string funcName, params object[] args)
    {
        if (luaFuncs.TryGetValue(funcName, out var func))
        {
            if (args == null || args.Length == 0)
                lua.Call(func, luaObject);
            else
            {
                var fullArgs = new object[args.Length + 1];
                fullArgs[0] = luaObject; // self
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

        foreach (var pair in luaObject.Pairs)
        {
            if (pair.Value.Type == DataType.Function)
                luaFuncs[pair.Key.String] = pair.Value;
        }
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
