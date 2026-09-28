using Crafty.ChunkGeneration.World;
using System.IO.Compression;

namespace Crafty.Engine.Helpers;

public static class ChunkByteTransformer
{
    public static Chunk FromBytes(int x, int z, byte[] data)
    {
        using var stream = new MemoryStream(data);
        using var compression = new DeflateStream(stream, CompressionMode.Decompress);
        using var reader = new BinaryReader(compression);

        var chunk = new Chunk()
        {
            X = x,
            Z = z,
            Blocks = [],
        };

        int count = reader.ReadInt32();

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
}
