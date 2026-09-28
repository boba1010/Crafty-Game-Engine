using StbImageSharp;

namespace CraftyNative.ThreeD;

public readonly record struct ImageData(byte[] Pixels, uint Width, uint Height);

public static class TextureLoader
{
    public static ImageData Load(string path)
    {
        using var stream = File.OpenRead(path);

        var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

        return new(image.Data, (uint)image.Width, (uint)image.Height);
    }
}
