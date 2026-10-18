using CraftyNative.ThreeD.World;
using System.Collections.Concurrent;
using System.Numerics;

namespace CraftyNative.ThreeD.Meshes;

public static class WorldMeshManager
{
    private static readonly ConcurrentDictionary<SectionCoordinate, WorldMeshSection> _sections = new();
    private static readonly ConcurrentQueue<(int X, int Y, int Z)> _dirtyBlocks = new();
    private static readonly ConcurrentQueue<WorldMeshSection> _dirtySections = new();

    public static bool HasDirtyBlocks => !_dirtyBlocks.IsEmpty;
    public static float RenderDistance { get; set; } = 256f;

    public static void MarkBlockDirty(int x, int y, int z)
    {
        _dirtyBlocks.Enqueue((x, y, z));
    }

    public static bool TryDequeueDirtySection(out WorldMeshSection? section) => _dirtySections.TryDequeue(out section);

    public static void ProcessDirtyBlocks()
    {
        int sectionSize = SectionCoordinate.SectionSize;
        int sectionsPerChunk = SectionCoordinate.ChunkSize / sectionSize;

        while (_dirtyBlocks.TryDequeue(out var block))
        {
            int worldSectionX = Math.DivRem(block.X, sectionSize, out int localX);
            int worldSectionZ = Math.DivRem(block.Z, sectionSize, out int localZ);

            if (localX < 0)
            {
                localX += sectionSize;
                worldSectionX--;
            }

            if (localZ < 0)
            {
                localZ += sectionSize;
                worldSectionZ--;
            }

            int sectionY = block.Y / sectionSize;
            int chunkX = Math.DivRem(worldSectionX, sectionsPerChunk, out int sectionX);
            int chunkZ = Math.DivRem(worldSectionZ, sectionsPerChunk, out int sectionZ);

            if (sectionX < 0)
            {
                sectionX += sectionsPerChunk;
                chunkX--;
            }

            if (sectionZ < 0)
            {
                sectionZ += sectionsPerChunk;
                chunkZ--;
            }

            MarkSectionDirty(chunkX, chunkZ, sectionX, sectionY, sectionZ);

            if (localX == 0)
                MarkSectionDirty(chunkX, chunkZ, sectionX - 1, sectionY, sectionZ);
            else if (localX == sectionSize - 1)
                MarkSectionDirty(chunkX, chunkZ, sectionX + 1, sectionY, sectionZ);

            if (localZ == 0)
                MarkSectionDirty(chunkX, chunkZ, sectionX, sectionY, sectionZ - 1);
            else if (localZ == sectionSize - 1)
                MarkSectionDirty(chunkX, chunkZ, sectionX, sectionY, sectionZ + 1);

            int localY = block.Y % sectionSize;

            if (localY == 0 && sectionY > 0)
                MarkSectionDirty(chunkX, chunkZ, sectionX, sectionY - 1, sectionZ);
            else if (localY == sectionSize - 1 && sectionY < SectionCoordinate.SectionsY - 1)
                MarkSectionDirty(chunkX, chunkZ, sectionX, sectionY + 1, sectionZ);
        }
    }

    private static void MarkSectionDirty(int chunkX, int chunkZ, int sectionX, int sectionY, int sectionZ)
    {
        int sectionsPerChunk = SectionCoordinate.ChunkSize / SectionCoordinate.SectionSize;
        if (sectionY < 0 || sectionY >= SectionCoordinate.SectionsY)
            return;
        if (sectionX < 0)
        {
            sectionX += sectionsPerChunk;
            chunkX--;
        }
        else if (sectionX >= sectionsPerChunk)
        {
            sectionX -= sectionsPerChunk;
            chunkX++;
        }
        if (sectionZ < 0)
        {
            sectionZ += sectionsPerChunk;
            chunkZ--;
        }
        else if (sectionZ >= sectionsPerChunk)
        {
            sectionZ -= sectionsPerChunk;
            chunkZ++;
        }
        var coordinate = new SectionCoordinate(chunkX, chunkZ, sectionX, sectionY, sectionZ);
        if (!_sections.TryGetValue(coordinate, out var section) || section.IsDirty)
            return;
        section.MarkDirty();
        _dirtySections.Enqueue(section);
    }

    public static bool TryAdd(WorldMeshSection section)
    {
        return _sections.TryAdd(section.Coordinate, section);
    }

    public static bool RemoveSection(SectionCoordinate coordinate)
    {
        return _sections.Remove(coordinate, out _);
    }

