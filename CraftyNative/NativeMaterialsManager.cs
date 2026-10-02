using CraftyNative.ThreeD;

namespace CraftyNative;

public static class NativeMaterialsManager
{
    private static readonly Dictionary<string, (ImageData Atlas, Material Material)> _materials = [];

    public static Material Get(string id, ImageData atlas)
    {
        if (_materials.TryGetValue(id, out var cached) &&
            ReferenceEquals(cached.Atlas.Pixels, atlas.Pixels))
            return cached.Material;

        if (_materials.Remove(id, out var old))
            old.Material.Dispose();

        var material = Material.Create(atlas);
        _materials[id] = (atlas, material);

        return material;
    }
}
