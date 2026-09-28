using Vulcan.Graphics;

namespace CraftyNative.ThreeD;

public sealed class Texture : IDisposable
{
    internal ITexture Resource { get; }

    internal Texture(ITexture resource)
    {
        Resource = resource;
    }

    public static Texture Create(ImageData image)
    {
        return CraftyNative3D.CreateTexture(image);
    }

    public void Dispose()
    {
        Resource.Dispose();
    }
}