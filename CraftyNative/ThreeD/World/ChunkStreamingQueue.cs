//using CraftyNative.ThreeD.ECS;

//namespace CraftyNative.ThreeD.World;

//public static class ChunkStreamingQueue
//{
//    private static readonly Queue<IJob> _queue = [];
//    private static readonly object _lock = new();

//    public static void Enqueue(IJob job)
//    {
//        lock (_lock)
//            _queue.Enqueue(job);
//    }

//    public static void Process()
//    {
//        IJob[] jobs;

//        lock (_lock)
//        {
//            if (_queue.Count == 0)
//                return;

//            jobs = [.. _queue];
//            _queue.Clear();
//        }

//        Parallel.ForEach(jobs, job => job.Execute());
//    }
//}

using CraftyNative.ECS;
using System.Collections.Concurrent;

namespace CraftyNative.ThreeD.World;

public static class ChunkStreamingQueue
{
    private static readonly ConcurrentQueue<IJob> _queue = [];
    private static readonly SemaphoreSlim _signal = new(0);
    private static readonly CancellationTokenSource _cancellation = new();

    private static readonly Thread[] _workers = CreateWorkers();

    public static void Enqueue(IJob job)
    {
        _queue.Enqueue(job);
        _signal.Release();
    }

    public static void Start()
    {
        foreach (var worker in _workers)
            worker.Start();
    }

    public static void Stop()
    {
        _cancellation.Cancel();

        _signal.Release(_workers.Length);

        foreach (var worker in _workers)
            worker.Join();
    }

    private static Thread[] CreateWorkers()
    {
        int count = Math.Max(1, Environment.ProcessorCount - 1);

        return [.. Enumerable.Range(0, count).Select(_ => new Thread(Worker) { IsBackground = true })];
    }

    private static void Worker()
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

            while (_queue.TryDequeue(out var job))
            {
                try
                {
                    job.Execute();
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
            }
        }
    }
}