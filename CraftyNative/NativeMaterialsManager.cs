using CraftyNative.ThreeD;
using Vulcan.Graphics;
using Vulcan.Graphics.Descriptions;

namespace CraftyNative;

public static class NativeMaterialsManager
{
    private static readonly Dictionary<object, Material> _materials = new(ReferenceEqualityComparer.Instance);

    public static Material Get(ImageData atlas)
    {
        var key = atlas.Pixels;

        if (_materials.TryGetValue(key, out var cached))
            return cached;

        var material = Material.Create(atlas);
        _materials[key] = material;

        return material;
    }

    // Call when an atlas is reloaded or replaced
    public static void Invalidate(ImageData atlas)
    {
        if (_materials.Remove(atlas.Pixels, out var old))
            old.Dispose();
    }

    internal static Texture CreateTexture(ImageData image)
    {
        var texture = Rendering.CraftyNative.Device.CreateTexture(new TextureDescription
        {
            Width = image.Width,
            Height = image.Height,
            Depth = 1,
            MipLevels = 1,
            ArrayLayers = 1,
            Format = TextureFormat.R8G8B8A8Unorm,
            Usage = TextureUsage.Sampled,
            Type = TextureType.Texture2D,
            Samples = SampleCount.X1
        });

        var rowPitch = image.Width * 4;

        texture.Upload(image.Pixels, rowPitch);

        return new Texture(texture);
    }

    internal static Material CreateMaterial(ImageData image)
    {
        return new Material
        {
            Atlas = CreateTexture(image)
        };
    }


    public static void Clear()
    {
        foreach (var material in _materials.Values)
            material.Dispose();

        _materials.Clear();
    }
}