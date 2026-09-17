namespace CraftyNative3D;

public readonly struct Renderable(Mesh mesh)
{
    public Mesh Mesh { get; } = mesh;
}
