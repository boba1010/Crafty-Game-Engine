namespace CraftyNative.ECS;

public sealed class JobSystem : IJobSystem, IDisposable
{
    private readonly PriorityQueue<IJob, int> _queue = new();
    private readonly object _queueLock = new();

    private readonly SemaphoreSlim _signal = new(0);
    private readonly CancellationTokenSource _cancellation = new();

    private readonly Thread[] _workerThreads;

    public JobSystem()
    {
        int threadCount = Math.Max(2, System.Environment.ProcessorCount - 1);
        _workerThreads = new Thread[threadCount];

        for (int i = 0; i < threadCount; i++)
        {
            _workerThreads[i] = new Thread(ProcessLoop)
            {
                Name = $"CraftyNative.Worker.{i}",
                IsBackground = true,
                Priority = ThreadPriority.AboveNormal
            };
            _workerThreads[i].Start();
        }
    }

    public JobFence CreateFence() => new();

    public void Submit(IJob job, int priority = 0, JobFence? fence = null)
    {
        fence?.Add();

        lock (_queueLock)
        {
            _queue.Enqueue(new FencedJob(job, fence), priority);
        }

        _signal.Release();
    }

    private void ProcessLoop()
    {
        while (!_cancellation.IsCancellationRequested)
        {
            try
            {
                _signal.Wait(_cancellation.Token);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            IJob? job = null;

            lock (_queueLock)
            {
                if (!_queue.TryDequeue(out job, out _))
                    continue;
            }

            job?.Execute();
        }
    }

    public void Dispose()
    {
        if (_cancellation.IsCancellationRequested) return;

        _cancellation.Cancel();

        try { _signal.Release(_workerThreads.Length); } catch (ObjectDisposedException) { }

        foreach (var thread in _workerThreads)
        {
            if (thread.IsAlive)
            {
                thread.Join(timeout: TimeSpan.FromSeconds(1));
            }
        }

        _signal.Dispose();
        _cancellation.Dispose();
    }

    private readonly struct FencedJob(IJob job, JobFence? fence) : IJob
    {
        private readonly IJob _job = job;
        private readonly JobFence? _fence = fence;

        public void Execute()
        {
            try
            {
                _job.Execute();
            }
            finally
            {
                _fence?.Signal();
            }
        }
    }
}