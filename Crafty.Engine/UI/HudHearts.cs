using CraftyNative.UI;
using System.Numerics;

namespace Crafty.Engine.UI;

public static class HudHearts
{
    public static bool IsVisible { get; set; } = true;

    private const int HeartsPerRow = 10;
    private const int HealthPerHeart = 2;

    private const uint ImageSize = 32;
    private const float StrideRatio = 20f / 32f;

    public static float HeartSize { get; set; } = 32f;

    private static float HeartStride => HeartSize * StrideRatio;

    private const float GapAboveHotbar = 0f;

    private static readonly SpriteView[] _views = new SpriteView[HeartsPerRow];

    private static byte[] _full = null!;
    private static byte[] _half = null!;
    private static byte[] _empty = null!;

    private static int _health = 20;
    private static int _maxHealth = 20;

    public static void Initialize(byte[] full, byte[] half, byte[] empty)
    {
        _full = full;
        _half = half;
        _empty = empty;

        for (int i = 0; i < _views.Length; i++)
        {
            _views[i] = new SpriteView
            {
                ImageWidth = ImageSize,
                ImageHeight = ImageSize,
                Pixels = _empty,
                Scale = new(0.6f, 0.6f)
            };
        }
    }

    public static void SetHealth(int health, int maxHealth = 20)
    {
        _maxHealth = Math.Max(0, maxHealth);
        _health = Math.Clamp(health, 0, _maxHealth);
    }

    public static void Render(Vector2 currentWindowSize)
    {
        if (!IsVisible)
            return;

        int heartCount = (_maxHealth + HealthPerHeart - 1) / HealthPerHeart;

        Vector2 hotbarTopLeft = HudHotbar.GetTopLeft(currentWindowSize);
        float baseY = hotbarTopLeft.Y - GapAboveHotbar - HeartSize;

        for (int i = 0; i < heartCount; i++)
        {
            int row = i / HeartsPerRow;
            int column = i % HeartsPerRow;

            int points = Math.Clamp(_health - i * HealthPerHeart, 0, HealthPerHeart);

            SpriteView view = _views[column];
            view.Pixels = points switch
            {
                2 => _full,
                1 => _half,
                _ => _empty
            };

            view.Position = new Vector2(hotbarTopLeft.X + column * HeartStride, baseY - row * HeartStride);
            view.Arrange(new Vector2(HeartSize, HeartSize));
            view.Draw();
        }
    }
}