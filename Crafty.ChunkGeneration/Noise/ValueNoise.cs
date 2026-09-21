namespace Crafty.ChunkGeneration.Noise;

internal sealed class ValueNoise(ulong seed)
{
    private readonly ulong _seed = seed;

    public double Sample(double x, double z)
    {
        int x0 = (int)Math.Floor(x);
        int z0 = (int)Math.Floor(z);

        int x1 = x0 + 1;
        int z1 = z0 + 1;

        double tx = x - x0;
        double tz = z - z0;

        double v00 = RandomValue(x0, z0);
        double v10 = RandomValue(x1, z0);
        double v01 = RandomValue(x0, z1);
        double v11 = RandomValue(x1, z1);

        tx = SmoothStep(tx);
        tz = SmoothStep(tz);

        double a = Lerp(v00, v10, tx);
        double b = Lerp(v01, v11, tx);

        return Lerp(a, b, tz);
    }

    private double RandomValue(int x, int z)
    {
        ulong hash = _seed;

        hash ^= (ulong)(long)x * 0x9E3779B97F4A7C15UL;
        hash ^= (ulong)(long)z * 0xC2B2AE3D27D4EB4FUL;

        hash ^= hash >> 30;
        hash *= 0xBF58476D1CE4E5B9UL;
        hash ^= hash >> 27;
        hash *= 0x94D049BB133111EBUL;
        hash ^= hash >> 31;

        return hash / (double)ulong.MaxValue * 2.0 - 1.0;
    }

    private static double Lerp(double a, double b, double t) => a + (b - a) * t;

    private static double SmoothStep(double t) => t * t * (3.0 - 2.0 * t);
}