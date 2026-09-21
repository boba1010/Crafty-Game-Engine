using Crafty.ChunkGeneration.World;
using System.IO.Compression;

namespace Crafty.ChunkGeneration.IO;

public struct WorldIOManager(string worldPath)
{
    public void SaveWorld(World.World world)
    {
        var path = Path.Combine(worldPath, $"{world.Name}.world");

        var fs = File.Open(path, FileMode.CreateNew);
        using var compression = new DeflateStream(fs, CompressionMode.Compress);
        using var writer = new BinaryWriter(compression);

        writer.Write(world.Name);
        writer.Write(world.Seed);
    }

    public World.World LoadWorld(string path)
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

    public void SaveChunk(Chunk chunk)
    {
        string path = Path.Combine(worldPath, "chunks", $"{chunk.X}_{chunk.Z}.chunk");

        using var fs = File.Create(path);
        using var compression = new DeflateStream(fs, CompressionMode.Compress);
        using var writer = new BinaryWriter(compression);

        //writer.Write(chunk.X);
        //writer.Write(chunk.Z);
        writer.Write(chunk.Blocks.Count);

        foreach (var block in chunk.Blocks)
        {
            writer.Write(block.Id);
            //writer.Write(block.X);
            //writer.Write(block.Z);
            //writer.Write(block.Y);
        }
    }

    public Chunk LoadChunk(int x, int z)
    {
        string path = Path.Combine(worldPath, "chunks", $"{x}_{z}.chunk");
        using var fs = File.OpenRead(path);
        using var compression = new DeflateStream(fs, CompressionMode.Decompress);
        using var reader = new BinaryReader(compression);

        //int chunkX = reader.ReadInt32();
        //int chunkZ = reader.ReadInt32();
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
            //var blockX = reader.ReadByte();
            //var y = reader.ReadUInt16();
            //var blockZ = reader.ReadByte();

            int blockX = i / (Chunk.Size * 401);
            int blockZ = (i / 401) % Chunk.Size;
            int y = i % 401;

            chunk.Blocks.Add(new(id, (byte)blockX, (ushort)y, (byte)blockZ));
        }

        return chunk;
    }
}
