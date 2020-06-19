using ConformalDecals.Text;
using ConformalDecals.Util;
using TMPro;
using UnityEngine;

namespace ConformalDecals {
    public class ModuleConformalText: ModuleConformalDecal {
        private const string DefaultFlag = "Squad/Flags/default";

        [KSPField(isPersistant = true)] public string text = "Hello World!";

        public override void OnLoad(ConfigNode node) {
            base.OnLoad(node);

            SetText(text);
        }

        public override void OnStart(StartState state) {
            base.OnStart(state);

            SetText(text);
        }

        private void SetText(string newText) {
            this.Log("Rendering text for part");
            var fonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();

            foreach (var font in fonts) {
                this.Log($"Font: {font.name}");
                foreach (var fallback in font.fallbackFontAssets) {
                    this.Log($"    Fallback: {fallback.name}");
                }
            }

            materialProperties.AddOrGetTextureProperty("_Decal", true).Texture = TextRenderer.RenderToTexture(fonts[0], newText);

            UpdateMaterials();
        }
    }
}