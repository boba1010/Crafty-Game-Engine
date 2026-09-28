using Crafty.ChunkGeneration;
using Crafty.ChunkGeneration.World;

namespace Crafty.Engine.ChunkBuilding;

public static class ChunkLoader
{
    public static IWorldGenService WorldGenService { get; set; } = null!;
    public static string WorldDirectory { get; set; } = "";
    public static Chunk[] Load(int amount = 10)
    {
        List<Chunk> chunks = [];

        var files = Directory.GetFiles(WorldDirectory + @"\chunks");

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

            chunks.Add(WorldGenService.LoadChunk(x, z));
            i++;
        }

        return [.. chunks];
    }
}
