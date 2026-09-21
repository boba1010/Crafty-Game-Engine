
namespace Crafty.SDK.Client.Blocks;

public readonly record struct CollisionShape(IReadOnlyList<BoundingBox> Boxes)
{
    public static CollisionShape FullCube => new([BoundingBox.UnitCube]);
}
