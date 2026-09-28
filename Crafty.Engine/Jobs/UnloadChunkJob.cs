using Crafty.ChunkGeneration.World;
using CraftyNative.ThreeD.ECS;
using CraftyNative.ThreeD.World;

namespace Crafty.Engine.Jobs;

public readonly struct UnloadChunkJob(World world, int x, int z) : IJob
{
    private readonly World _world = world;
    private readonly int _x = x;
    private readonly int _z = z;

    public void Execute()
    {
        ChunkCache.Remove(_x, _z);
        _world.UnloadChunk(_x, _z);
    }
}
