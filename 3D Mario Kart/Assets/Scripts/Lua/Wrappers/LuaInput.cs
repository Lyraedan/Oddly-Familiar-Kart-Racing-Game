using MoonSharp.Interpreter;
using UnityEngine;

[MoonSharpUserData]
public class LuaInput : LuaAutoRegister
{
    public static bool GetKey(string key) => Input.GetKey(key);
    public static bool GetKeyDown(string key) => Input.GetKeyDown(key);
    public static bool GetKeyUp(string key) => Input.GetKeyUp(key);

    public static bool GetKeyCode(string keyCode)
    {
        if (System.Enum.TryParse<KeyCode>(keyCode, true, out var kc))
            return Input.GetKey(kc);
        throw new ScriptRuntimeException($"Invalid KeyCode '{keyCode}'");
    }

    public static bool GetKeyDownCode(string keyCode)
    {
        if (System.Enum.TryParse<KeyCode>(keyCode, true, out var kc))
            return Input.GetKeyDown(kc);
        throw new ScriptRuntimeException($"Invalid KeyCode '{keyCode}'");
    }

    public static bool GetKeyUpCode(string keyCode)
    {
        if (System.Enum.TryParse<KeyCode>(keyCode, true, out var kc))
            return Input.GetKeyUp(kc);
        throw new ScriptRuntimeException($"Invalid KeyCode '{keyCode}'");
    }

    public static bool GetMouseButton(int button) => Input.GetMouseButton(button);
    public static bool GetMouseButtonDown(int button) => Input.GetMouseButtonDown(button);
    public static bool GetMouseButtonUp(int button) => Input.GetMouseButtonUp(button);

    public static LuaVector3 MousePosition() => new LuaVector3(Input.mousePosition);

    public static float GetAxis(string axisName) => Input.GetAxis(axisName);
    public static float GetAxisRaw(string axisName) => Input.GetAxisRaw(axisName);

    public static int TouchCount() => Input.touchCount;
    public static LuaVector3 GetTouchPosition(int index)
    {
        if (index < 0 || index >= Input.touchCount)
            throw new ScriptRuntimeException($"Touch index {index} out of range");
        return new LuaVector3(Input.GetTouch(index).position);
    }

    public static bool AnyKey() => Input.anyKey;
    public static bool AnyKeyDown() => Input.anyKeyDown;
}