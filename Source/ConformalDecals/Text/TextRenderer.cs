using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace ConformalDecals.Text {
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class TextRenderer : MonoBehaviour {
        public const TextureFormat       TextTextureFormat       = TextureFormat.RG16;
        public const RenderTextureFormat TextRenderTextureFormat = RenderTextureFormat.R8;

        public static TextRenderer Instance {
            get {
                if (!_instance._isSetup) {
                    _instance.Setup();
                }

                return _instance;
            }
        }

        [Serializable]
        public class TextRenderEvent : UnityEvent<TextRenderOutput> { }

        private const string BlitShader     = "ConformalDecals/Text Blit";
        private const int    MaxTextureSize = 4096;
        private const float  FontSize       = 100;
        private const float  PixelDensity   = 5;

        private static TextRenderer _instance;

        private bool        _isSetup;
        private TextMeshPro _tmp;
        private Material    _blitMaterial;

        private static readonly Dictionary<DecalText, TextRenderOutput> RenderCache = new Dictionary<DecalText, TextRenderOutput>();
        private static readonly Queue<TextRenderJob>                    RenderJobs  = new Queue<TextRenderJob>();

        public static TextRenderJob UpdateText(DecalText oldText, DecalText newText, UnityAction<TextRenderOutput> renderFinishedCallback) {
            if (newText == null) throw new ArgumentNullException(nameof(newText));

            var job = new TextRenderJob(oldText, newText, renderFinishedCallback);
            RenderJobs.Enqueue(job);
            return job;
        }
        
        public static TextRenderOutput UpdateTextNow(DecalText oldText, DecalText newText) {
            if (newText == null) throw new ArgumentNullException(nameof(newText));
            
            return Instance.RunJob(new TextRenderJob(oldText, newText, null), out _);
        }

        public static void UnregisterText(DecalText text) {
            if (RenderCache.TryGetValue(text, out var renderedText)) {
                renderedText.UserCount--;
                if (renderedText.UserCount <= 0) {
                    RenderCache.Remove(text);
                    Destroy(renderedText.Texture);
                }
            }
        }

        private void Start() {
            if (_instance != null) {
                Debug.Log("[ConformalDecals] Duplicate TextRenderer created???");
            }

            Debug.Log("[ConformalDecals] Creating TextRenderer Object");
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update() {
            // TODO: ASYNC RENDERING
            // bool renderNeeded;
            // do {
            //     if (RenderJobs.Count == 0) return;
            //     var nextJob = RenderJobs.Dequeue();
            //     RunJob(nextJob, out renderNeeded);
            // } while (!renderNeeded);
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

        private TextRenderOutput RunJob(TextRenderJob job, out bool renderNeeded) {
            if (!job.Needed) {
                renderNeeded = false;
                return null;
            }

            Debug.Log($"Starting Text Rendering Job. queue depth = {RenderJobs.Count}, cache size = {RenderCache.Count}");
            job.Start();

            Texture2D texture = null;
            if (job.OldText != null && RenderCache.TryGetValue(job.OldText, out var oldRender)) {
                // old output still exists

                oldRender.UserCount--;

                if (oldRender.UserCount <= 0) {
                    // this is the only usage of this output, so we are free to re-render into the texture
                    Debug.Log("Render output is not shared with other users, so reusing texture and removing cache slot");

                    texture = oldRender.Texture;
                    RenderCache.Remove(job.OldText);
                }
                else {
                    // other things are using this render output, so decriment usercount, and we'll make a new entry instead
                    Debug.Log("Render output is shared with other users, so making new output");
                }
            }

            // now that all old references are handled, begin rendering the new output

            if (RenderCache.TryGetValue(job.NewText, out var cachedRender)) {
                Debug.Log("Using Cached Render Output");
                Debug.Log($"Finished Text Rendering Job. queue depth = {RenderJobs.Count}, cache size = {RenderCache.Count}");

                cachedRender.UserCount++;
                job.Finish(cachedRender);
                renderNeeded = false;
                return cachedRender;
            }

            var output = RenderText(job.NewText, texture);
            RenderCache.Add(job.NewText, output);

            job.Finish(output);
            Debug.Log($"Finished Text Rendering Job. queue depth = {RenderJobs.Count}, cache size = {RenderCache.Count}");
            renderNeeded = true;
            return output;
        }

        public TextRenderOutput RenderText(DecalText text, Texture2D texture) {
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
            
            // CALCULATE FONT WEIGHT

            float weight = 0;
            if (text.Style.Bold && text.Font.FontAsset.fontWeights[7].regularTypeface == null) {
                weight = text.Font.FontAsset.boldStyle;
            }

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
            float sizeRatio = Mathf.Min(textureSize.x / size.x, textureSize.y / size.y);

            // calculate where in the texture the used area actually is
            var window = new Rect {
                size = size * sizeRatio,
                center = (Vector2) textureSize / 2
            };
            
            Debug.Log($"Window size: {window.size}");
            Debug.Log($"Texture size: {textureSize}");

            // SETUP TEXTURE
            if (texture == null) {
                texture = new Texture2D(textureSize.x, textureSize.y, TextTextureFormat, false);
            }
            else if (texture.width != textureSize.x || texture.height != textureSize.y || texture.format != TextTextureFormat) {
                texture.Resize(textureSize.x, textureSize.y, TextTextureFormat, false);
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
            GL.Clear(false, true, Color.black);
            _blitMaterial.SetPass(0);
            Graphics.DrawMeshNow(mesh, Matrix4x4.identity);
            GL.PopMatrix();

            // COPY TEXTURE BACK INTO RAM
            RenderTexture.active = renderTex;
            texture.ReadPixels(new Rect(0, 0, textureSize.x, textureSize.y), 0, 0, false);
            texture.Apply();

            // RELEASE RENDERTEX
            RenderTexture.ReleaseTemporary(renderTex);

            return new TextRenderOutput(texture, window, weight);
        }
    }
}