using MoonSharp.Interpreter;

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

    public Coroutine Thread;
    public WaitInstruction Waiting = new WaitInstruction();

    public LuaCoroutineEntry(Coroutine thread)
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