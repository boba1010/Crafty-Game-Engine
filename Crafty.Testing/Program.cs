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

        IWorldGenService worldGenService = new WorldGenService(new(".\\saves\\silly"), seed);

        var genWorld = worldGenService.GenerateWorld();

        Console.WriteLine($"{genWorld.Name} HAS FINISHED GENERATING");
    }
}
