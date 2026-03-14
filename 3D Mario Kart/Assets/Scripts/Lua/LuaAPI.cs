using System;
using System.Linq;
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

        // Core wrapper, these dont inherit LuaComponent
        UserData.RegisterType<LuaGameObject>();
        UserData.RegisterType<LuaTransform>();

        AutoRegisterLuaComponents();

        // Unity
        UserData.RegisterType<Vector3>();
        UserData.RegisterType<Quaternion>();
        UserData.RegisterType<Color>();
    }

    private static void AutoRegisterLuaComponents()
    {
        // Using reflection auto register components that inherit LuaComponent
        var baseType = typeof(LuaComponent);

        var types = AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(a =>
            {
                try
                {
                    return a.GetTypes();
                }
                catch
                {
                    return Array.Empty<Type>();
                }
            })
            .Where(t =>
                baseType.IsAssignableFrom(t) &&
                !t.IsAbstract &&
                !t.IsGenericType
            );

        foreach (var type in types)
        {
            UserData.RegisterType(type);
        }
    }
}