    public static bool RemoveChunk(int x, int z)
    {
        bool removed = false;

        foreach (var coordinate in _sections.Keys)
        {
            if (coordinate.ChunkX != x || coordinate.ChunkZ != z)
                continue;

            removed |= _sections.TryRemove(coordinate, out _);
        }

        return removed;
    }

    public static bool TryGet(SectionCoordinate coordinate, out WorldMeshSection? section)
    {
        return _sections.TryGetValue(coordinate, out section);
    }

    private static WorldMeshSection?[] _visibleSectionsBuffer = new WorldMeshSection?[1024];
    public static int GetVisibleSections(Vector3 cameraPosition, Matrix4x4 viewProjection, out WorldMeshSection?[] visibleSections)
    {
        const int planeCount = 6;

        Span<Vector4> planes = stackalloc Vector4[planeCount];
        ExtractFrustumPlanes(viewProjection, planes);

        int sectionSize = SectionCoordinate.SectionSize;

        float renderDistanceBlocks = RenderDistance * SectionCoordinate.ChunkSize;

        float renderDistanceSquared = renderDistanceBlocks * renderDistanceBlocks;

        int sectionCount = _sections.Count;

        if (_visibleSectionsBuffer.Length < sectionCount)
            Array.Resize(ref _visibleSectionsBuffer, Math.Max(sectionCount, _visibleSectionsBuffer.Length * 2));

        int count = 0;

        foreach (var pair in _sections)
        {
            var section = pair.Value;
            var coordinate = pair.Key;

            Vector3 min = coordinate.WorldPosition;
            Vector3 max = min + new Vector3(sectionSize);

            // Preserve the existing spherical render-distance check,
            // using the section center.
            Vector3 center = (min + max) * 0.5f;
            Vector3 offset = center - cameraPosition;

            if (offset.LengthSquared() > renderDistanceSquared)
                continue;

            // Reject sections whose bounding boxes are outside
            // the camera's view frustum.
            if (!IsInsideFrustum(min, max, planes))
                continue;

            if (count == _visibleSectionsBuffer.Length)
                Array.Resize(ref _visibleSectionsBuffer, Math.Max(1024, _visibleSectionsBuffer.Length * 2));

            _visibleSectionsBuffer[count++] = section;
        }

        visibleSections = _visibleSectionsBuffer;
        return count;
    }

    private static bool IsInsideFrustum(Vector3 min, Vector3 max, ReadOnlySpan<Vector4> planes)
    {
        foreach (var plane in planes)
        {
            // Positive vertex: the AABB corner furthest along the
            // plane normal. If it is outside, the entire box is out.
            var positive = new Vector3(plane.X >= 0 ? max.X : min.X, plane.Y >= 0 ? max.Y : min.Y, plane.Z >= 0 ? max.Z : min.Z);

            if (Vector3.Dot(new(plane.X, plane.Y, plane.Z), positive) + plane.W < 0)
                return false;
        }

        return true;
    }

    private static void ExtractFrustumPlanes(Matrix4x4 m, Span<Vector4> planes)
    {
        // System.Numerics row-vector convention:
        // clip position = position * viewProjection.
        // System.Numerics perspective projection uses D3D-style
        // depth (0..1).

        planes[0] = new Vector4(
            m.M14 + m.M11,
            m.M24 + m.M21,
            m.M34 + m.M31,
            m.M44 + m.M41); // Left

        planes[1] = new Vector4(
            m.M14 - m.M11,
            m.M24 - m.M21,
            m.M34 - m.M31,
            m.M44 - m.M41); // Right

        planes[2] = new Vector4(
            m.M14 + m.M12,
            m.M24 + m.M22,
            m.M34 + m.M32,
            m.M44 + m.M42); // Bottom

        planes[3] = new Vector4(
            m.M14 - m.M12,
            m.M24 - m.M22,
            m.M34 - m.M32,
            m.M44 - m.M42); // Top

        planes[4] = new Vector4(
            m.M13,
            m.M23,
            m.M33,
            m.M43); // Near: z >= 0

        planes[5] = new Vector4(
            m.M14 - m.M13,
            m.M24 - m.M23,
            m.M34 - m.M33,
            m.M44 - m.M43); // Far: z <= w

        for (int i = 0; i < planes.Length; i++)
        {
            var plane = planes[i];
            float length = new Vector3(plane.X, plane.Y, plane.Z).Length();

            if (length > 0f)
                planes[i] = plane / length;
        }
    }

    public static void Clear()
    {
        _sections.Clear();
        while (_dirtyBlocks.TryDequeue(out _))
        {
        }
    }
}
