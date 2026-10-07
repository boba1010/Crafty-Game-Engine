using Crafty.ChunkGeneration.IO;
using Crafty.SDK.World;

namespace Crafty.ChunkGeneration.World;

internal sealed class WorldGenerator
{
    private const int WorldSize = 10;

    public World GenerateWorld(string worldDir, string worldName, ulong seed, IProgress<WorldProgress> progress = null!)
    {
        progress?.Report(new("Creating world metadata...", 0));

        var world = new World() { Seed = seed, Name = worldName, Directory = Path.Combine(worldDir, worldName) };

        WorldIOManager.WorldDir = Path.Combine(worldDir, worldName);

        Directory.CreateDirectory(Path.Combine(Path.Combine(worldDir, worldName), "chunks"));

        int min = -WorldSize / 2;
        int max = WorldSize / 2;

        int totalChunks = (WorldSize - min) * (max - min);
        int completedChunks = 0;

        Parallel.For(min, WorldSize, x =>
        {
            for (int z = min; z < max; z++)
            {
                WorldIOManager.SaveChunk((Chunk)GenerateChunk(x, z, seed));

                int currentCompleted = Interlocked.Increment(ref completedChunks);
                float currentProgress = (float)currentCompleted / totalChunks;
                progress?.Report(new("Generating World...", currentProgress));
            }
        });

        WorldIOManager.SaveWorld(world);

        return world;
    }

    public IChunk GenerateChunk(int x, int z, ulong seed)
    {
        var generator = new ChunkGenerator();

        var context = new ChunkGenerationContext(seed, x, z, 0, 416);

        return generator.Generate(in context);
    }
}
