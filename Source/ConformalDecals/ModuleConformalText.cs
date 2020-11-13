using System.Collections;
using System.Net;
using ConformalDecals.MaterialProperties;
using ConformalDecals.Text;
using ConformalDecals.UI;
using ConformalDecals.Util;
using TMPro;
using UnityEngine;

namespace ConformalDecals {
    public class ModuleConformalText : ModuleConformalDecal, ISerializationCallbackReceiver {
        [KSPField(isPersistant = true)] public string text = "Text";

        [KSPField] public Vector2 lineSpacingRange = new Vector2(-50, 50);
        [KSPField] public Vector2 charSpacingRange = new Vector2(-50, 50);

        // serialization-only fields. do not use except in serialization functions
        [KSPField(isPersistant = true)] public string fontName = "Calibri SDF";
        [KSPField(isPersistant = true)] public int    style;
        [KSPField(isPersistant = true)] public bool   vertical;
        [KSPField(isPersistant = true)] public float  lineSpacing;
        [KSPField(isPersistant = true)] public float  charSpacing;
        [KSPField(isPersistant = true)] public string fillColor    = "000000FF";
        [KSPField(isPersistant = true)] public string outlineColor = "FFFFFFFF";

        // KSP TWEAKABLES

        [KSPEvent(guiName = "#LOC_ConformalDecals_gui-set-text", guiActive = false, guiActiveEditor = true)]
        public void SetText() {
            if (_textEntryController == null) {
                _textEntryController = TextEntryController.Create(_text, _font, _style, lineSpacingRange, charSpacingRange, OnTextUpdate);
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
                _fillColorPickerController = ColorPickerController.Create(_fillColor, OnFillColorUpdate);
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
                _outlineColorPickerController = ColorPickerController.Create(_outlineColor, OnOutlineColorUpdate);
            }
            else {
                _outlineColorPickerController.Close();
            }
        }

        private string         _text;
        private DecalTextStyle _style;
        private DecalFont      _font;
        private Color32        _fillColor;
        private Color32        _outlineColor;

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
            OnAfterDeserialize();

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
            OnBeforeSerialize();
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

        public void OnTextUpdate(string newText, DecalFont newFont, DecalTextStyle newStyle) {
            _text = newText;
            _font = newFont;
            _style = newStyle;
            UpdateTextRecursive();
        }

        public void OnFillColorUpdate(Color rgb, Util.ColorHSV hsv) {
            _fillColor = rgb;
            UpdateMaterials();

            foreach (var counterpart in part.symmetryCounterparts) {
                var decal = counterpart.GetComponent<ModuleConformalText>();
                decal._fillColor = _fillColor;
                decal.UpdateMaterials();
            }
        }

        public void OnOutlineColorUpdate(Color rgb, Util.ColorHSV hsv) {
            _outlineColor = rgb;
            UpdateMaterials();

            foreach (var counterpart in part.symmetryCounterparts) {
                var decal = counterpart.GetComponent<ModuleConformalText>();
                decal._outlineColor = _outlineColor;
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

        public void OnBeforeSerialize() {
            text = WebUtility.UrlEncode(_text);
            fontName = _font.Name;
            style = (int) _style.FontStyle;
            vertical = _style.Vertical;
            lineSpacing = _style.LineSpacing;
            charSpacing = _style.CharSpacing;
            fillColor = _fillColor.ToHexString();
            outlineColor = _outlineColor.ToHexString();
        }

        public void OnAfterDeserialize() {
            _text = WebUtility.UrlDecode(text);
            _font = DecalConfig.GetFont(fontName);
            _style = new DecalTextStyle((FontStyles) style, vertical, lineSpacing, charSpacing);

            if (!ParseUtil.TryParseColor32(fillColor, out _fillColor)) {
                Logging.LogWarning($"Improperly formatted color value for fill: '{fillColor}'");
                _fillColor = Color.magenta;
            }

            if (!ParseUtil.TryParseColor32(outlineColor, out _outlineColor)) {
                Logging.LogWarning($"Improperly formatted color value for outline: '{outlineColor}'");
                _outlineColor = Color.magenta;
            }
        }

        public override void OnDestroy() {
            if (HighLogic.LoadedSceneIsGame && _currentText != null) TextRenderer.UnregisterText(_currentText);

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
                decal._text = _text;
                decal._font = _font;
                decal._style = _style;

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
            var newText = new DecalText(_text, _font, _style);
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
            _fillColorProperty.color = _fillColor;

            _outlineEnabledProperty.value = outlineEnabled;
            _outlineColorProperty.color = _outlineColor;
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