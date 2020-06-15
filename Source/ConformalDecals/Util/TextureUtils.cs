using UnityEngine;

namespace ConformalDecals.Util {
    public static class TextureUtils {
        public enum BlitMode {
            Set,
            Add,
        }

        public static Color32 AddColor32(Color32 color1, Color32 color2) {
            return new Color32((byte) (color1.r + color2.r), (byte) (color1.g + color2.g), (byte) (color1.b + color2.b), (byte) (color1.a + color2.a));
        }

        public static Color32 AddColor32Clamped(Color32 color1, Color32 color2) {
            var r = color1.r + color2.r;
            var g = color1.g + color2.g;
            var b = color1.b + color2.b;
            var a = color1.a + color2.a;
            if (r > byte.MaxValue) r = byte.MaxValue;
            if (g > byte.MaxValue) g = byte.MaxValue;
            if (b > byte.MaxValue) b = byte.MaxValue;
            if (a > byte.MaxValue) a = byte.MaxValue;

            return new Color32((byte) r, (byte) g, (byte) b, (byte) a);
        }

        public static void ClearTexture(Color32[] colors, Color32 clearColor = default) {
            for (var i = 0; i < colors.Length; i++) {
                colors[i] = clearColor;
            }
        }

        public static void BlitRectAlpha(
            Texture2D src, Color32[] srcColors, Vector2Int srcPos,
            Texture2D dst, Color32[] dstColors, Vector2Int dstPos,
            Vector2Int size, BlitMode mode) {

            ClipRect(src, ref srcPos, dst, ref dstPos, ref size);

            if (size.x <= 0 || size.y <= 0) return;

            int srcIndex = srcPos.x + srcPos.y * src.width;
            int dstIndex = dstPos.x + dstPos.y * dst.width;

            for (int dy = size.y - 1; dy >= 0; dy--) {

                for (int dx = size.x - 1; dx >= 0; dx--) {
                    switch (mode) {
                        case BlitMode.Set:
                            dstColors[dstIndex + dx].a = srcColors[srcIndex + dx].a;
                            break;
                        case BlitMode.Add:
                            var s = srcColors[srcIndex + dx].a;
                            var d = dstColors[dstIndex + dx].a;
                            var sum = s + d;
                            if (sum > byte.MaxValue) sum = byte.MaxValue;
                            dstColors[dstIndex + dx].a = (byte) sum;
                            break;
                    }
                }

                srcIndex += src.width;
                dstIndex += dst.width;
            }
        }

        public static void BlitRect(
            Texture2D src, Color32[] srcColors, Vector2Int srcPos,
            Texture2D dst, Color32[] dstColors, Vector2Int dstPos,
            Vector2Int size, BlitMode mode) {

            ClipRect(src, ref srcPos, dst, ref dstPos, ref size);

            if (size.x <= 0 || size.y <= 0) return;

            int srcIndex = srcPos.x + srcPos.y * src.width;
            int dstIndex = dstPos.x + dstPos.y * dst.width;

            for (int dy = 0; dy < size.y; dy++) {

                for (int dx = 0; dx < size.x; dx++) {
                    switch (mode) {
                        case BlitMode.Set:
                            dstColors[dstIndex + dx] = srcColors[srcIndex + dx];
                            break;
                        case BlitMode.Add:
                            dstColors[dstIndex + dx] = AddColor32Clamped(srcColors[srcIndex + dx], dstColors[dstIndex + dx]);
                            break;
                    }
                }

                srcIndex += src.width;
                dstIndex += dst.width;
            }
        }

        public static void BlitRectBilinearAlpha(
            Texture2D src, Vector2Int srcPos, Vector2 srcSize,
            Texture2D dst, Color32[] dstColors, Vector2Int dstPos, Vector2Int dstSize,
            BlitMode mode) {

            var sizeRatio = dstSize / srcSize;

            ClipRect(src, ref srcPos, dst, ref dstPos, ref srcSize, ref dstSize);

            if (dstSize.x <= 0 || dstSize.y <= 0) return;

            var srcPixel = new Vector2(1.0f / src.width, 1.0f / src.height);
            var srcStart = (srcPos * srcPixel) + (srcPixel / 2);
            var srcStep = sizeRatio * srcPixel;
            var srcY = srcStart.y;

            int dstIndex = dstPos.x + dstPos.y * dst.width;
            for (int dy = 0;
                dy < dstSize.y;
                dy++) {
                var srcX = srcStart.x;

                for (int dx = 0; dx < dstSize.x; dx++) {
                    switch (mode) {
                        case BlitMode.Set:
                            dstColors[dstIndex + dx].a = (byte) (src.GetPixelBilinear(srcX, srcY).a * byte.MaxValue);
                            break;
                        case BlitMode.Add:
                            var s = (byte) (src.GetPixelBilinear(srcX, srcY).a * byte.MaxValue);
                            var d = dstColors[dstIndex + dx].a;
                            var sum = s + d;
                            if (sum > byte.MaxValue) sum = byte.MaxValue;
                            dstColors[dstIndex + dx].a = (byte) sum;
                            break;
                    }

                    srcX += srcStep.x;
                }

                srcY += srcStep.y;
                dstIndex += dst.width;
            }
        }

