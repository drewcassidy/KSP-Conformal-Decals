using System.Collections;
using System.Net;
using ConformalDecals.MaterialProperties;
using ConformalDecals.Text;
using ConformalDecals.UI;
using ConformalDecals.Util;
using TMPro;
using UnityEngine;

namespace ConformalDecals {
    public class ModuleConformalText : ModuleConformalDecal {
        [KSPField] public Vector2 lineSpacingRange = new Vector2(-50, 50);
        [KSPField] public Vector2 charSpacingRange = new Vector2(-50, 50);

        [KSPField(isPersistant = true)] public bool  vertical;
        [KSPField(isPersistant = true)] public float lineSpacing;
        [KSPField(isPersistant = true)] public float charSpacing;

        [KSPField] public string     text;
        [KSPField] public DecalFont  font;
        [KSPField] public FontStyles style;
        [KSPField] public Color32    fillColor    = Color.black;
        [KSPField] public Color32    outlineColor = Color.white;

        // KSP TWEAKABLES

        [KSPEvent(guiName = "#LOC_ConformalDecals_gui-set-text", guiActive = false, guiActiveEditor = true)]
        public void SetText() {
            if (_textEntryController == null) {
                _textEntryController = TextEntryController.Create(text, font, style, vertical, lineSpacing, charSpacing, lineSpacingRange, charSpacingRange, OnTextUpdate);
            }
            else {
                _textEntryController.Close();
            }
        }

        // FILL

        [KSPField(guiName = "#LOC_ConformalDecals_gui-fill", groupName = "decal-fill", groupDisplayName = "#LOC_ConformalDecals_gui-group-fill",
             guiActive = false, guiActiveEditor = true, isPersistant = true),
         UI_Toggle()]
        public bool fillEnabled = true;

        [KSPEvent(guiName = "#LOC_ConformalDecals_gui-set-fill-color", groupName = "decal-fill", groupDisplayName = "#LOC_ConformalDecals_gui-group-fill",
            guiActive = false, guiActiveEditor = true)]
        public void SetFillColor() {
            if (_fillColorPickerController == null) {
                _fillColorPickerController = ColorPickerController.Create(fillColor, OnFillColorUpdate);
            }
            else {
                _fillColorPickerController.Close();
            }
        }

        // OUTLINE

        [KSPField(guiName = "#LOC_ConformalDecals_gui-outline", groupName = "decal-outline", groupDisplayName = "#LOC_ConformalDecals_gui-group-outline",
             guiActive = false, guiActiveEditor = true, isPersistant = true),
         UI_Toggle()]
        public bool outlineEnabled;

        [KSPField(guiName = "#LOC_ConformalDecals_gui-outline-width", groupName = "decal-outline", groupDisplayName = "#LOC_ConformalDecals_gui-group-outline",
             guiActive = false, guiActiveEditor = true, isPersistant = true, guiFormat = "F2"),
         UI_FloatRange(stepIncrement = 0.05f)]
        public float outlineWidth = 0.1f;

        [KSPEvent(guiName = "#LOC_ConformalDecals_gui-set-outline-color", groupName = "decal-outline", groupDisplayName = "#LOC_ConformalDecals_gui-group-outline",
            guiActive = false, guiActiveEditor = true)]
        public void SetOutlineColor() {
            if (_outlineColorPickerController == null) {
                _outlineColorPickerController = ColorPickerController.Create(outlineColor, OnOutlineColorUpdate);
            }
            else {
                _outlineColorPickerController.Close();
            }
        }


        private TextEntryController   _textEntryController;
        private ColorPickerController _fillColorPickerController;
        private ColorPickerController _outlineColorPickerController;

        private MaterialTextureProperty _decalTextureProperty;

        private MaterialKeywordProperty _fillEnabledProperty;
        private MaterialColorProperty   _fillColorProperty;

        private MaterialKeywordProperty _outlineEnabledProperty;
        private MaterialColorProperty   _outlineColorProperty;
        private MaterialFloatProperty   _outlineWidthProperty;

