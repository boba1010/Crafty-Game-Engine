using System.Security.Cryptography;

namespace Crafty.ChunkGeneration.World;

public static class WorldSeed
{
    public static ulong Generate()
    {
        byte[] bytes = new byte[8];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        ulong seed = BitConverter.ToUInt64(bytes);
        return seed;
    }
}