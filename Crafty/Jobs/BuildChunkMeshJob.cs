using Crafty.ChunkGeneration.World;
using Crafty.SDK.World;
using CraftyNative.ECS;
using CraftyNative.ThreeD.World;

namespace Crafty.Jobs;

public readonly struct BuildChunkMeshJob(World world, IChunk chunk) : IJob
{
    private readonly World _world = world;
    private readonly IChunk _chunk = chunk;

    public void Execute()
    {
        if (_chunk is null)
            return;

        if (_chunk.IsEmpty)
            return;

        var built = new List<WorldMeshSection>();

        for (int sectionY = 0; sectionY < SectionCoordinate.SectionsY; sectionY++)
        {
            for (int sectionZ = 0; sectionZ < 2; sectionZ++)
            {
                for (int sectionX = 0; sectionX < 2; sectionX++)
                {
                    var coordinate = new SectionCoordinate(_chunk.X, _chunk.Z, sectionX, sectionY, sectionZ);

                    var meshes = MeshBuilder.BuildSectionMesh(_world, sectionX, sectionY, sectionZ, _chunk.X, _chunk.Z);

                    if (meshes.Opaque.Vertices.Count == 0)
                        continue;

                    var section = new WorldMeshSection(coordinate, meshes);
                    WorldMeshManager.AddOrReplace(section);
                    built.Add(section);
                }
            }
        }

        WorldMeshManager.PublishChunk(_chunk.X, _chunk.Z, [.. built]);
    }
}