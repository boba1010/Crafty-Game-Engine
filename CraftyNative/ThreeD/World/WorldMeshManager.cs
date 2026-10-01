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
    public static int GetVisibleSections(Vector3 cameraPosition, out WorldMeshSection?[] visibleSections)
    {
        int sectionSize = SectionCoordinate.SectionSize;
        int chunkSize = SectionCoordinate.ChunkSize;
        int sectionsPerChunk = chunkSize / sectionSize;
        int radius = (int)MathF.Ceiling(RenderDistance / sectionSize);

        int cameraWorldSectionX = (int)MathF.Floor(cameraPosition.X / sectionSize);
        int cameraSectionY = (int)MathF.Floor(cameraPosition.Y / sectionSize);
        int cameraWorldSectionZ = (int)MathF.Floor(cameraPosition.Z / sectionSize);

        float renderDistanceSquared = RenderDistance * RenderDistance;
        float halfSection = sectionSize * 0.5f;
        int diameter = radius * 2 + 1;
        int maxCapacity = diameter * diameter * SectionCoordinate.SectionsY;

        if (_visibleSectionsBuffer.Length < maxCapacity)
            Array.Resize(ref _visibleSectionsBuffer, maxCapacity);

        int count = 0;

        for (int x = -radius; x <= radius; x++)
        {
            int worldSectionX = cameraWorldSectionX + x;
            float centerX = worldSectionX * sectionSize + halfSection;
            float dx = centerX - cameraPosition.X;
            float dxSquared = dx * dx;

            if (dxSquared > renderDistanceSquared)
                continue;

            for (int z = -radius; z <= radius; z++)
            {
                int worldSectionZ = cameraWorldSectionZ + z;
                float centerZ = worldSectionZ * sectionSize + halfSection;
                float dz = centerZ - cameraPosition.Z;
                float horizontalDistanceSquared = dxSquared + dz * dz;

                if (horizontalDistanceSquared > renderDistanceSquared)
                    continue;

                float verticalRadius = MathF.Sqrt(
                    renderDistanceSquared - horizontalDistanceSquared);

                int minY = Math.Max(
                    0,
                    (int)MathF.Floor(
                        (cameraPosition.Y - verticalRadius) / sectionSize));

                int maxY = Math.Min(
                    SectionCoordinate.SectionsY - 1,
                    (int)MathF.Ceiling(
                        (cameraPosition.Y + verticalRadius) / sectionSize));

                int chunkX = Math.DivRem(
                    worldSectionX,
                    sectionsPerChunk,
                    out int localSectionX);

                int chunkZ = Math.DivRem(
                    worldSectionZ,
                    sectionsPerChunk,
                    out int localSectionZ);

                if (localSectionX < 0)
                {
                    localSectionX += sectionsPerChunk;
                    chunkX--;
                }

                if (localSectionZ < 0)
                {
                    localSectionZ += sectionsPerChunk;
                    chunkZ--;
                }

                for (int sectionY = minY; sectionY <= maxY; sectionY++)
                {
                    var coordinate = new SectionCoordinate(
                        chunkX,
                        chunkZ,
                        localSectionX,
                        sectionY,
                        localSectionZ);

                    if (!_sections.TryGetValue(coordinate, out var section))
                        continue;

                    _visibleSectionsBuffer[count++] = section;
                }
            }
        }

        visibleSections = _visibleSectionsBuffer;
        return count;
    }

    public static void Clear()
    {
        _sections.Clear();
        while (_dirtyBlocks.TryDequeue(out _))
        {
        }
    }
}