        private TextRenderJob _currentJob;
        private DecalText     _currentText;

        public override void OnLoad(ConfigNode node) {
            base.OnLoad(node);
            text = WebUtility.UrlDecode(ParseUtil.ParseString(node, "text"));
            font = DecalConfig.GetFont(ParseUtil.ParseString(node, "font", true, "Calibri SDF"));
            int styleInt = 0;
            if (ParseUtil.ParseIntIndirect(ref styleInt, node, "style")) style = (FontStyles) styleInt;
            if (!ParseUtil.ParseColor32Indirect(ref fillColor, node, "fillColor")) fillColor = Color.magenta;
            if (!ParseUtil.ParseColor32Indirect(ref outlineColor, node, "outlineColor")) outlineColor = Color.magenta;

            if (HighLogic.LoadedSceneIsGame) {
                // For some reason, rendering doesnt work right on the first frame a scene is loaded
                // So delay any rendering until the next frame when called in OnLoad
                // This is probably a problem with Unity, not KSP
                StartCoroutine(UpdateTextLate());
            }
            else {
                UpdateText();
            }
        }

        public override void OnSave(ConfigNode node) {
            node.AddValue("text", WebUtility.UrlEncode(text));
            node.AddValue("fontName", font.Name);
            node.AddValue("style", (int) style);
            node.AddValue("fillColor", fillColor.ToHexString());
            node.AddValue("outlineColor", outlineColor.ToHexString());
            base.OnSave(node);
        }

        public override void OnAwake() {
            base.OnAwake();

            _decalTextureProperty = materialProperties.AddOrGetTextureProperty("_Decal", true);

            _fillEnabledProperty = materialProperties.AddOrGetProperty<MaterialKeywordProperty>("DECAL_FILL");
            _fillColorProperty = materialProperties.AddOrGetProperty<MaterialColorProperty>("_DecalColor");

            _outlineEnabledProperty = materialProperties.AddOrGetProperty<MaterialKeywordProperty>("DECAL_OUTLINE");
            _outlineColorProperty = materialProperties.AddOrGetProperty<MaterialColorProperty>("_OutlineColor");
            _outlineWidthProperty = materialProperties.AddOrGetProperty<MaterialFloatProperty>("_OutlineWidth");
        }

        public void OnTextUpdate(string newText, DecalFont newFont, FontStyles newStyle, bool newVertical, float newLineSpacing, float newCharSpacing) {
            this.text = newText;
            this.font = newFont;
            this.style = newStyle;
            this.vertical = newVertical;
            this.lineSpacing = newLineSpacing;
            this.charSpacing = newCharSpacing;
            UpdateTextRecursive();
        }

        public void OnFillColorUpdate(Color rgb, Util.ColorHSV hsv) {
            fillColor = rgb;
            UpdateMaterials();

            foreach (var counterpart in part.symmetryCounterparts) {
                var decal = counterpart.GetComponent<ModuleConformalText>();
                decal.fillColor = fillColor;
                decal.UpdateMaterials();
            }
        }

        public void OnOutlineColorUpdate(Color rgb, Util.ColorHSV hsv) {
            outlineColor = rgb;
            UpdateMaterials();

            foreach (var counterpart in part.symmetryCounterparts) {
                var decal = counterpart.GetComponent<ModuleConformalText>();
                decal.outlineColor = outlineColor;
                decal.UpdateMaterials();
            }
        }

        public void OnFillToggle(BaseField field, object obj) {
            // fill and outline cant both be disabled
            outlineEnabled = outlineEnabled || (!outlineEnabled && !fillEnabled);

            UpdateTweakables();
            UpdateMaterials();

            foreach (var counterpart in part.symmetryCounterparts) {
                var decal = counterpart.GetComponent<ModuleConformalText>();
                decal.fillEnabled = fillEnabled;
                decal.outlineEnabled = outlineEnabled;
                decal.UpdateTweakables();
                decal.UpdateMaterials();
            }
        }

