using CraftyNative.ThreeD;

namespace CraftyNative;

public static class NativeMaterialsManager
{
    private static readonly Dictionary<string, Material> _materials = [];

    public static Material Get(string id, ImageData atlas)
    {
        if (_materials.TryGetValue(id, out var material))
            return material;

        material = Material.Create(atlas);
        _materials.Add(id, material);

        return material;
    }
}
