namespace Crafty.SDK.Client.Blocks;

public readonly record struct SelectionShape(IReadOnlyList<BoundingBox> Boxes)
{
    public static SelectionShape FullCube => new([BoundingBox.UnitCube]);
}
