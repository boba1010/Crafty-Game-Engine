using Crafty.ChunkGeneration;
using Crafty.ChunkGeneration.World;

namespace Crafty.Engine.Helpers;

public static class ChunkLoader
{
    public static IWorldGenService WorldGenService { get; set; } = null!;

    public static Chunk[] Load(ulong seed, int amount = 10)
    {
        List<Chunk> chunks = [];

        var worldPath = Program.LaunchConfig.SelectedWorldPath;
        var files = Directory.GetFiles(Path.GetDirectoryName(worldPath) + @"\chunks");

        int i = 0;
        foreach (var filePath in files)
        {
            if (amount <= i)
                break;

            if (!filePath.EndsWith(".chunk"))
                continue;

            var file = Path.GetFileName(filePath);

            var unformatedCoords = file.Replace(".chunk", "");

            var coords = unformatedCoords.Split("_");

            int x = Convert.ToInt32(coords[0]);
            int z = Convert.ToInt32(coords[1]);

            chunks.Add(WorldGenService.LoadChunk(x, z, seed));
            i++;
        }

        return [.. chunks];
    }

    public static Chunk Load(int x, int z, ulong seed)
    {
        return WorldGenService.LoadChunk(x, z, seed);
    }
}
