using MoonSharp.Interpreter;
using UnityEngine;

public class LuaVector3 : LuaAutoRegister
{
    public float x { get; set; }
    public float y { get; set; }
    public float z { get; set; }

    public LuaVector3() { x = y = z = 0; }

    public LuaVector3(Vector3 vec3)
    {
        x = vec3.x;
        y = vec3.y;
        z = vec3.z;
    }

    public LuaVector3(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public Vector3 ToUnityVector() => new Vector3(x, y, z);

    public LuaVector3 New(float x, float y, float z) => new LuaVector3(x, y, z);

    public float Magnitude() => Mathf.Sqrt(x * x + y * y + z * z);

    public LuaVector3 Normalize()
    {
        float mag = Magnitude();
        if (mag > 0)
            return new LuaVector3(x / mag, y / mag, z / mag);
        return new LuaVector3(0, 0, 0);
    }

    public static float Dot(LuaVector3 a, LuaVector3 b) => a.x * b.x + a.y * b.y + a.z * b.z;

    public static LuaVector3 Cross(LuaVector3 a, LuaVector3 b) =>
        new LuaVector3(
            a.y * b.z - a.z * b.y,
            a.z * b.x - a.x * b.z,
            a.x * b.y - a.y * b.x
        );

    public static LuaVector3 Lerp(LuaVector3 a, LuaVector3 b, float t) =>
        new LuaVector3(
            Mathf.Lerp(a.x, b.x, t),
            Mathf.Lerp(a.y, b.y, t),
            Mathf.Lerp(a.z, b.z, t)
        );

    public static float Distance(LuaVector3 a, LuaVector3 b)
    {
        return Vector3.Distance(a.ToUnityVector(), b.ToUnityVector());
    }

    public LuaVector3 Reflect(LuaVector3 direction, LuaVector3 normal)
    {
        return new LuaVector3(Vector3.Reflect(direction.ToUnityVector(), normal.ToUnityVector()));
    }

    [MoonSharpUserDataMetamethod("__add")]
    public static LuaVector3 Add(LuaVector3 a, LuaVector3 b) => new LuaVector3(a.x + b.x, a.y + b.y, a.z + b.z);

    [MoonSharpUserDataMetamethod("__sub")]
    public static LuaVector3 Sub(LuaVector3 a, LuaVector3 b) => new LuaVector3(a.x - b.x, a.y - b.y, a.z - b.z);

    [MoonSharpUserDataMetamethod("__mul")]
    public static LuaVector3 Mul(LuaVector3 a, DynValue b)
    {
        if (b.Type == DataType.Number)
            return new LuaVector3(a.x * (float)b.Number, a.y * (float)b.Number, a.z * (float)b.Number);
        throw new ScriptRuntimeException("Can only multiply LuaVector3 by a number");
    }

    [MoonSharpUserDataMetamethod("__div")]
    public static LuaVector3 Div(LuaVector3 a, DynValue b)
    {
        if (b.Type == DataType.Number)
            return new LuaVector3(a.x / (float)b.Number, a.y / (float)b.Number, a.z / (float)b.Number);
        throw new ScriptRuntimeException("Can only divide LuaVector3 by a number");
    }

    [MoonSharpUserDataMetamethod("__eq")]
    public static bool Equal(LuaVector3 a, LuaVector3 b) =>
        Mathf.Approximately(a.x, b.x) &&
        Mathf.Approximately(a.y, b.y) &&
        Mathf.Approximately(a.z, b.z);

    [MoonSharpUserDataMetamethod("__tostring")]
    public static string ToStr(LuaVector3 v) => $"({v.x}, {v.y}, {v.z})";
}