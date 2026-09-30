using Crafty.ChunkGeneration.World;
using Crafty.Engine.Components;
using Crafty.Engine.Jobs;
using CraftyNative;
using CraftyNative.ECS;
using CraftyNative.Scenes;
using CraftyNative.ThreeD;
using CraftyNative.ThreeD.Meshes;
using CraftyNative.ThreeD.World;

namespace Crafty.Engine.Systems;

public struct ChunkStreamingSystem(World world) : ISystem
{
    public int StreamingDistance { get; set; } = 8;

    private readonly World _world = world;

    private int _lastChunkX;
    private int _lastChunkZ;
    private bool _initialized;

    private HashSet<(int x, int z)> _loadedChunks = [];
    private readonly HashSet<(int x, int z)> _chunksToMesh = [];

    private JobFence? _loadFence;
    private JobFence? _meshFence;

    public void Update(ref Scene scene, double deltaTime)
    {
        if (_loadFence is not null)
        {
            if (!_loadFence.IsComplete)
                return;

            _loadFence = null;
            BuildMeshes();
        }

        if (_meshFence is not null)
        {
            if (!_meshFence.IsComplete)
                return;

            _meshFence = null;
        }

        foreach (var entity in scene.GetEntitiesWith<Player>())
        {
            ref var transform = ref scene.GetComponent<Transform>(entity);

            int chunkX = (int)MathF.Floor(transform.Position.X / Chunk.Size);
            int chunkZ = (int)MathF.Floor(transform.Position.Z / Chunk.Size);

            if (_initialized &&
                chunkX == _lastChunkX &&
                chunkZ == _lastChunkZ)
                continue;

            _initialized = true;
            _lastChunkX = chunkX;
            _lastChunkZ = chunkZ;

            UpdateStreaming(chunkX, chunkZ);
        }
    }

    private void BuildMeshes()
    {
        var fence = SystemAPI.JobSystem.CreateFence();
        bool jobsQueued = false;

        foreach (var (chunkX, chunkZ) in _chunksToMesh)
        {
            var chunk = _world.GetChunk(chunkX, chunkZ);

            if (chunk is null)
                continue;

            // GPU mesh already exists.
            var firstSection = new SectionCoordinate(chunkX, chunkZ, 0, 0, 0);

            if (WorldMeshManager.TryGet(firstSection, out _))
                continue;

            SystemAPI.JobSystem.Submit(new BuildChunkMeshJob(_world, chunk), fence: fence);

            jobsQueued = true;
        }

        _chunksToMesh.Clear();

        if (jobsQueued)
            _meshFence = fence;
    }

    private void UpdateStreaming(int centerX, int centerZ)
    {
        int radius = StreamingDistance;
        int radiusSquared = radius * radius;

        int startX = Math.Max(0, centerX - radius);
        int endX = centerX + radius;

        int startZ = Math.Max(0, centerZ - radius);
        int endZ = centerZ + radius;

        HashSet<(int x, int z)> requiredChunks = [];

        var fence = SystemAPI.JobSystem.CreateFence();
        bool jobsQueued = false;

        for (int chunkZ = startZ; chunkZ <= endZ; chunkZ++)
        {
            for (int chunkX = startX; chunkX <= endX; chunkX++)
            {
                int dx = chunkX - centerX;
                int dz = chunkZ - centerZ;

                int distanceSquared = dx * dx + dz * dz;

                if (distanceSquared > radiusSquared)
                    continue;

                requiredChunks.Add((chunkX, chunkZ));

                if (_world.GetChunk(chunkX, chunkZ) is null)
                {
                    SystemAPI.JobSystem.Submit(new LoadChunkJob(_world, chunkX, chunkZ), distanceSquared, fence);
                    jobsQueued = true;
                }

                _chunksToMesh.Add((chunkX, chunkZ));
            }
        }

        foreach (var (x, z) in _loadedChunks)
        {
            if (!requiredChunks.Contains((x, z)))
                SystemAPI.JobSystem.Submit(new UnloadChunkJob(_world, x, z));
        }

        _loadedChunks = requiredChunks;

        if (jobsQueued)
        {
            _loadFence = fence;
        }
        else
        {
            _loadFence = null;
            BuildMeshes();
        }
    }
}