using CraftyNative.ThreeD.Meshes;

namespace CraftyNative.ThreeD.World;

public readonly record struct SectionMeshes(Mesh Opaque, Mesh Translucent);

public sealed class WorldMeshSection(SectionCoordinate coordinate, SectionMeshes meshes, BoundingBox? bounds = null)
{
    public SectionCoordinate Coordinate { get; } = coordinate;
    public BoundingBox Bounds => bounds ?? Coordinate.Bounds;

    private SectionMeshes _meshes = meshes;

    public Mesh Mesh => _meshes.Opaque;
    public Mesh TranslucentMesh => _meshes.Translucent;

    public bool IsDirty { get; private set; }

    public void SetMesh(SectionMeshes meshes)
    {
        var old = _meshes;

        _meshes = meshes;

        CraftyNative.ReleaseMesh(old.Opaque);
        CraftyNative.ReleaseMesh(old.Translucent);

        IsDirty = false;
    }

    public void MarkDirty()
    {
        IsDirty = true;
    }
}
