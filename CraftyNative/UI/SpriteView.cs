using System.Numerics;

namespace CraftyNative.UI;

/// <summary>
/// Displays an image (or a sub-rectangle of one) from raw RGBA8 pixel data.
/// The texture is created and cached by <see cref="UIRenderer"/> the first time it is drawn.
/// </summary>
public sealed class SpriteView : UIElement
{
    private Vector2 _size;

    /// <summary>RGBA8 pixels (4 bytes per pixel, straight alpha, top row first). Keep the same array instance
    /// across frames so the renderer's texture cache hits; call <see cref="UIRenderer.ReleaseTexture"/> after
    /// editing its contents.</summary>
    public byte[]? Pixels { get; set; }

    public uint ImageWidth { get; set; }

    public uint ImageHeight { get; set; }

    /// <summary>Multiplied with the texel color. White (default) = no tint; alpha fades the sprite.</summary>
    public Vector4 Tint { get; set; } = Vector4.One;

    /// <summary>Normalised (0..1, top-left origin) region of the image to draw. Defaults to the whole image;
    /// set these to draw one sprite out of an atlas.</summary>
    public Vector2 UvMin { get; set; } = Vector2.Zero;

    public Vector2 UvMax { get; set; } = Vector2.One;

    /// <summary>Size requested during Measure. Defaults to the image size (scaled by the UV region).</summary>
    public Vector2? PreferredSize { get; set; }

    /// <summary>Round the draw position to whole pixels so pixel art stays crisp.</summary>
    public bool SnapToPixels { get; set; } = true;

    public Vector2 Scale { get; set; } = Vector2.One;

    public override Vector2 Measure(Vector2 availableSize)
    {
        Vector2 desired = PreferredSize
            ?? new Vector2(ImageWidth * MathF.Abs(UvMax.X - UvMin.X), ImageHeight * MathF.Abs(UvMax.Y - UvMin.Y));

        return Vector2.Min(desired, availableSize);
    }

    public override void Arrange(Vector2 finalSize)
    {
        _size = finalSize;
    }

    public override void Update(Vector2 cursorPosition, Vector2 windowSize, bool mousePressed) { }

    public override void Draw()
    {
        if (Pixels is null || ImageWidth == 0 || ImageHeight == 0 || _size.X <= 0 || _size.Y <= 0)
            return;

        Vector2 position = SnapToPixels ? new(MathF.Round(Position.X), MathF.Round(Position.Y)) : Position;

        Vector2 scaledSize = _size * Scale;

        SpriteRenderer.Draw(Pixels, ImageWidth, ImageHeight, position, scaledSize, Tint, UvMin, UvMax);
    }
}