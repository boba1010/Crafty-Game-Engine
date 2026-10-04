using System.Numerics;
using System.Runtime.InteropServices;
using Vulcan;
using Vulcan.Graphics;

namespace CraftyNative.HUD;

/// <summary>
/// Stack counts drawn with a tiny 3x5 pixel font made of rectangles (no font renderer needed).
/// Use one instance per HUD layer: each owns its own vertex buffer, so two layers never overwrite each other.
/// Usage per frame, after the layer's meshes are drawn: Begin -> AddCount (any number of times) -> Flush.
/// </summary>
public sealed class HudDigits
{
    private const int FloatsPerVertex = 6;          // x, y, r, g, b, a
    private const float PixelSize = 2f;             // screen pixels per font pixel
    private const float Advance = 4f * PixelSize;   // 3 columns + 1 column spacing
    private const int FloatsPerNumber = 3 * 15 * 2 * 36;   // 3 digits * 15 cells * (shadow + fill) * 36 floats per rect
    private const int Capacity = 64 * FloatsPerNumber;     // up to 64 numbers per frame

    // 3 columns x 5 rows per glyph, row-major, '1' = filled
    private static readonly string[] Glyphs =
    [
        "111101101101111", // 0
        "010110010010111", // 1
        "111001111100111", // 2
        "111001111001111", // 3
        "101101111001001", // 4
        "111100111001111", // 5
        "111100111101111", // 6
        "111001001010010", // 7
        "111101111101111", // 8
        "111101111001111", // 9
    ];

    private static readonly Vector4 Fill = new(0.95f, 0.95f, 0.95f, 1f);
    private static readonly Vector4 Shadow = new(0.10f, 0.10f, 0.12f, 1f);

    private IBuffer _buffer = null!;
    private readonly List<float> _list = new(4096);
    private float[] _uploaded = [];
    private Vector2 _size;

    public void Initialize(IGraphicsDevice device)
    {
        _buffer = device.CreateBuffer(new()
        {
            Size = (ulong)(Capacity * sizeof(float)),
            Usage = BufferUsage.Vertex,
            MemoryUsage = MemoryUsage.Upload
        });
    }

    public void Begin(Vector2 screenSize)
    {
        _size = screenSize;
        _list.Clear();
    }

    /// <summary>Right/bottom edge of the number in pixels (origin top-left). Counts of 1 or less draw nothing.</summary>
    public void AddCount(int value, float right, float bottom)
    {
        if (value <= 1 || _list.Count + FloatsPerNumber > Capacity)
            return;

        Span<int> digits = stackalloc int[3];
        int n = 0;

        for (int v = Math.Min(value, 999); v > 0; v /= 10)
            digits[n++] = v % 10;

        float x = right - (n * Advance - PixelSize);
        float y = bottom - 5f * PixelSize;

        for (int k = n - 1; k >= 0; k--, x += Advance)
            Glyph(digits[k], x, y);
    }

    public void Flush(ICommandBuffer commandBuffer, IPipeline pipeline)
    {
        if (_list.Count == 0)
            return;

        var data = CollectionsMarshal.AsSpan(_list);

        // Only touch the GPU buffer when the digits actually changed
        if (!data.SequenceEqual(_uploaded))
        {
            _buffer.Upload(MemoryMarshal.AsBytes(data));
            _uploaded = data.ToArray();
        }

        commandBuffer.SetViewport(new Viewport
        {
            X = 0,
            Y = 0,
            Width = _size.X,
            Height = _size.Y,
            MinDepth = 0,
            MaxDepth = 1
        });

        commandBuffer.SetPipeline(pipeline);
        commandBuffer.SetVertexBuffer(_buffer, sizeof(float) * FloatsPerVertex);
        commandBuffer.Draw((uint)(data.Length / FloatsPerVertex));
    }

    public void Dispose()
    {
        _buffer?.Dispose();
    }

    private void Glyph(int digit, float x, float y)
    {
        var g = Glyphs[digit];

        // Row-major, so each cell's fill covers the shadows of the cells before it
        for (int i = 0; i < 15; i++)
        {
            if (g[i] != '1')
                continue;

            float px = x + (i % 3) * PixelSize;
            float py = y + (i / 3) * PixelSize;

            Rect(px + 1f, py + 1f, PixelSize, PixelSize, Shadow);
            Rect(px, py, PixelSize, PixelSize, Fill);
        }
    }

    private void Rect(float x, float y, float w, float h, Vector4 c)
    {
        float x0 = x / _size.X * 2f - 1f;
        float x1 = (x + w) / _size.X * 2f - 1f;
        float y0 = 1f - y / _size.Y * 2f;
        float y1 = 1f - (y + h) / _size.Y * 2f;

        V(x0, y0, c); V(x1, y0, c); V(x1, y1, c);
        V(x0, y0, c); V(x1, y1, c); V(x0, y1, c);
    }

    private void V(float x, float y, Vector4 c)
    {
        _list.Add(x);
        _list.Add(y);
        _list.Add(c.X);
        _list.Add(c.Y);
        _list.Add(c.Z);
        _list.Add(c.W);
    }
}