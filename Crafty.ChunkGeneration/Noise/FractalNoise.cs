namespace Crafty.ChunkGeneration.Noise;

internal sealed class FractalNoise(ulong seed)
{
    private readonly ValueNoise _noise = new(seed);

    public double Sample(
        double x,
        double z,
        int octaves = 4,
        double frequency = 1.0,
        double amplitude = 1.0,
        double lacunarity = 2.0,
        double persistence = 0.5)
    {
        double value = 0;
        double amplitudeSum = 0;

        for (int i = 0; i < octaves; i++)
        {
            value += _noise.Sample(x * frequency, z * frequency) * amplitude;
            amplitudeSum += amplitude;

            frequency *= lacunarity;
            amplitude *= persistence;
        }

        return value / amplitudeSum;
    }
}