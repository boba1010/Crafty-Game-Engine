using Crafty.ChunkGeneration.World;
using CraftyNative.ECS;
using CraftyNative.ThreeD.Meshes;

namespace Crafty.Engine.Jobs;

public readonly struct WorldUpdateJob(World world) : IJob
{
    private readonly World _world = world;
    public void Execute()
    {
        WorldMeshManager.ProcessDirtyBlocks();
        while (WorldMeshManager.TryDequeueDirtySection(out var section))
        {
            if (section is null)
                continue;
            var coordinate = section.Coordinate;
            var mesh = MeshBuilder.BuildSectionMesh(_world, coordinate.SectionX, coordinate.SectionY, coordinate.SectionZ, coordinate.ChunkX, coordinate.ChunkZ);
            section.SetMesh(mesh);
        }
    }
}