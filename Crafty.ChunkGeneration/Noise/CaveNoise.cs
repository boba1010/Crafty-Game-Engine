using System;
using System.Collections.Generic;
using System.Text;

namespace Crafty.ChunkGeneration.Noise;

internal sealed class CaveNoise(ulong seed)
{
    private readonly ulong _seed = seed;

    public double Sample(double x, double y, double z)
    {
        int xi = (int)Math.Floor(x);
        int yi = (int)Math.Floor(y);
        int zi = (int)Math.Floor(z);

        ulong hash = _seed;

        hash ^= (ulong)(long)xi * 0x9E3779B97F4A7C15UL;
        hash ^= (ulong)(long)yi * 0xC2B2AE3D27D4EB4FUL;
        hash ^= (ulong)(long)zi * 0x165667B19E3779F9UL;

        hash ^= hash >> 30;
        hash *= 0xBF58476D1CE4E5B9UL;
        hash ^= hash >> 27;
        hash *= 0x94D049BB133111EBUL;
        hash ^= hash >> 31;

        return hash / (double)ulong.MaxValue * 2.0 - 1.0;
    }
}
