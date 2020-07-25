using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

namespace ConformalDecals.Text {
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class TextRenderer : MonoBehaviour {
        public static TextRenderer Instance {
            get {
                if (!_instance._isSetup) {
                    _instance.Setup();
                }

                return _instance;
            }
        }

        public const TextureFormat       TextTextureFormat       = TextureFormat.RG16;
        public const RenderTextureFormat TextRenderTextureFormat = RenderTextureFormat.R8;

        private const string BlitShader     = "ConformalDecals/Text Blit";
        private const int    MaxTextureSize = 4096;
        private const float  FontSize       = 100;
        private const float  PixelDensity   = 5;

        private static TextRenderer _instance;

        private bool        _isSetup;
        private TextMeshPro _tmp;
        private Material    _blitMaterial;

        private Dictionary<DecalText, RenderedText> _renderedTextures = new Dictionary<DecalText, RenderedText>();
        private Texture2D                           _lastTexture; // to reduce the number of Texture2D objects created and destroyed, keep the last one on hand

        private void Start() {
            if (_instance != null) {
                Debug.Log("[ConformalDecals] Duplicate TextRenderer created???");
            }

            Debug.Log("[ConformalDecals] Creating TextRenderer Object");
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Setup() {
            if (_isSetup) return;

            Debug.Log("[ConformalDecals] Setting Up TextRenderer Object");

            _tmp = gameObject.AddComponent<TextMeshPro>();
            _tmp.renderer.enabled = false; // dont automatically render

            var shader = Shabby.Shabby.FindShader(BlitShader);
            if (shader == null) Debug.LogError($"[ConformalDecals] could not find text blit shader named '{shader}'");
            _blitMaterial = new Material(Shabby.Shabby.FindShader(BlitShader));

            _isSetup = true;
        }

        public void RenderText(DecalText text, out Texture2D texture, out Rect window) {
            // SETUP TMP OBJECT FOR RENDERING
            _tmp.text = text.FormattedText;
            _tmp.font = text.Font.FontAsset;
            _tmp.fontStyle = text.Style.FontStyle | text.Font.FontStyle;
            _tmp.lineSpacing = text.Style.LineSpacing;
            _tmp.characterSpacing = text.Style.CharacterSpacing;

            _tmp.extraPadding = true;
            _tmp.enableKerning = true;
            _tmp.enableWordWrapping = false;
            _tmp.overflowMode = TextOverflowModes.Overflow;
            _tmp.alignment = TextAlignmentOptions.Center | TextAlignmentOptions.Baseline;
            _tmp.fontSize = FontSize;

            // SETUP BLIT MATERIAL
            _blitMaterial.SetTexture(PropertyIDs._MainTex, text.Font.FontAsset.atlas);

            // GENERATE MESH
            _tmp.ForceMeshUpdate();
            var mesh = _tmp.mesh;
            mesh.RecalculateBounds();
            var bounds = mesh.bounds;

            // CALCULATE SIZES
            var size = bounds.size * PixelDensity;

            var textureSize = new Vector2Int {
                x = Mathf.NextPowerOfTwo((int) size.x),
                y = Mathf.NextPowerOfTwo((int) size.y)
            };

            // make sure texture isnt too big, scale it down if it is
            // this is just so you dont crash the game by pasting in the entire script of The Bee Movie
            if (textureSize.x > MaxTextureSize) {
                textureSize.y /= textureSize.x / MaxTextureSize;
                textureSize.x = MaxTextureSize;
            }

            if (textureSize.y > MaxTextureSize) {
                textureSize.x /= textureSize.y / MaxTextureSize;
                textureSize.y = MaxTextureSize;
            }

            // scale up everything to fit the texture for maximum usage
            float sizeRatio = Mathf.Min(textureSize.x / size.x, textureSize.y, size.y);

            // calculate where in the texture the used area actually is
            window = new Rect {
                size = size * sizeRatio,
                center = (Vector2) textureSize / 2
            };

            // GET TEXTURE
            if (_lastTexture != null) {
                texture = _lastTexture;
                texture.Resize(textureSize.x, textureSize.y, TextTextureFormat, false);
                _lastTexture = null;
            }
            else {
                texture = new Texture2D(textureSize.x, textureSize.y, TextTextureFormat, false);
            }

            // GENERATE PROJECTION MATRIX
            var halfSize = (Vector2) textureSize / PixelDensity / 2 / sizeRatio;
            var matrix = Matrix4x4.Ortho(bounds.center.x - halfSize.x, bounds.center.x + halfSize.x,
                bounds.center.y - halfSize.y, bounds.center.y + halfSize.y, -1, 1);

            // GET RENDERTEX
            var renderTex = RenderTexture.GetTemporary(textureSize.x, textureSize.y, 0, TextRenderTextureFormat, RenderTextureReadWrite.Linear, 1);
            renderTex.autoGenerateMips = false;

            // RENDER
            Graphics.SetRenderTarget(renderTex);
            GL.PushMatrix();
            GL.LoadProjectionMatrix(matrix);
            _blitMaterial.SetPass(0);
            Graphics.DrawMeshNow(mesh, Matrix4x4.identity);
            GL.PopMatrix();

            // COPY TEXTURE BACK INTO RAM
            RenderTexture.active = renderTex;
            texture.ReadPixels(new Rect(0, 0, textureSize.x, textureSize.y), 0, 0, false);
            texture.Apply();

            // RELEASE RENDERTEX
            RenderTexture.ReleaseTemporary(renderTex);
        }
    }
}