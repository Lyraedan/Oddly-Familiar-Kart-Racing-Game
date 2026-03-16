using MoonSharp.Interpreter;
using UnityEngine;

[MoonSharpUserData]
public class LuaPhysics
{

    /// <summary>
    /// Performs a raycast in the Unity scene from a specified origin in a given direction and distance,
    /// and returns the hit information as a Lua table (DynValue) to be used in Lua scripts.
    /// </summary>
    /// <param name="lua">The MoonSharp Script context, required to create Lua tables.</param>
    /// <param name="origin">The ray's origin.</param>
    /// <param name="direction">The direction of the ray.</param>
    /// <param name="distance">The maximum distance the ray should travel.</param>
    /// <returns>
    /// A Lua table (DynValue) containing hit information if the ray intersects a collider:
    /// - "object": the hit GameObject as a LuaGameObject
    /// - "point": table { x, y, z } representing the hit point
    /// - "normal": table { x, y, z } representing the surface normal
    /// - "distance": distance from the origin to the hit point
    /// Returns nil if no object was hit.
    /// </returns>
    public DynValue Raycast(Script lua, Vector3 origin, Vector3 direction, float distance)
    {
        if (Physics.Raycast(origin, direction, out RaycastHit hit, distance))
        {
            Table result = new Table(lua);

            result["object"] = new LuaGameObject(hit.collider.gameObject);

            Table point = new Table(lua);
            point["x"] = hit.point.x;
            point["y"] = hit.point.y;
            point["z"] = hit.point.z;
            result["point"] = point;

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
}