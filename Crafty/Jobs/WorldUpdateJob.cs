using Crafty.ChunkGeneration.World;
using CraftyNative.ECS;
using CraftyNative.ThreeD.World;

namespace Crafty.Jobs;

public readonly struct WorldUpdateJob(World world) : IJob
{
    private readonly World _world = world;

    public void Execute()
    {
        WorldMeshManager.ProcessDirtyBlocks();

        HashSet<(int X, int Z)>? touched = null;

        while (WorldMeshManager.TryDequeueDirtySection(out var section))
        {
            if (section is null)
                continue;

            var coordinate = section.Coordinate;
            var mesh = MeshBuilder.BuildSectionMesh(_world, coordinate.SectionX, coordinate.SectionY, coordinate.SectionZ, coordinate.ChunkX, coordinate.ChunkZ);

            section.SetMesh(mesh);

            (touched ??= []).Add((coordinate.ChunkX, coordinate.ChunkZ));
        }

        while (WorldMeshManager.TryDequeueMissingSection(out var c))
        {
            if (!WorldMeshManager.HasChunk(c.ChunkX, c.ChunkZ))
                continue;

            var meshes = MeshBuilder.BuildSectionMesh(_world, c.SectionX, c.SectionY, c.SectionZ, c.ChunkX, c.ChunkZ);

            if (meshes.Opaque.Vertices.Count == 0 && meshes.Translucent.Vertices.Count == 0)
                continue;

            WorldMeshManager.UpdateSection(new WorldMeshSection(c, meshes));
        }

        if (touched is null)
            return;

        foreach (var (x, z) in touched)
            WorldMeshManager.BumpChunk(x, z);
    }
}