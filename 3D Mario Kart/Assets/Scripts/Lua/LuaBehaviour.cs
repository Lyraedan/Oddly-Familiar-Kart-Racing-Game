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

    private void Awake()
    {
        //UserData.RegisterAssembly();
        LuaAPI.Initialize();
        lua = new Script();
        RegisterGlobals();

        LoadScript();
        BindLuaComponents();
    }

    private void Start()
    {
        if (startFunc.IsNotNil() && startFunc.Type == DataType.Function && luaObject != null)
        {
            lua.Call(startFunc, luaObject);
        }
    }

    private void Update()
    {
        if (updateFunc.IsNotNil() && updateFunc.Type == DataType.Function && luaObject != null)
        {
            lua.Call(updateFunc, luaObject, Time.deltaTime);
        }
    }

    private void FixedUpdate()
    {
        if (updateFixedFunc.IsNotNil() && updateFixedFunc.Type == DataType.Function && luaObject != null)
        {
            lua.Call(updateFixedFunc, luaObject, Time.fixedDeltaTime);
        }
    }

    private void LateUpdate()
    {
        if (updateLateFunc.IsNotNil() && updateLateFunc.Type == DataType.Function && luaObject != null)
        {
            lua.Call(updateFunc, luaObject, Time.deltaTime);
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

    void RegisterGlobals()
    {
        lua.Globals["print"] = (System.Action<DynValue>)LuaPrint;
        lua.Globals["warning"] = (System.Action<DynValue>)LuaWarning;
        lua.Globals["error"] = (System.Action<DynValue>)LuaError;

        lua.Globals["yield"] = (System.Func<DynValue>)Yield;
        lua.Globals["wait"] = (System.Func<double, DynValue>)Wait;
        lua.Globals["waitFrames"] = (System.Func<int, DynValue>)WaitFrames;
        lua.Globals["waitUntil"] = (System.Func<DynValue, DynValue>)WaitUntil;
        lua.Globals["startCoroutine"] = (System.Action<DynValue>)((fn) => StartLuaCoroutine(fn)); // TODO: doesn't work yet, a while loop in this in lua crashes unity
    }

    void BindLuaComponents()
    {
        if (luaObject == null) return;

        // Bind all components to the Lua table
        LuaComponent[] components = GetComponentsInChildren<LuaComponent>(true);

        foreach(var comp in components)
        {
            string key = comp.luaName;

            if (string.IsNullOrEmpty(key))
            {
                key = comp.gameObject.name;
            }

            string tableName = comp.GetLuaTable();

            DynValue existing = luaObject.Get(tableName);
            Table table;

            if(existing.Type == DataType.Table)
            {
                table = existing.Table;
            }
            else
            {
                table = new Table(lua);
                luaObject[tableName] = table;
            }

            // Handle duplicates
            int suffix = 1;
            string originalKey = key;

            while(table.Get(key).Type != DataType.Nil)
            {
                key = $"{originalKey}_{suffix++}";
            }

            table[key] = comp;
        }
    }

    // This shit should just work....
    private void StartLuaCoroutine(DynValue fn)
    {
        if (fn.Type != DataType.Function)
            return;

        // Create a MoonSharp coroutine
        Coroutine co = lua.CreateCoroutine(fn).Coroutine;
        StartCoroutine(RunLuaCoroutine(co));
    }

    // WIP
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

    #region Global methods
    private DynValue Yield()
    {
        // Pause one frame
        return DynValue.NewYieldReq(new DynValue[] { DynValue.NewString("yield") });
    }

    private DynValue Wait(double seconds)
    {
        // Pause for N seconds
        return DynValue.NewYieldReq(new DynValue[] {
        DynValue.NewString("wait"),
        DynValue.NewNumber(seconds)
    });
    }

    private DynValue WaitFrames(int frames)
    {
        return DynValue.NewYieldReq(new DynValue[] {
        DynValue.NewString("waitFrames"),
        DynValue.NewNumber(frames)
    });
    }

    private DynValue WaitUntil(DynValue condition)
    {
        if (condition.Type != DataType.Function)
        {
            Debug.LogWarning("waitUntil expects a function.");
            return DynValue.Nil;
        }

        return DynValue.NewYieldReq(new DynValue[] {
        DynValue.NewString("waitUntil"),
        condition
    });
    }

    private void LuaPrint(DynValue value)
    {
        Debug.Log("[Lua] " + value.ToString());
    }

    private void LuaWarning(DynValue value)
    {
        Debug.LogWarning("[Lua] " + value.ToString());
    }

    private void LuaError(DynValue value)
    {
        Debug.LogError("[Lua] " + value.ToString());
    }
    #endregion
}
