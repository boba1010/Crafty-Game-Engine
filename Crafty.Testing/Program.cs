using Crafty.ChunkGeneration;
using Crafty.ChunkGeneration.World;

namespace Crafty.Testing;

internal class Program
{
    public static void Main()
    {
        Console.WriteLine("GENERATING WORLD");

        var seed = WorldSeed.Generate();
        Console.WriteLine($"Seed: {seed}");

        IWorldGenService worldGenService = new WorldGenService(new(".\\saves\\silly"));

        var genWorld = worldGenService.GenerateWorld(seed);

        Console.WriteLine($"{genWorld.Name} HAS FINISHED GENERATING");

        Console.WriteLine("LOADING WORLD");

        var world = worldGenService.LoadWorld(".\\saves\\silly\\silly.world");

        Console.WriteLine($"{world.Name} HAS FINISHED LOADING");

        Console.WriteLine("LOADING CHUNK");

        var chunk = worldGenService.LoadChunk(0, 0);

        foreach (var block in chunk.Blocks)
            Console.Write($"{block.Id}");
    }
}
