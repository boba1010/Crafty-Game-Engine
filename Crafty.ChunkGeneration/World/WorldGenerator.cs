using Crafty.ChunkGeneration.IO;
using Crafty.SDK.World;

namespace Crafty.ChunkGeneration.World;

internal sealed class WorldGenerator(ulong seed)
{
    private const int WorldSize = 10;
    private readonly WorldIOManager _worldIOManager = new(@".\saves\silly");

    public World GenerateWorld()
    {
        var world = new World() { Seed = seed, Name = "silly", };

        Directory.CreateDirectory(Path.Combine(@$".\saves\{world.Name}", "chunks"));

        Parallel.For(0, WorldSize, x =>
        {
            for (int z = 0; z < WorldSize; z++)
            {
                _worldIOManager.SaveChunk((Chunk)GenerateChunk(x, z));
            }
        });

        _worldIOManager.SaveWorld(world);

        return world;
    }

    private IChunk GenerateChunk(int x, int z)
    {
        var generator = new ChunkGenerator();

        var context = new ChunkGenerationContext(seed, x, z, 0, 400);

        return generator.Generate(in context);
    }
}
