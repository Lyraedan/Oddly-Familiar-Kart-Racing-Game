using MoonSharp.Interpreter;
using UnityEngine;
using System;

public static class LuaGlobals
{
    public static void Register(Script lua, LuaBehaviour behaviour)
    {
        lua.Globals["print"] = (Action<DynValue>)LuaPrint;
        lua.Globals["warning"] = (Action<DynValue>)LuaWarning;
        lua.Globals["error"] = (Action<DynValue>)LuaError;

        lua.Globals["yield"] = (Func<DynValue>)Yield;
        lua.Globals["wait"] = (Func<double, DynValue>)Wait;
        lua.Globals["waitFrames"] = (Func<int, DynValue>)WaitFrames;
        lua.Globals["waitUntil"] = (Func<DynValue, DynValue>)WaitUntil;

        lua.Globals["startCoroutine"] = (Action<DynValue>)((fn) => behaviour.StartLuaCoroutine(fn));

        Table timeTable = new Table(lua);
        timeTable["deltaTime"] = (Func<float>)(() => Time.deltaTime);
        timeTable["fixedDeltaTime"] = (Func<float>)(() => Time.fixedDeltaTime);
        timeTable["time"] = (Func<float>)(() => Time.time);
        lua.Globals["time"] = timeTable;

        LuaPhysics physicsTable = new LuaPhysics();
        lua.Globals["physics"] = physicsTable;

        LuaVector3 vector3Table = new LuaVector3();
        lua.Globals["vector3"] = vector3Table;

        Table raceTable = new Table(lua);
        raceTable["raceStarted"] = (Func<bool>)(() => RaceManager.RACE_STARTED);
        raceTable["raceCompleted"] = (Func<bool>)(() => RaceManager.RACE_COMPLETED);
        lua.Globals["raceManager"] = raceTable;

        Table playerTable = new Table(lua);
        playerTable["currentLap"] = (Func<int>)(() => RaceManager.Instance.LocalPlayerLap.LAPCOUNT);
        playerTable["raceCompleted"] = (Func<bool>)(() => RaceManager.Instance.LocalPlayerLap.RaceComplete);
        lua.Globals["ourPlayer"] = playerTable;
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