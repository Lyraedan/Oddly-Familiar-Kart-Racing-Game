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
        AutoRegisterLuaComponents();

        // Unity - Ideally I would like to use wrappers for this
        //UserData.RegisterType<Vector3>();
        //UserData.RegisterType<Quaternion>();
        //UserData.RegisterType<Color>();
    }

    private static void AutoRegisterLuaComponents()
    {
        var baseTypes = new[]
        {
            typeof(LuaComponent), // Lua Components that inherit from Monobehaviour
            typeof(LuaAutoRegister) // Lua Components that do not inherit from Monobehaviour, but still want to be registered
        };

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
                baseTypes.Any(bt => bt.IsAssignableFrom(t)) &&
                !t.IsAbstract &&
                !t.IsGenericType
            );

        foreach (var type in types)
        {
            UserData.RegisterType(type);
        }
    }
}