namespace CraftyNative.ThreeD;

public static class AssetRegistry
{
    private static readonly Dictionary<string, string> _assets = [];

    public static void Scan(string assetsDirectory)
    {
        foreach (var path in Directory.EnumerateFiles(assetsDirectory, "*", SearchOption.AllDirectories))
        {
            var name = Path.GetFileNameWithoutExtension(path);

            if (_assets.ContainsKey(name))
                throw new InvalidOperationException($"Duplicate asset name: {name}");

            _assets[name] = path;
        }
    }

    public static string Get(string name)
    {
        return _assets[name];
    }
}
