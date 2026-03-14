using MoonSharp.Interpreter;
using UnityEngine;

public static class LuaAPI
{
    private static bool initialized = false;

    public static void Initialize()
    {
        if (initialized)
            return;

        initialized = true;

        // Core wrapper
        UserData.RegisterType<LuaGameObject>();

        // Components
        UserData.RegisterType<LuaComponent>();
        UserData.RegisterType<LuaAnimator>();
        UserData.RegisterType<LuaTransform>();
        UserData.RegisterType<LuaRigidbody>();
        UserData.RegisterType<LuaAudioSource>();
        UserData.RegisterType<LuaLight>();

        // Unity
        UserData.RegisterType<Vector3>();
        UserData.RegisterType<Quaternion>();
        UserData.RegisterType<Color>();
    }
}