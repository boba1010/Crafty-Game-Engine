using System.Collections.Concurrent;
using System.Numerics;

namespace CraftyNative.ThreeD.World;

public static class WorldMeshManager
{
    public sealed class ChunkMeshSet
    {
        public WorldMeshSection[] Sections = [];
        public int Version;
    }

    private static readonly ConcurrentDictionary<SectionCoordinate, WorldMeshSection> _sections = new();
    private static readonly ConcurrentQueue<(int X, int Y, int Z)> _dirtyBlocks = new();
    private static readonly ConcurrentQueue<WorldMeshSection> _dirtySections = new();
    private static readonly ConcurrentDictionary<(int X, int Z), ChunkMeshSet> _chunkSets = new();
    private static readonly ConcurrentQueue<(int X, int Z)> _removedChunks = new();
    private static readonly ConcurrentQueue<SectionCoordinate> _missingSections = new();
    private static readonly ConcurrentDictionary<SectionCoordinate, byte> _missingQueued = new();

    public static IEnumerable<KeyValuePair<(int X, int Z), ChunkMeshSet>> Chunks => _chunkSets;
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
        if (!_sections.TryGetValue(coordinate, out var section))
        {
            if (_missingQueued.TryAdd(coordinate, 0))
                _missingSections.Enqueue(coordinate);
            return;
        }

        if (section.IsDirty)
            return;

        section.MarkDirty();
        _dirtySections.Enqueue(section);
    }

    public static bool TryAdd(WorldMeshSection section)
    {
        return _sections.TryAdd(section.Coordinate, section);
    }

    public static void AddOrReplace(WorldMeshSection section) => _sections[section.Coordinate] = section;

    public static bool RemoveSection(SectionCoordinate coordinate)
    {
        return _sections.Remove(coordinate, out _);
    }

    public static void UpdateSection(WorldMeshSection section)
    {
        _sections[section.Coordinate] = section;

        if (!_chunkSets.TryGetValue((section.Coordinate.ChunkX, section.Coordinate.ChunkZ), out var set))
            return;

        lock (set)
        {
            var list = new List<WorldMeshSection>(set.Sections);
            int index = list.FindIndex(s => s.Coordinate.Equals(section.Coordinate));

            if (index >= 0)
                list[index] = section;
            else
                list.Add(section);

            set.Sections = [.. list];
        }

        Interlocked.Increment(ref set.Version);
    }

    public static bool RemoveChunk(int x, int z)
    {
        if (!_chunkSets.TryRemove((x, z), out var set))
            return false;

        foreach (var section in set.Sections)
            _sections.TryRemove(section.Coordinate, out _);

        _removedChunks.Enqueue((x, z));
        return true;
    }

    public static bool TryGet(SectionCoordinate coordinate, out WorldMeshSection? section)
    {
        return _sections.TryGetValue(coordinate, out section);
    }

    public static bool HasChunk(int x, int z) => _chunkSets.ContainsKey((x, z));

    public static bool TryDequeueMissingSection(out SectionCoordinate coordinate)
    {
        if (_missingSections.TryDequeue(out coordinate))
        {
            _missingQueued.TryRemove(coordinate, out _);
            return true;
        }

        return false;
    }

    public static bool TryDequeueRemovedChunk(out (int X, int Z) key) => _removedChunks.TryDequeue(out key);

    public static void PublishChunk(int x, int z, WorldMeshSection[] sections)
    {
        var set = _chunkSets.GetOrAdd((x, z), static _ => new ChunkMeshSet());
        set.Sections = sections;
        Interlocked.Increment(ref set.Version);
    }

    public static void BumpChunk(int x, int z)
    {
        if (_chunkSets.TryGetValue((x, z), out var set))
            Interlocked.Increment(ref set.Version);
    }

    private static WorldMeshSection?[] _visibleSectionsBuffer = new WorldMeshSection?[1024];
    public static int GetVisibleSections(Vector3 cameraPosition, out WorldMeshSection?[] visibleSections)
    {
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

            Vector3 center = (min + max) * 0.5f;
            Vector3 offset = center - cameraPosition;

            if (offset.LengthSquared() > renderDistanceSquared)
                continue;

            _visibleSectionsBuffer[count++] = section;
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
