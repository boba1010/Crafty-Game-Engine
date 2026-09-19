using CraftyNative.ThreeD.Meshes;

namespace CraftyNative.ThreeD;

public readonly struct Renderable(Mesh mesh)
{
    public Mesh Mesh { get; } = mesh;
}
