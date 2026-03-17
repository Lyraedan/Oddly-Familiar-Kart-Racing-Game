using MoonSharp.Interpreter;
using UnityEngine;

[MoonSharpUserData]
public class LuaPhysics
{

    public int defaultMask { get; private set; } = Physics.DefaultRaycastLayers;

    public DynValue Raycast(Script lua, Vector3 origin, Vector3 direction, float distance, int layerMask)
    {
        if (Physics.Raycast(origin, direction, out RaycastHit hit, distance, layerMask))
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

        return DynValue.False;
    }

    public DynValue Raycast(Script lua, Vector3 origin, Vector3 direction, float distance)
    {
        return Raycast(lua, origin, direction, distance, defaultMask);
    }

    public DynValue Raycast(Script lua, float ox, float oy, float oz, float dx, float dy, float dz, float distance, int layerMask)
    {
        Vector3 origin = new Vector3(ox, oy, oz);
        Vector3 direction = new Vector3(dx, dy, dz);

        if (Physics.Raycast(origin, direction, out RaycastHit hit, distance, layerMask))
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

        return DynValue.False;
    }

    public DynValue Raycast(Script lua, float ox, float oy, float oz, float dx, float dy, float dz, float distance)
    {
        return Raycast(lua, ox, oy, oz, dx, dy, dz, distance, defaultMask);
    }

    public int LayerMask(params int[] layers)
    {
        int mask = 0;
        foreach (var layer in layers)
            mask |= (1 << layer);
        return mask;
    }

    public DynValue RaycastAll(Script lua, Vector3 origin, Vector3 direction, float distance, int layerMask)
    {
        RaycastHit[] hits = Physics.RaycastAll(origin, direction, distance, layerMask);

        Table results = new Table(lua);

        for (int i = 0; i < hits.Length; i++)
        {
            var hit = hits[i];
            Table entry = new Table(lua);

            entry["object"] = new LuaGameObject(hit.collider.gameObject);

            Table point = new Table(lua);
            point["x"] = hit.point.x;
            point["y"] = hit.point.y;
            point["z"] = hit.point.z;
            entry["point"] = point;

            Table normal = new Table(lua);
            normal["x"] = hit.normal.x;
            normal["y"] = hit.normal.y;
            normal["z"] = hit.normal.z;
            entry["normal"] = normal;

            entry["distance"] = hit.distance;

            results[i + 1] = entry;
        }

        return DynValue.FromObject(lua, results);
    }

    public DynValue RaycastAll(Script lua, Vector3 origin, Vector3 direction, float distance)
    {
        return RaycastAll(lua, origin, direction, distance, defaultMask);
    }

    public DynValue SphereCast(Script lua, Vector3 origin, float radius, Vector3 direction, float distance, int layerMask)
    {
        if (Physics.SphereCast(origin, radius, direction, out RaycastHit hit, distance, layerMask))
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

        return DynValue.False;
    }

    public DynValue SphereCast(Script lua, Vector3 origin, float radius, Vector3 direction, float distance)
    {
        return SphereCast(lua, origin, radius, direction, distance, defaultMask);
    }

    public DynValue OverlapSphere(Script lua, Vector3 position, float radius, int layerMask)
    {
        Collider[] colliders = Physics.OverlapSphere(position, radius, layerMask);

        Table results = new Table(lua);

        for (int i = 0; i < colliders.Length; i++)
        {
            results[i + 1] = new LuaGameObject(colliders[i].gameObject);
        }

        return DynValue.FromObject(lua, results);
    }

    public DynValue OverlapSphere(Script lua, Vector3 position, float radius)
    {
        return OverlapSphere(lua, position, radius, defaultMask);
    }

    public bool CheckSphere(Vector3 position, float radius, int layerMask)
    {
        return Physics.CheckSphere(position, radius, layerMask);
    }

    public bool CheckSphere(Vector3 position, float radius)
    {
        return CheckSphere(position, radius, defaultMask);
    }

    public DynValue OverlapBox(Script lua, Vector3 center, Vector3 halfExtents, int layerMask)
    {
        Collider[] colliders = Physics.OverlapBox(center, halfExtents, Quaternion.identity, layerMask);

        Table results = new Table(lua);

        for (int i = 0; i < colliders.Length; i++)
        {
            results[i + 1] = new LuaGameObject(colliders[i].gameObject);
        }

        return DynValue.FromObject(lua, results);
    }

    public DynValue OverlapBox(Script lua, Vector3 center, Vector3 halfExtents)
    {
        return OverlapBox(lua, center, halfExtents, defaultMask);
    }

    public DynValue Linecast(Script lua, Vector3 start, Vector3 end, int layerMask)
    {
        if (Physics.Linecast(start, end, out RaycastHit hit, layerMask))
        {
            Table result = new Table(lua);

            result["object"] = new LuaGameObject(hit.collider.gameObject);

            Table point = new Table(lua);
            point["x"] = hit.point.x;
            point["y"] = hit.point.y;
            point["z"] = hit.point.z;
            result["point"] = point;

            result["distance"] = hit.distance;

            return DynValue.FromObject(lua, result);
        }

        return DynValue.False;
    }
    
    public DynValue Linecast(Script lua, Vector3 start, Vector3 end)
    {
        return Linecast(lua, start, end, defaultMask);
    }
}