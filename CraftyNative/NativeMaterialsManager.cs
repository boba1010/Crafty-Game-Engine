using CraftyNative.ThreeD;

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

    public static void Clear()
    {
        foreach (var material in _materials.Values)
            material.Dispose();

        _materials.Clear();
    }
}