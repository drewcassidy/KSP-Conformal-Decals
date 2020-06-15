using System;
using ConformalDecals.Util;
using TMPro;
using UnityEngine;

namespace ConformalDecals {
    public class TextRenderer {
        private struct GlyphInfo {
            public TMP_Glyph  glyph;
            public Vector2Int size;
            public Vector2Int position;
            public int        fontIndex;
            public bool       needsResample;
        }

        private struct FontInfo {
            public TMP_FontAsset font;
            public Texture2D     fontAtlas;
            public Color32[]     fontAtlasColors;
        }

        public static Texture2D RenderToTexture(TMP_FontAsset font, string text) {
            Debug.Log($"Rendering text: {text}");
            var charArray = text.ToCharArray();
            var glyphInfoArray = new GlyphInfo[charArray.Length];
            var fontInfoArray = new FontInfo[charArray.Length];

            var baseScale = font.fontInfo.Scale;

            var padding = (int) font.fontInfo.Padding;
            var ascender = (int) font.fontInfo.Ascender;
            var descender = (int) font.fontInfo.Descender;
            var baseline = (int) baseScale * (descender + padding);
            Debug.Log($"baseline: {baseline}");
            Debug.Log($"ascender: {ascender}");
            Debug.Log($"descender: {descender}");
            Debug.Log($"baseScale: {baseScale}");

            fontInfoArray[0].font = font;

            int xAdvance = 0;
            for (var i = 0; i < charArray.Length; i++) {

                var glyphFont = TMP_FontUtilities.SearchForGlyph(font, charArray[i], out var glyph);

                if (glyphFont == font) {
                    glyphInfoArray[i].fontIndex = 0;
                }
                else {
                    for (int f = 1; i < charArray.Length; i++) {
                        if (fontInfoArray[f].font == null) {
                            fontInfoArray[f].font = glyphFont;
                            glyphInfoArray[i].fontIndex = f;
                            break;
                        }

                        if (fontInfoArray[f].font == glyphFont) {
                            glyphInfoArray[i].fontIndex = f;
                            break;
                        }
                    }
                }

                Debug.Log($"getting font info for character: '{charArray[i]}'");
                Debug.Log($"character font: {glyphFont.name}");

                glyphInfoArray[i].glyph = glyph;
                glyphInfoArray[i].needsResample = false;

                float elementScale = glyph.scale;

                if (glyphFont == font) {
                    if (!Mathf.Approximately(elementScale, 1)) {
                        glyphInfoArray[i].needsResample = true;
                    }

                    elementScale *= baseScale;
                }
                else {
                    var fontScale = glyphFont.fontInfo.Scale / glyphFont.fontInfo.PointSize;
                    if (!Mathf.Approximately(fontScale, baseScale)) {
                        glyphInfoArray[i].needsResample = true;
                    }

                    elementScale *= fontScale;
                }

                Debug.Log($"character scale: {glyphFont.fontInfo.Scale / glyphFont.fontInfo.PointSize}");
                Debug.Log($"character needs resampling: {glyphInfoArray[i].needsResample}");

                glyphInfoArray[i].size.x = (int) ((glyph.width + (padding * 2)) * elementScale);
                glyphInfoArray[i].size.y = (int) ((glyph.height + (padding * 2)) * elementScale);
                glyphInfoArray[i].position.x = (int) ((xAdvance + glyph.xOffset - padding) * elementScale);
                glyphInfoArray[i].position.y = (int) ((baseline + glyph.yOffset - padding) * elementScale);
                
                Debug.Log($"character size: {glyphInfoArray[i].size}");
                Debug.Log($"character position: {glyphInfoArray[i].position}");
            }

            // calculate texture bounds
            int xOffset = glyphInfoArray[0].position.x;
            var textureWidth = (glyphInfoArray[charArray.Length - 1].position.x + glyphInfoArray[charArray.Length - 1].size.x) - xOffset;
            var textureHeight = (int) baseScale * (ascender + descender + padding * 2);

            // ensure texture sizes are powers of 2
            textureWidth = Mathf.NextPowerOfTwo(textureWidth);
            textureHeight = Mathf.NextPowerOfTwo(textureHeight);
            Debug.Log($"texture is {textureWidth} x {textureHeight}");

            var texture = new Texture2D(textureWidth, textureHeight, TextureFormat.Alpha8, true);

            var colors = new Color32[textureWidth * textureHeight];

            for (var i = 0; i < fontInfoArray.Length; i++) {
                if (fontInfoArray[i].font == null) break;
                fontInfoArray[i].fontAtlas = fontInfoArray[i].font.atlas;
                fontInfoArray[i].fontAtlasColors = fontInfoArray[i].fontAtlas.GetPixels32();
            }

            for (int i = 0; i < charArray.Length; i++) {
                var glyphInfo = glyphInfoArray[i];
                var glyph = glyphInfo.glyph;
                var fontInfo = fontInfoArray[glyphInfo.fontIndex];

                var srcPos = new Vector2Int((int) glyph.x, (int) glyph.y);
                var dstPos = glyphInfo.position;
                dstPos.x += xOffset;
                var dstSize = glyphInfo.size;
                
                Debug.Log($"rendering character number {i}");

                if (glyphInfo.needsResample) {
                    var srcSize = new Vector2(glyph.width, glyph.height);
                    TextureUtils.BlitRectBilinearAlpha(fontInfo.fontAtlas, srcPos, srcSize, texture, colors, dstPos, dstSize, TextureUtils.BlitMode.Add);
                }
                else {
                    TextureUtils.BlitRectAlpha(fontInfo.fontAtlas, fontInfo.fontAtlasColors, srcPos, texture, colors, dstPos, dstSize, TextureUtils.BlitMode.Add);
                }
            }

            texture.Apply(true);

            return texture;
        }
    }
}