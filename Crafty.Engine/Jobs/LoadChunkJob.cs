using Crafty.ChunkGeneration.World;
using Crafty.Engine.Helpers;
using Crafty.Engine.Systems.Lighting;
using Crafty.SDK.World;
using CraftyNative.ECS;

namespace Crafty.Engine.Jobs;

public readonly struct LoadChunkJob(World world, int x, int z) : IJob
{
    private readonly World _world = world;
    private readonly int _x = x;
    private readonly int _z = z;

    public void Execute()
    {
        var world = _world;

        if (world.GetChunk(_x, _z) is not null)
            return;

        IChunk chunk;

        if (!world.TryGetChunk(_x, _z, out chunk!))
            chunk = ChunkLoader.Load(_x, _z, _world.Seed);

        world.LoadChunk(chunk);

        SkyLightSystem.InitializeChunk(chunk);
    }
}
