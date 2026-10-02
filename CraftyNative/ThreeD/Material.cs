namespace CraftyNative.ThreeD;

public sealed class Material : IDisposable
{
    public Texture Atlas { get; init; } = null!;

    public static Material Create(ImageData atlas)
    {
        return CraftyNative.CreateMaterial(atlas);
    }

    public void Dispose()
    {
        Atlas.Dispose();
    }
}