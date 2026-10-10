using Crafty.ChunkGeneration.World;
using CraftyNative.ECS;
using CraftyNative.ThreeD.Meshes;

namespace Crafty.Engine.Jobs;

public readonly struct UnloadChunkJob(World world, int x, int z) : IJob
{
    public void Execute()
    {
        WorldMeshManager.RemoveChunk(x, z);
        world.UnloadChunk(x, z);
    }
}
