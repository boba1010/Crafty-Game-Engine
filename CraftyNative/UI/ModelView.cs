using CraftyNative.ThreeD.Meshes;
using System.Numerics;

namespace CraftyNative.UI;

public sealed class ModelView : UIElement
{
    public Mesh? Model { get; set; }
    public Vector3 Rotation { get; set; } = Vector3.Zero;
    public Vector3 Scale { get; set; } = Vector3.One;

    public override Vector2 Measure(Vector2 availableSize)
    {
        return Vector2.Min(Vector2.Max(Size, Vector2.Zero), Vector2.Max(availableSize, Vector2.Zero));
    }

    public override void Arrange(Vector2 finalSize)
    {
        ActualSize = Vector2.Max(finalSize, Vector2.Zero);
    }

    public override void Update(Vector2 cursorPosition, Vector2 windowSize, bool mousePressed) { }

    public override void Draw()
    {
        if (!IsVisible || ActualSize.X <= 0 || ActualSize.Y <= 0)
            return;

        if (Model is not null)
            UIRenderer.DrawMesh(Model, AbsolutePosition, Rotation, Scale, ActualSize);
    }
}
