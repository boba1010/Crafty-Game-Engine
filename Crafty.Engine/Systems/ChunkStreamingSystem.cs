using Crafty.ChunkGeneration.World;
using Crafty.Engine.ChunkBuilding;
using Crafty.Engine.Components;
using Crafty.Engine.Helpers;
using Crafty.Engine.Jobs;
using CraftyNative;
using CraftyNative.ThreeD;
using CraftyNative.ThreeD.ECS;
using CraftyNative.ThreeD.Meshes;
using CraftyNative.ThreeD.Scenes;
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
    private HashSet<(int x, int z)> _queuedChunks = [];

    public void Update(ref Scene scene, double deltaTime)
    {
        foreach (var entity in scene.GetEntitiesWith<Player>())
        {
            ref var transform = ref scene.GetComponent<Transform>(entity);

            int chunkX = (int)MathF.Floor(transform.Position.X / Chunk.Size);
            int chunkZ = (int)MathF.Floor(transform.Position.Z / Chunk.Size);

            if (_initialized && chunkX == _lastChunkX && chunkZ == _lastChunkZ)
                continue;

            _initialized = true;
            _lastChunkX = chunkX;
            _lastChunkZ = chunkZ;

            UpdateStreaming(chunkX, chunkZ);
        }
    }

    private void UpdateStreaming(int centerX, int centerZ)
    {
        int radius = StreamingDistance;
        int radiusSquared = radius * radius;

        HashSet<(int x, int z)> requiredChunks = [];

        int startX = Math.Max(0, centerX - radius);
        int endX = centerX + radius;

        int startZ = Math.Max(0, centerZ - radius);
        int endZ = centerZ + radius;

        for (int chunkZ = startZ; chunkZ <= endZ; chunkZ++)
        {
            for (int chunkX = startX; chunkX <= endX; chunkX++)
            {
                int dx = chunkX - centerX;
                int dz = chunkZ - centerZ;

                if (dx * dx + dz * dz > radiusSquared)
                    continue;

                requiredChunks.Add((chunkX, chunkZ));

                if (_world.GetChunk(chunkX, chunkZ) is not null)
                    continue;

                if (!_queuedChunks.Add((chunkX, chunkZ)))
                    continue;

                QueueChunkLoad(chunkX, chunkZ);
            }
        }

        foreach (var (x, z) in _loadedChunks)
        {
            if (!requiredChunks.Contains((x, z)))
                QueueChunkUnload(x, z);
        }

        _loadedChunks = requiredChunks;
    }

    private void QueueChunkLoad(int x, int z)
    {
        if (_world.GetChunk(x, z) is not null)
            return;

        var job = new LoadChunkJob(_world, x, z);
        SystemAPI.JobSystem.Submit(job);
    }

    private void QueueChunkUnload(int x, int z)
    {
        var job = new UnloadChunkJob(_world, x, z);
        SystemAPI.JobSystem.Submit(job);
    }
}