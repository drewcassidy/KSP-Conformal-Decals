using UnityEngine;

namespace ConformalDecals.Util {
    public static class TextureUtils {
        public static void BlitRect(
            Texture2D src, Color32[] srcColors, Vector2Int srcPos,
            Texture2D dst, Color32[] dstColors, Vector2Int dstPos,
            Vector2Int size) {

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

            if (size.x <= 0) return;
            if (size.y <= 0) return;

            int srcIndex = srcPos.x + srcPos.y * src.width;
            int dstIndex = dstPos.x + dstPos.y * dst.width;

            for (int dy = 0; dy < size.y; dy++) {

                for (int dx = 0; dx < size.x; dx++) {
                    dstColors[dstIndex + dx] = srcColors[srcIndex + dx];
                }

                srcIndex += src.width;
                dstIndex += dst.width;
            }
        }

        public static void BlitRectBilinear(
            Texture2D src, Vector2Int srcPos, Vector2 srcSize,
            Texture2D dst, Color32[] dstColors, Vector2Int dstPos, Vector2Int dstSize) {

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

            var srcPixel = new Vector2(1.0f / src.width, 1.0f / src.height);

            var srcStart = (srcPos * srcPixel) + (srcPixel / 2);
            var srcStep = sizeRatio * srcPixel;

            var srcY = srcStart.y;
            int dstIndex = dstPos.x + dstPos.y * dst.width;

            for (int dy = 0; dy < dstSize.y; dy++) {
                var srcX = srcStart.x;

                for (int dx = 0; dx < dstSize.x; dx++) {
                    dstColors[dstIndex + dx] = src.GetPixelBilinear(srcX, srcY);
                    srcX += srcStep.x;
                }

                srcY += srcStep.y;
                dstIndex += dst.width;
            }
        }
    }
}