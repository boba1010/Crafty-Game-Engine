using Crafty.ChunkGeneration.World;
using System.IO.Compression;

namespace Crafty.ChunkGeneration.IO;

public static class WorldIOManager
{
    public static string WorldPath { get; set; } = null!;

    public static void SaveWorld(World.World world)
    {
        var path = Path.Combine(WorldPath, $"{world.Name}.world");

        var fs = File.Open(path, FileMode.CreateNew);
        using var compression = new DeflateStream(fs, CompressionMode.Compress);
        using var writer = new BinaryWriter(compression);

        writer.Write(world.Name);
        writer.Write(world.Seed);
    }

    public static World.World LoadWorld(string path)
    {
        using var fs = File.OpenRead(path);
        using var compression = new DeflateStream(fs, CompressionMode.Decompress);
        using var reader = new BinaryReader(compression);

        var name = reader.ReadString();
        var seed = reader.ReadUInt64();

        return new()
        {
            Directory = path,
            Name = name,
            Seed = seed
        };
    }

    public static void SaveChunk(Chunk chunk)
    {
        string path = Path.Combine(WorldPath, "chunks", $"{chunk.X}_{chunk.Z}.chunk");

        using var fs = File.Create(path);
        using var compression = new DeflateStream(fs, CompressionMode.Compress);
        using var writer = new BinaryWriter(compression);

        writer.Write(chunk.Blocks.Count);

        foreach (var block in chunk.Blocks)
            writer.Write(block.Id);
    }

    public static Chunk LoadChunk(int x, int z)
    {
        string path = Path.Combine(WorldPath, "chunks", $"{x}_{z}.chunk");
        using var fs = File.OpenRead(path);
        using var compression = new DeflateStream(fs, CompressionMode.Decompress);
        using var reader = new BinaryReader(compression);

        int count = reader.ReadInt32();

        var chunk = new Chunk
        {
            X = x,
            Z = z,
            Blocks = [],
        };

        for (int i = 0; i < count; i++)
        {
            var id = reader.ReadUInt16();

            int blockX = i / (Chunk.Size * 417);
            int blockZ = (i / 417) % Chunk.Size;
            int y = i % 417;

            chunk.Blocks.Add(new(id, (byte)blockX, (ushort)y, (byte)blockZ));
        }

        return chunk;
    }

    public static byte[] LoadCompressedChunkBytes(int x, int z)
    {
        string path = Path.Combine(WorldPath, "chunks", $"{x}_{z}.chunk");

        using var fs = File.OpenRead(path);
        using var reader = new BinaryReader(fs);

        return reader.ReadBytes((int)fs.Length);
    }
}
