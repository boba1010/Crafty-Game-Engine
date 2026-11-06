using Crafty.ChunkGeneration.World;
using Crafty.Helpers;
using Crafty.Systems.Lighting;
using CraftyNative.ECS;
using System.Collections.Concurrent;

namespace Crafty.Jobs;

public readonly struct LoadChunkJob(World world, int x, int z, ConcurrentQueue<Chunk> completedLoads) : IJob
{
    private readonly World _world = world;
    private readonly int _x = x;
    private readonly int _z = z;
    private readonly ConcurrentQueue<Chunk> _completedLoads = completedLoads;

    public void Execute()
    {
        Chunk chunk;

        if (_world.TryGetChunk(_x, _z, out var existing))
        {
            chunk = (Chunk)existing;
        }
        else
        {
            chunk = ChunkLoader.Load(_x, _z, _world.Seed);
            _world.LoadChunk(chunk);
        }

        SkyLightSystem.InitializeChunk(chunk);

        _completedLoads.Enqueue(chunk);
    }
}