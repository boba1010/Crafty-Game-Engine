using Crafty.ChunkGeneration.IO;
using Crafty.SDK.World;

namespace Crafty.ChunkGeneration.World;

internal sealed class WorldGenerator
{
    private const int WorldSize = 10;
    private readonly ulong _seed;

    public WorldGenerator(ulong seed)
    {
        _seed = seed;
        WorldIOManager.WorldPath = @".\saves\silly";
    }

    public World GenerateWorld()
    {
        var world = new World() { Seed = _seed, Name = "silly", };

        Directory.CreateDirectory(Path.Combine(@$".\saves\{world.Name}", "chunks"));

        Parallel.For(0, WorldSize, x =>
        {
            for (int z = 0; z < WorldSize; z++)
            {
                WorldIOManager.SaveChunk((Chunk)GenerateChunk(x, z));
            }
        });

        WorldIOManager.SaveWorld(world);

        return world;
    }

    private IChunk GenerateChunk(int x, int z)
    {
        var generator = new ChunkGenerator();

        var context = new ChunkGenerationContext(_seed, x, z, 0, 416);

        return generator.Generate(in context);
    }
}
