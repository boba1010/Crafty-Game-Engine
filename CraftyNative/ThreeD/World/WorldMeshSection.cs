using CraftyNative.ThreeD.Meshes;

namespace CraftyNative.ThreeD.World;

public sealed class WorldMeshSection(SectionCoordinate coordinate, Mesh mesh, BoundingBox? bounds = null) : IDisposable
{
    public SectionCoordinate Coordinate { get; } = coordinate;

    public BoundingBox Bounds => bounds ?? Coordinate.Bounds;

    public Mesh Mesh { get; private set; } = mesh;

    public bool IsDirty { get; private set; }

    public void SetMesh(Mesh mesh)
    {
        Mesh = mesh;
        IsDirty = false;
    }

    public void MarkDirty()
    {
        IsDirty = true;
    }

    public void Dispose()
    {
        Mesh?.Dispose();
    }
}
