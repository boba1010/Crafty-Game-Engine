using System.Collections.Concurrent;

namespace CraftyNative.ThreeD.ECS;

public sealed class JobSystem : IJobSystem
{
    private readonly ConcurrentQueue<IJob> _queue = [];
    private readonly SemaphoreSlim _signal = new(0);
    private readonly CancellationTokenSource _cancellation = new();
    private readonly Task _worker;

    public JobSystem()
    {
        _worker = Task.Run(Process);
    }

    public void Submit(IJob job)
    {
        _queue.Enqueue(job);
        _signal.Release();
    }

    private async Task Process()
    {
        while (!_cancellation.IsCancellationRequested)
        {
            await _signal.WaitAsync(_cancellation.Token);

            List<IJob> jobs = [];

            while (_queue.TryDequeue(out var job))
                jobs.Add(job);

            Parallel.ForEach(jobs, job => job.Execute());
        }
    }

    public void Dispose()
    {
        _cancellation.Cancel();

        try
        {
            _worker.Wait();
        }
        catch (AggregateException)
        {
        }

        _signal.Dispose();
        _cancellation.Dispose();
    }
}