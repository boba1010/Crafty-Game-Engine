using CraftyNative.ThreeD;

namespace Crafty.Engine.ChunkBuilding;

public readonly record struct AtlasRegion(float U0, float V0, float U1, float V1);

public sealed class ChunkTextureAtlas
{
    private readonly Dictionary<string, AtlasRegion> _regions;

    public ImageData Image { get; }

    public ChunkTextureAtlas(ImageData image, Dictionary<string, AtlasRegion> regions)
    {
        Image = image;
        _regions = regions;
    }

    public AtlasRegion Get(string texture)
    {
        return _regions[texture];
    }
}

public static class ChunkTextureAtlasBuilder
{
    public static ChunkTextureAtlas Build(IReadOnlyCollection<string> textures, uint tileSize)
    {
        var textureCount = textures.Count;

        if (textureCount == 0)
            throw new InvalidOperationException("Cannot build an empty texture atlas.");

        var columns = (int)Math.Ceiling(Math.Sqrt(textureCount));
        var rows = (int)Math.Ceiling(textureCount / (double)columns);

        var atlasWidth = (uint)(columns * tileSize);
        var atlasHeight = (uint)(rows * tileSize);

        var atlasPixels = new byte[atlasWidth * atlasHeight * 4];

        var regions = new Dictionary<string, AtlasRegion>();

        var index = 0;

        foreach (var textureName in textures)
        {
            var image = TextureLoader.Load(AssetRegistry.Get(textureName));

            if (image.Width != tileSize || image.Height != tileSize)
                throw new InvalidOperationException($"Texture '{textureName}' must be {tileSize}x{tileSize}.");

            var column = index % columns;
            var row = index / columns;

            var x = column * (int)tileSize;
            var y = row * (int)tileSize;

            for (var py = 0; py < tileSize; py++)
            {
                var sourceOffset = (int)(py * image.Width * 4);
                var destinationOffset = ((y + (int)py) * (int)atlasWidth + x) * 4;

                image.Pixels.AsSpan(sourceOffset, (int)(tileSize * 4))
                    .CopyTo(atlasPixels.AsSpan(destinationOffset, (int)(tileSize * 4)));
            }

            regions[textureName] = new AtlasRegion(
                x / (float)atlasWidth,
                y / (float)atlasHeight,
                (x + tileSize) / (float)atlasWidth,
                (y + tileSize) / (float)atlasHeight);

            index++;
        }

        return new ChunkTextureAtlas(new ImageData(atlasPixels, atlasWidth, atlasHeight), regions);
    }
}