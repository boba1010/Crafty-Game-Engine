namespace CraftyNative.UI;

public struct Thickness(float left, float top, float right, float bottom)
{
    public float Left { get; set; } = left;
    public float Top { get; set; } = top;
    public float Right { get; set; } = right;
    public float Bottom { get; set; } = bottom;

    public Thickness(float uniform) : this(uniform, uniform, uniform, uniform) { }

    public Thickness(float horizontal, float vertical) : this(horizontal, vertical, horizontal, vertical) { }

    public readonly float Horizontal => Left + Right;
    public readonly float Vertical => Top + Bottom;
}