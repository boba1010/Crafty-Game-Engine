using Crafty.ChunkGeneration.IO;
using Crafty.ChunkGeneration.World;

namespace Crafty.Engine.Core;

public static class WorldManager
{
    public static World Current { get; private set; } = null!;

    public static void Load()
    {
        var worldPath = Program.LaunchConfig.SelectedWorldPath;
        Current = WorldIOManager.LoadWorld(worldPath);
    }

    public static void Save()
    {
        foreach (var chunk in Current.chunks.Values)
        {
            if (chunk.IsModified)
                WorldIOManager.SaveChunk((Chunk)chunk);
        }
    }

    public static void DeleteUnchangedChunks()
    {
        string worldDir = Path.GetDirectoryName(Program.LaunchConfig.SelectedWorldPath)!;

        var chunksDir = Path.Combine(worldDir, "chunks");

        foreach (var chunk in Current.chunks.Values)
        {
            if (!chunk.IsModified)
            {
                var chunkPath = Path.Combine(chunksDir, $"{chunk.X}_{chunk.Z}.chunk");

                if (File.Exists(chunkPath))
                    File.Delete(chunkPath);
            }
        }
    }
}
