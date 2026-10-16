using Crafty.ChunkGeneration.World;
using Crafty.Jobs;
using CraftyNative;
using CraftyNative.ECS;
using CraftyNative.Scenes;
using CraftyNative.ThreeD.Meshes;

namespace Crafty.Systems;

public sealed class WorldMeshUpdateSystem(World world) : ISystem
{
    public void Update(ref Scene scene, double deltaTime)
    {
        if (!WorldMeshManager.HasDirtyBlocks)
            return;

        SystemAPI.JobSystem.Submit(new WorldUpdateJob(world));
    }
}