        public static void BlitRectBilinear(
            Texture2D src, Vector2Int srcPos, Vector2 srcSize,
            Texture2D dst, Color32[] dstColors, Vector2Int dstPos, Vector2Int dstSize,
            BlitMode mode) {

            var sizeRatio = dstSize / srcSize;

            ClipRect(src, ref srcPos, dst, ref dstPos, ref srcSize, ref dstSize);

            if (dstSize.x <= 0 || dstSize.y <= 0) return;

            var srcPixel = new Vector2(1.0f / src.width, 1.0f / src.height);
            var srcStart = (srcPos * srcPixel) + (srcPixel / 2);
            var srcStep = sizeRatio * srcPixel;
            var srcY = srcStart.y;

            int dstIndex = dstPos.x + dstPos.y * dst.width;
            for (int dy = 0;
                dy < dstSize.y;
                dy++) {
                var srcX = srcStart.x;

                for (int dx = 0; dx < dstSize.x; dx++) {
                    switch (mode) {
                        case BlitMode.Set:
                            dstColors[dstIndex + dx] = src.GetPixelBilinear(srcX, srcY);
                            break;
                        case BlitMode.Add:
                            dstColors[dstIndex + dx] = AddColor32Clamped(src.GetPixelBilinear(srcX, srcY), dstColors[dstIndex + dx]);
                            break;
                    }

                    srcX += srcStep.x;
                }

                srcY += srcStep.y;
                dstIndex += dst.width;
            }
        }

        private static void ClipRect(Texture2D src, ref Vector2Int srcPos, Texture2D dst, ref Vector2Int dstPos, ref Vector2Int size) {
            if (srcPos.x < 0) {
                size.x += srcPos.x;
                dstPos.x -= srcPos.x;
                srcPos.x = 0;
            }

            if (srcPos.y < 0) {
                size.y += srcPos.y;
                dstPos.y -= srcPos.y;
                srcPos.y = 0;
            }

            if (dstPos.x < 0) {
                size.x += dstPos.x;
                srcPos.x -= dstPos.x;
                dstPos.x = 0;
            }

            if (dstPos.y < 0) {
                size.y += dstPos.y;
                srcPos.y -= dstPos.y;
                dstPos.y = 0;
            }

            if (srcPos.x + size.x > src.width) size.x = src.width - srcPos.x;
            if (srcPos.y + size.y > src.height) size.y = src.height - srcPos.y;
            if (dstPos.x + size.x > dst.width) size.x = dst.width - srcPos.x;
            if (dstPos.y + size.y > dst.height) size.y = dst.height - srcPos.y;
        }

        private static void ClipRect(Texture2D src, ref Vector2Int srcPos, Texture2D dst, ref Vector2Int dstPos, ref Vector2 srcSize, ref Vector2Int dstSize) {
            var sizeRatio = dstSize / srcSize;
            if (srcPos.x < 0) {
                dstSize.x += (int) (srcPos.x * sizeRatio.x);
                dstPos.x -= (int) (srcPos.x * sizeRatio.x);
                srcSize.x += srcPos.x;
                srcPos.x = 0;
            }

            if (srcPos.y < 0) {
                dstSize.y += (int) (srcPos.y * sizeRatio.y);
                dstPos.y -= (int) (srcPos.y * sizeRatio.y);
                srcSize.y += srcPos.y;
                srcPos.y = 0;
            }

            if (dstPos.x < 0) {
                srcSize.x += dstPos.x / sizeRatio.x;
                srcPos.x -= (int) (dstPos.x / sizeRatio.x);
                dstSize.x += dstPos.x;
                dstPos.x = 0;
            }

            if (dstPos.y < 0) {
                srcSize.y += dstPos.y / sizeRatio.y;
                srcPos.y -= (int) (dstPos.y / sizeRatio.y);
                dstSize.y += dstPos.y;
                dstPos.y = 0;
            }

            if (srcPos.x + srcSize.x > src.width) {
                srcSize.x = src.width - srcPos.x;
                dstSize.x = (int) (srcSize.x * sizeRatio.x);
            }

            if (srcPos.y + srcSize.y > src.height) {
                srcSize.y = src.height - srcPos.y;
                dstSize.y = (int) (srcSize.y * sizeRatio.y);
            }

            if (dstPos.x + dstSize.x > dst.width) {
                dstSize.x = dst.width - srcPos.x;
                srcSize.x = (int) (dstSize.x / sizeRatio.x);
            }

            if (dstPos.y + dstSize.y > dst.height) {
                dstSize.y = dst.height - srcPos.y;
                srcSize.y = (int) (dstSize.y / sizeRatio.y);
            }
        }
    }
}