using Crafty.ChunkGeneration.World;
using Crafty.Engine.Helpers;
using CraftyNative.ECS;
using CraftyNative.ThreeD.World;

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

        Chunk chunk;

        if (!ChunkCache.TryGet(_x, _z, out chunk!))
        {
            chunk = ChunkLoader.Load(_x, _z);
            ChunkCache.Set(_x, _z, chunk);
        }

        world.LoadChunk(chunk);

        //for (int sectionY = 0; sectionY < SectionCoordinate.SectionsY; sectionY++)
        //{
        //    for (int sectionZ = 0; sectionZ < 2; sectionZ++)
        //    {
        //        for (int sectionX = 0; sectionX < 2; sectionX++)
        //        {
        //            var coordinate = new SectionCoordinate(chunk.X, chunk.Z, sectionX, sectionY, sectionZ);

        //            var sectionMesh = MeshBuilder.BuildSectionMesh(world, sectionX, sectionY, sectionZ, chunk.X, chunk.Z);

        //            WorldMeshManager.TryAdd(new WorldMeshSection(coordinate, sectionMesh));
                    
        //        }
        //    }
        //}
    }
}
