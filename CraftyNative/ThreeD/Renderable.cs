using CraftyNative.ThreeD.Meshes;

namespace CraftyNative.ThreeD;

public struct Renderable(Mesh mesh, bool shouldRender)
{
    public Mesh Mesh { get; } = mesh;
    public bool ShouldRender { get; set; } = shouldRender;
}
