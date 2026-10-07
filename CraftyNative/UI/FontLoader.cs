using StbTrueTypeSharp;
using System.Numerics;
using Vulcan;
using Vulcan.Graphics;

namespace CraftyNative.UI;

public unsafe static class FontLoader
{
    public static UIFont Load(IGraphicsDevice device, string path, int pixelSize)
    {
        byte[] fontData = File.ReadAllBytes(path);
        var font = new StbTrueType.stbtt_fontinfo();

        fixed (byte* data = fontData)
        {
            if (StbTrueType.stbtt_InitFont(font, data, 0) == 0)
                throw new InvalidOperationException($"Failed to load font: {path}");

            float scale = StbTrueType.stbtt_ScaleForPixelHeight(font, pixelSize);

            int ascent;
            int descent;
            int lineGap;

            StbTrueType.stbtt_GetFontVMetrics(font, &ascent, &descent, &lineGap);

            const int firstCharacter = 32;
            const int lastCharacter = 126;

            int atlasWidth = 512;
            int atlasHeight = 512;

            byte[] atlas = new byte[atlasWidth * atlasHeight];

            var glyphs = new Dictionary<char, Glyph>();

            int cursorX = 0;
            int cursorY = 0;
            int rowHeight = 0;

            for (int codepoint = firstCharacter; codepoint <= lastCharacter; codepoint++)
            {
                char character = (char)codepoint;

                int advanceWidth;
                int leftSideBearing;

                StbTrueType.stbtt_GetCodepointHMetrics(font, codepoint, &advanceWidth, &leftSideBearing);

                int x0;
                int y0;
                int x1;
                int y1;

                StbTrueType.stbtt_GetCodepointBitmapBox(font, codepoint, scale, scale, &x0, &y0, &x1, &y1);

                int width = x1 - x0;
                int height = y1 - y0;

                if (width == 0 || height == 0)
                {
                    glyphs[character] = new Glyph(Vector2.Zero, Vector2.Zero, Vector2.Zero, new Vector2(x0, y0), advanceWidth * scale);
                    continue;
                }

                if (cursorX + width > atlasWidth)
                {
                    cursorX = 0;
                    cursorY += rowHeight + 1;
                    rowHeight = 0;
                }

                if (cursorY + height > atlasHeight)
                    throw new InvalidOperationException("Font atlas is too small.");

                byte[] bitmap = new byte[width * height];

                fixed (byte* bitmapPtr = bitmap)
                    StbTrueType.stbtt_MakeCodepointBitmap(font, bitmapPtr, width, height, width, scale, scale, codepoint);

                for (int y = 0; y < height; y++)
                    Buffer.BlockCopy(bitmap, y * width, atlas, (cursorY + y) * atlasWidth + cursorX, width);

                var uvPosition = new Vector2((float)cursorX / atlasWidth, (float)cursorY / atlasHeight);

                var uvSize = new Vector2((float)width / atlasWidth, (float)height / atlasHeight); 

                var size = new Vector2(width, height);

                var bearing = new Vector2(x0, y0);

                float advance = advanceWidth * scale;

                glyphs[character] = new Glyph(uvPosition, uvSize, size, bearing, advance); 

                cursorX += width + 1;
                rowHeight = Math.Max(rowHeight, height);
            }

            var texture = device.CreateTexture(new()
            {
                Width = (uint)atlasWidth,
                Height = (uint)atlasHeight,
                Depth = 1,
                MipLevels = 1,
                ArrayLayers = 1,
                Format = TextureFormat.R8Unorm,
                Usage = TextureUsage.Sampled,
                Type = TextureType.Texture2D,
                Samples = SampleCount.X1
            });

            texture.Upload(atlas, (uint)atlasWidth);

            float scaledAscent = ascent * scale;
            float scaledDescent = descent * scale;
            return new UIFont
            {
                Glyphs = glyphs,
                Atlas = texture,
                Ascent = scaledAscent,
                Descent = scaledDescent
            };
        }
    }
}
