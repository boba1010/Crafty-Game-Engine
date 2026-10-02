namespace CraftyNative.ThreeD;

public static class TextureRegistry
{
    private static readonly Dictionary<string, Texture> _textures = [];

    public static Texture Get(string name)
    {
        if (_textures.TryGetValue(name, out var texture))
            return texture;

        var path = AssetRegistry.Get(name);
        var image = TextureLoader.Load(path);
        texture = CraftyNative.CreateTexture(image);

        _textures.Add(name, texture);
        return texture;
    }

    public static void Dispose()
    {
        foreach (var texture in _textures.Values)
            texture.Dispose();
    }
}
