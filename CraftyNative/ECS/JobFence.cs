namespace CraftyNative.ECS;

public sealed class JobFence
{
    private int _remaining;
    private readonly ManualResetEventSlim _completed = new(true);

    internal void Add()
    {
        if (Interlocked.Increment(ref _remaining) == 1)
            _completed.Reset();
    }

    internal void Signal()
    {
        if (Interlocked.Decrement(ref _remaining) == 0)
            _completed.Set();
    }

    public bool IsComplete => Volatile.Read(ref _remaining) == 0;

    public void Wait()
    {
        _completed.Wait();
    }
}
