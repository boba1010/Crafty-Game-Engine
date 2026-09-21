namespace Crafty.SDK.Client.Blocks;

public readonly record struct BoundingBox(
    float MinX, float MinY, float MinZ, 
    float MaxX, float MaxY, float MaxZ)
{
    public static BoundingBox UnitCube => new(0, 0, 0, 1, 1, 1);
}