        public void OnOutlineToggle(BaseField field, object obj) {
            // fill and outline cant both be disabled
            fillEnabled = fillEnabled || (!fillEnabled && !outlineEnabled);

            UpdateTweakables();
            UpdateMaterials();

            foreach (var counterpart in part.symmetryCounterparts) {
                var decal = counterpart.GetComponent<ModuleConformalText>();
                decal.fillEnabled = fillEnabled;
                decal.outlineEnabled = outlineEnabled;
                decal.UpdateTweakables();
                decal.UpdateMaterials();
            }
        }

        public void OnOutlineWidthUpdate(BaseField field, object obj) {
            UpdateMaterials();

            foreach (var counterpart in part.symmetryCounterparts) {
                var decal = counterpart.GetComponent<ModuleConformalText>();
                decal.outlineWidth = outlineWidth;
                decal.UpdateMaterials();
            }
        }

        public override void OnDestroy() {
            if (HighLogic.LoadedSceneIsGame && _currentText != null) TextRenderer.UnregisterText(_currentText);
            
            // close all UIs
            if (_textEntryController != null) _textEntryController.Close();
            if (_fillColorPickerController != null) _fillColorPickerController.Close();
            if (_outlineColorPickerController != null) _outlineColorPickerController.Close();
            
            base.OnDestroy();
        }

        protected override void OnDetach() {
            // close all UIs
            if (_textEntryController != null) _textEntryController.Close();
            if (_fillColorPickerController != null) _fillColorPickerController.Close();
            if (_outlineColorPickerController != null) _outlineColorPickerController.Close();

            base.OnDetach();
        }

        private void UpdateTextRecursive() {
            UpdateText();

            foreach (var counterpart in part.symmetryCounterparts) {
                var decal = counterpart.GetComponent<ModuleConformalText>();
                decal.text = text;
                decal.font = font;
                decal.style = style;

                decal._currentJob = _currentJob;
                decal._currentText = _currentText;
                decal.UpdateText();
            }
        }

        private IEnumerator UpdateTextLate() {
            yield return null;
            UpdateText();
        }

        private void UpdateText() {
            // Render text
            var newText = new DecalText(text, font, style, vertical, lineSpacing, charSpacing);
            var output = TextRenderer.UpdateTextNow(_currentText, newText);
            _currentText = newText;

            UpdateTexture(output);

            // TODO: ASYNC RENDERING
            // var newText = new DecalText(text, _font, _style);
            // _currentJob = TextRenderer.UpdateText(_currentText, newText, UpdateTexture);
            // _currentText = newText;
        }

        public void UpdateTexture(TextRenderOutput output) {
            _decalTextureProperty.Texture = output.Texture;
            _decalTextureProperty.SetTile(output.Window);

            UpdateMaterials();
            UpdateScale();
        }

        protected override void UpdateMaterials() {
            _fillEnabledProperty.value = fillEnabled;
            _fillColorProperty.color = fillColor;

            _outlineEnabledProperty.value = outlineEnabled;
            _outlineColorProperty.color = outlineColor;
            _outlineWidthProperty.value = outlineWidth;

            base.UpdateMaterials();
        }

        protected override void UpdateTweakables() {
            var fillEnabledField = Fields[nameof(fillEnabled)];
            var fillColorEvent = Events["SetFillColor"];

            var outlineEnabledField = Fields[nameof(outlineEnabled)];
            var outlineWidthField = Fields[nameof(outlineWidth)];
            var outlineColorEvent = Events["SetOutlineColor"];

            fillColorEvent.guiActiveEditor = fillEnabled;
            outlineWidthField.guiActiveEditor = outlineEnabled;
            outlineColorEvent.guiActiveEditor = outlineEnabled;

            ((UI_Toggle) fillEnabledField.uiControlEditor).onFieldChanged = OnFillToggle;
            ((UI_Toggle) outlineEnabledField.uiControlEditor).onFieldChanged = OnOutlineToggle;
            ((UI_FloatRange) outlineWidthField.uiControlEditor).onFieldChanged = OnOutlineWidthUpdate;

            base.UpdateTweakables();
        }
    }
}