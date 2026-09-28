using System.Numerics;

namespace CraftyNative.ThreeD.World;

public readonly record struct SectionCoordinate(int ChunkX, int ChunkZ, int SectionX, int SectionY, int SectionZ)
{
    public const int SectionSize = 16;
    public const int WorldHeight = 416;
    public const int ChunkSize = 32;
    public const int SectionsY = WorldHeight / SectionSize;

    public int WorldX => ChunkX * ChunkSize + SectionX * SectionSize;
    public int WorldY => SectionY * SectionSize;
    public int WorldZ => ChunkZ * ChunkSize + SectionZ * SectionSize;
    public Vector3 WorldPosition => new(WorldX, WorldY, WorldZ);
    public BoundingBox Bounds => new(WorldPosition, WorldPosition + new Vector3(SectionSize));
}
