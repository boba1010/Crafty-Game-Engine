using Crafty.ChunkGeneration.World;
using Crafty.Components;
using Crafty.Jobs;
using CraftyNative;
using CraftyNative.ECS;
using CraftyNative.Scenes;
using CraftyNative.ThreeD;
using CraftyNative.ThreeD.World;
using System.Collections.Concurrent;

namespace Crafty.Systems;

public sealed class ChunkStreamingSystem(World world) : IGameplaySystem
{
    public int StreamingDistance { get; set; } = 8;

    public int MaxConcurrentLoads { get; set; } = 2;
    public int MaxConcurrentMeshes { get; set; } = 2;

    private readonly World _world = world;

    private int _lastChunkX;
    private int _lastChunkZ;
    private bool _initialized;

    private readonly HashSet<(int x, int z)> _requiredChunks = [];
    private readonly HashSet<(int x, int z)> _loadingChunks = [];

    private readonly ConcurrentQueue<Chunk> _completedLoads = new();

    private readonly List<(int x, int z, int distance)> _pendingLoads = [];
    private int _pendingLoadIndex;

    private readonly Queue<Chunk> _meshQueue = new();
    private readonly HashSet<(int x, int z)> _meshQueued = [];
    private readonly HashSet<(int x, int z)> _meshingChunks = [];
    private readonly HashSet<(int x, int z)> _meshedChunks = [];

    private readonly List<(JobFence fence, int x, int z)> _activeMeshJobs = [];

    private int _loadsInFlight;

    public void Update(ref Scene scene, double deltaTime)
    {
        UpdatePlayerPosition(ref scene);

        ProcessCompletedLoads();
        ProcessCompletedMeshes();

        PumpLoads();
        PumpMeshes();
    }

    private void UpdatePlayerPosition(ref Scene scene)
    {
        foreach (var entity in scene.GetEntitiesWith<Player>())
        {
            ref var transform = ref scene.GetComponent<Transform>(entity);

            int chunkX = (int)MathF.Floor(transform.Position.X / Chunk.Size);
            int chunkZ = (int)MathF.Floor(transform.Position.Z / Chunk.Size);

            if (_initialized && chunkX == _lastChunkX && chunkZ == _lastChunkZ)
                return;

            _initialized = true;
            _lastChunkX = chunkX;
            _lastChunkZ = chunkZ;

            UpdateStreaming(chunkX, chunkZ);
            return;
        }
    }

    private void UpdateStreaming(int centerX, int centerZ)
    {
        int radius = StreamingDistance;
        int radiusSquared = radius * radius;

        HashSet<(int x, int z)> required = [];

        _pendingLoads.Clear();
        _pendingLoadIndex = 0;

        for (int z = centerZ - radius; z <= centerZ + radius; z++)
        {
            for (int x = centerX - radius; x <= centerX + radius; x++)
            {
                int dx = x - centerX;
                int dz = z - centerZ;
                int distance = dx * dx + dz * dz;

                if (distance > radiusSquared)
                    continue;

                var coordinate = (x, z);
                required.Add(coordinate);

                if (_world.GetChunk(x, z) is { } chunk)
                    QueueMesh((Chunk)chunk);
                else if (!_loadingChunks.Contains(coordinate))
                    _pendingLoads.Add((x, z, distance));
            }
        }

        // Retire chunks that are no longer required.
        foreach (var coordinate in _requiredChunks)
        {
            if (required.Contains(coordinate))
                continue;

            RetireChunk(coordinate);
        }

        _requiredChunks.Clear();
        _requiredChunks.UnionWith(required);

        // Nearest chunks are generated first.
        _pendingLoads.Sort(static (a, b) => a.distance.CompareTo(b.distance));
    }

    private void PumpLoads()
    {
        while (_loadsInFlight < MaxConcurrentLoads && _pendingLoadIndex < _pendingLoads.Count)
        {
            var (x, z, distance) = _pendingLoads[_pendingLoadIndex++];

            var coordinate = (x, z);

            if (!_requiredChunks.Contains(coordinate))
                continue;

            if (_world.GetChunk(x, z) is { } existing)
            {
                QueueMesh((Chunk)existing);
                continue;
            }

            if (!_loadingChunks.Add(coordinate))
                continue;

            _loadsInFlight++;

            SystemAPI.JobSystem.Submit(new LoadChunkJob(_world, x, z, _completedLoads), distance);
        }
    }

    private void ProcessCompletedLoads()
    {
        while (_completedLoads.TryDequeue(out var chunk))
        {
            var coordinate = (chunk.X, chunk.Z);

            _loadingChunks.Remove(coordinate);
            _loadsInFlight--;

            if (!_requiredChunks.Contains(coordinate))
            {
                SystemAPI.JobSystem.Submit(new UnloadChunkJob(_world, chunk.X, chunk.Z));
                continue;
            }

            QueueMesh(chunk);
        }
    }

    private void QueueMesh(Chunk chunk)
    {
        var coordinate = (chunk.X, chunk.Z);

        if (_meshedChunks.Contains(coordinate) || _meshingChunks.Contains(coordinate) || !_meshQueued.Add(coordinate))
            return;

        var firstSection = new SectionCoordinate(chunk.X, chunk.Z, 0, 0, 0);

        if (WorldMeshManager.TryGet(firstSection, out _))
        {
            _meshedChunks.Add(coordinate);
            return;
        }

        _meshQueue.Enqueue(chunk);
    }

    private void PumpMeshes()
    {
        while (_activeMeshJobs.Count < MaxConcurrentMeshes && _meshQueue.TryDequeue(out var chunk))
        {
            var coordinate = (chunk.X, chunk.Z);

            _meshQueued.Remove(coordinate);

            // Discard stale queue entries.
            if (!_requiredChunks.Contains(coordinate))
                continue;

            if (!ReferenceEquals(_world.GetChunk(chunk.X, chunk.Z), chunk))
                continue;

            if (_meshedChunks.Contains(coordinate) || !_meshingChunks.Add(coordinate))
                continue;

            var fence = SystemAPI.JobSystem.CreateFence();

            SystemAPI.JobSystem.Submit(new BuildChunkMeshJob(_world, chunk), fence: fence);

            _activeMeshJobs.Add((fence, chunk.X, chunk.Z));
        }
    }

    private void ProcessCompletedMeshes()
    {
        for (int i = _activeMeshJobs.Count - 1; i >= 0; i--)
        {
            var job = _activeMeshJobs[i];

            if (!job.fence.IsComplete)
                continue;

            var coordinate = (job.x, job.z);

            _meshingChunks.Remove(coordinate);

            _activeMeshJobs.RemoveAt(i);

            if (_requiredChunks.Contains(coordinate))
                _meshedChunks.Add(coordinate);
            else
                SystemAPI.JobSystem.Submit(new UnloadChunkJob(_world, job.x, job.z));
        }
    }

    private void RetireChunk((int x, int z) coordinate)
    {
        if (_loadingChunks.Contains(coordinate) || _meshingChunks.Contains(coordinate))
            return;

        // Queued mesh entries become stale and are ignored by PumpMeshes.
        _meshQueued.Remove(coordinate);
        _meshedChunks.Remove(coordinate);

        if (_world.GetChunk(coordinate.x, coordinate.z) is not null)
            SystemAPI.JobSystem.Submit(new UnloadChunkJob(_world, coordinate.x, coordinate.z));
    }
}