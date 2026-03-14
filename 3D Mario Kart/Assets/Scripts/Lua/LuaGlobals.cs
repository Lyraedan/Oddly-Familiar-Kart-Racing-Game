using MoonSharp.Interpreter;
using UnityEngine;
using System;

public static class LuaGlobals
{
    public static void Register(Script lua, LuaBehaviour behaviour)
    {
        // Logging
        lua.Globals["print"] = (Action<DynValue>)LuaPrint;
        lua.Globals["warning"] = (Action<DynValue>)LuaWarning;
        lua.Globals["error"] = (Action<DynValue>)LuaError;

        // Coroutine helpers
        lua.Globals["yield"] = (Func<DynValue>)Yield;
        lua.Globals["wait"] = (Func<double, DynValue>)Wait;
        lua.Globals["waitFrames"] = (Func<int, DynValue>)WaitFrames;
        lua.Globals["waitUntil"] = (Func<DynValue, DynValue>)WaitUntil;

        // Coroutine start (needs behaviour reference)
        lua.Globals["startCoroutine"] = (Action<DynValue>)((fn) => behaviour.StartLuaCoroutine(fn));
    }

    #region Coroutine helpers

    private static DynValue Yield()
    {
        return DynValue.NewYieldReq(new DynValue[]
        {
            DynValue.NewString("yield")
        });
    }

    private static DynValue Wait(double seconds)
    {
        return DynValue.NewYieldReq(new DynValue[]
        {
            DynValue.NewString("wait"),
            DynValue.NewNumber(seconds)
        });
    }

    private static DynValue WaitFrames(int frames)
    {
        return DynValue.NewYieldReq(new DynValue[]
        {
            DynValue.NewString("waitFrames"),
            DynValue.NewNumber(frames)
        });
    }

    private static DynValue WaitUntil(DynValue condition)
    {
        if (condition.Type != DataType.Function)
        {
            Debug.LogWarning("waitUntil expects a function.");
            return DynValue.Nil;
        }

        return DynValue.NewYieldReq(new DynValue[]
        {
            DynValue.NewString("waitUntil"),
            condition
        });
    }

    #endregion

    #region Logging

    private static void LuaPrint(DynValue value)
    {
        Debug.Log("[Lua] " + value);
    }

    private static void LuaWarning(DynValue value)
    {
        Debug.LogWarning("[Lua] " + value);
    }

    private static void LuaError(DynValue value)
    {
        Debug.LogError("[Lua] " + value);
    }

    #endregion
}