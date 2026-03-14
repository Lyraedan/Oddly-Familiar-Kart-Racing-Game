using MoonSharp.Interpreter;
using UnityEngine;

public class LuaCoroutineEntry
{
    public enum WaitType
    {
        None,
        Seconds,
        Frames,
        Until
    }

    public class WaitInstruction
    {
        public WaitType Type = WaitType.None;
        public double Seconds;          // for wait(seconds)
        public int Frames;              // for waitFrames(frames)
        public DynValue Condition;      // for waitUntil(func)
    }

    public MoonSharp.Interpreter.Coroutine Thread;
    public UnityEngine.Coroutine UnityCoroutine;      // Unity IEnumerator coroutine
    public WaitInstruction Waiting = new WaitInstruction();

    public LuaCoroutineEntry(MoonSharp.Interpreter.Coroutine thread)
    {
        Thread = thread;
    }

    public void ClearWaiting()
    {
        Waiting.Type = WaitType.None;
        Waiting.Seconds = 0;
        Waiting.Frames = 0;
        Waiting.Condition = null;
    }

    public bool IsWaiting => Waiting.Type != WaitType.None;
}