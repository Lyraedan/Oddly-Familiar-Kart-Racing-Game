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

        Table physicsTable = new Table(lua);
        physicsTable["raycast"] = (Func<double, double, double, double, double, double, double, DynValue>)
            ((ox, oy, oz, dx, dy, dz, distance) => Raycast(lua, ox, oy, oz, dx, dy, dz, distance));

        lua.Globals["physics"] = physicsTable;

        Table raceTable = new Table(lua);
        raceTable["raceStarted"] = (Func<bool>)(() => RaceManager.RACE_STARTED);
        raceTable["raceCompleted"] = (Func<bool>)(() => RaceManager.RACE_COMPLETED);
        lua.Globals["raceManager"] = raceTable;

        Table playerTable = new Table(lua);
        playerTable["currentLap"] = (Func<int>)(() => RaceManager.Instance.LocalPlayerLap.LAPCOUNT);
        playerTable["raceCompleted"] = (Func<bool>)(() => RaceManager.Instance.LocalPlayerLap.RaceComplete);
        lua.Globals["ourPlayer"] = playerTable;
    }

    /// <summary>
    /// Performs a raycast in the Unity scene from a specified origin in a given direction and distance,
    /// and returns the hit information as a Lua table (DynValue) to be used in Lua scripts.
    /// </summary>
    /// <param name="lua">The MoonSharp Script context, required to create Lua tables.</param>
    /// <param name="ox">The X coordinate of the ray's origin.</param>
    /// <param name="oy">The Y coordinate of the ray's origin.</param>
    /// <param name="oz">The Z coordinate of the ray's origin.</param>
    /// <param name="dx">The X component of the ray's direction vector.</param>
    /// <param name="dy">The Y component of the ray's direction vector.</param>
    /// <param name="dz">The Z component of the ray's direction vector.</param>
    /// <param name="distance">The maximum distance the ray should travel.</param>
    /// <returns>
    /// A Lua table (DynValue) containing hit information if the ray intersects a collider:
    /// - "object": the hit GameObject as a LuaGameObject
    /// - "point": table { x, y, z } representing the hit point
    /// - "normal": table { x, y, z } representing the surface normal
    /// - "distance": distance from the origin to the hit point
    /// Returns nil if no object was hit.
    /// </returns>
    private static DynValue Raycast(Script lua, double ox, double oy, double oz, double dx, double dy, double dz, double distance)
    {
        Vector3 origin = new Vector3((float)ox, (float)oy, (float)oz);
        Vector3 dir = new Vector3((float)dx, (float)dy, (float)dz);

        if (Physics.Raycast(origin, dir, out RaycastHit hit, (float)distance))
        {
            Table result = new Table(lua);

            // hit object
            result["object"] = new LuaGameObject(hit.collider.gameObject);

            // hit point
            Table point = new Table(lua);
            point["x"] = hit.point.x;
            point["y"] = hit.point.y;
            point["z"] = hit.point.z;
            result["point"] = point;

            // normal
            Table normal = new Table(lua);
            normal["x"] = hit.normal.x;
            normal["y"] = hit.normal.y;
            normal["z"] = hit.normal.z;
            result["normal"] = normal;

            result["distance"] = hit.distance;

            return DynValue.FromObject(lua, result);
        }

        return DynValue.Nil;
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