using CraftyNative.ThreeD.World;
using System.Numerics;
namespace CraftyNative.ThreeD.Meshes;

public static class WorldMeshManager
{
    private static readonly Dictionary<SectionCoordinate, WorldMeshSection> _sections = [];

    public static float RenderDistance { get; set; } = 256f;
    public static IReadOnlyCollection<WorldMeshSection> Sections => _sections.Values;

    public static void Add(WorldMeshSection section)
    {
        _sections.Add(section.Coordinate, section);
    }

    public static bool Remove(SectionCoordinate coordinate)
    {
        return _sections.Remove(coordinate);
    }

    public static bool TryGet(SectionCoordinate coordinate, out WorldMeshSection? section)
    {
        return _sections.TryGetValue(coordinate, out section);
    }

    public static IEnumerable<WorldMeshSection> GetVisibleSections(Vector3 cameraPosition)
    {
        float renderDistanceSquared = RenderDistance * RenderDistance;

        foreach (var section in _sections.Values)
        {
            Vector3 delta = section.Bounds.Center - cameraPosition;

            if (delta.LengthSquared() > renderDistanceSquared)
                continue;

            yield return section;
        }
    }

    public static void Clear()
    {
        _sections.Clear();
    }

    public static void Dispose()
    {
        foreach (var section in _sections.Values)
            section?.Dispose();
    }
}
