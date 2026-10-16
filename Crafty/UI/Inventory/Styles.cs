using System.Numerics;

namespace Crafty.UI.Inventory;

public static class Styles
{
    public const float SlotSize = 44f;
    public const float FontSize = 16f;

    public static readonly Vector4 Accent = new(0.25f, 0.65f, 1.00f, 1f);

    public static readonly Vector4 PanelBackground = new(0.08f, 0.08f, 0.09f, 1f);
    public static readonly Vector4 SlotBackground = new(0.12f, 0.12f, 0.14f, 1f);
    public static readonly Vector4 SlotHover = new(0.19f, 0.19f, 0.22f, 1f);

    public static readonly Vector4 TabSelected = PanelBackground;
    public static readonly Vector4 TabHover = new(0.14f, 0.14f, 0.16f, 1f);
    public static readonly Vector4 TabIdle = new(0.05f, 0.05f, 0.06f, 1f);

    public static readonly Vector4 Label = new(0.85f, 0.85f, 0.9f, 1f);
    public static readonly Vector4 White = new(1f, 1f, 1f, 1f);
    public static readonly Vector4 TextShadow = new(0.1f, 0.1f, 0.1f, 1f);

    public static readonly Vector4 TooltipBackground = new(0.06f, 0.05f, 0.10f, 0.96f);

    public static readonly Vector4 ScrollTrack = SlotBackground;
    public static readonly Vector4 ScrollHandle = new(0.35f, 0.35f, 0.38f, 1f);
}
