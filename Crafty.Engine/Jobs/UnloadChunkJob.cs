using Crafty.ChunkGeneration.World;
using CraftyNative.ECS;
using CraftyNative.ThreeD.Meshes;
using CraftyNative.ThreeD.World;

namespace Crafty.Engine.Jobs;

public readonly struct UnloadChunkJob(int x, int z) : IJob
{
    private readonly int _x = x;
    private readonly int _z = z;

    public void Execute()
    {
        WorldMeshManager.RemoveChunk(_x, _z);
        ChunkCache.Remove(_x, _z);
    }
}
