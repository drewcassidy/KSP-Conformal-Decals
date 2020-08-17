using ConformalDecals.MaterialProperties;
using ConformalDecals.Text;
using ConformalDecals.UI;
using TMPro;
using UnityEngine;

namespace ConformalDecals {
    public class ModuleConformalText : ModuleConformalDecal, ISerializationCallbackReceiver {
        [KSPField(isPersistant = true)] public string text         = "Hello World!";
        [KSPField(isPersistant = true)] public Color  fillColor    = Color.black;
        [KSPField(isPersistant = true)] public Color  outlineColor = Color.white;

        // serialization-only fields. do not use except in serialization functions
        [KSPField(isPersistant = true)] public string fontName = "Calibri SDF";
        [KSPField(isPersistant = true)] public int    style;
        [KSPField(isPersistant = true)] public bool   vertical;
        [KSPField(isPersistant = true)] public float  lineSpacing;
        [KSPField(isPersistant = true)] public float  characterSpacing;

        // KSP TWEAKABLES

        [KSPEvent(guiName = "#LOC_ConformalDecals_gui-set-text", guiActive = false, guiActiveEditor = true)]
        public void SetText() {
            if (_textEntryController == null) {
                _textEntryController = TextEntryController.Create(text, _font, _style, OnTextUpdate);
            }
            else {
                _textEntryController.OnClose();
            }
        }

        // FILL

        [KSPField(guiName = "#LOC_ConformalDecals_gui-fill", groupName = "decal-fill", groupDisplayName = "#LOC_ConformalDecals_gui-group-fill",
             guiActive = false, guiActiveEditor = true, isPersistant = true),
         UI_Toggle()]
        public bool fillEnabled = true;

        [KSPEvent(guiName = "#LOC_ConformalDecals_gui-fill-color", groupName = "decal-fill", groupDisplayName = "#LOC_ConformalDecals_gui-group-fill",
            guiActive = false, guiActiveEditor = true)]
        public void SetFillColor() {
            if (_fillColorPickerController == null) {
                _fillColorPickerController = ColorPickerController.Create(fillColor, OnFillColorUpdate);
            }
            else {
                _fillColorPickerController.OnClose();
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
                _outlineColorPickerController.OnClose();
            }
        }

        private DecalTextStyle _style;
        private DecalFont      _font;

        private TextEntryController   _textEntryController;
        private ColorPickerController _fillColorPickerController;
        private ColorPickerController _outlineColorPickerController;

        private MaterialTextureProperty _decalTextureProperty;
        private MaterialFloatProperty   _decalTextWeightProperty;

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

            UpdateTextRecursive();
        }

        public override void OnSave(ConfigNode node) {
            OnBeforeSerialize();
            base.OnSave(node);
        }

        public override void OnStart(StartState state) {
            base.OnStart(state);

            UpdateTextRecursive();
        }

        public override void OnAwake() {
            base.OnAwake();

            _decalTextureProperty = materialProperties.AddOrGetTextureProperty("_Decal", true);
            _decalTextWeightProperty = materialProperties.AddOrGetProperty<MaterialFloatProperty>("_Weight");

            _fillEnabledProperty = materialProperties.AddOrGetProperty<MaterialKeywordProperty>("DECAL_FILL");
            _fillColorProperty = materialProperties.AddOrGetProperty<MaterialColorProperty>("_DecalColor");

            _outlineEnabledProperty = materialProperties.AddOrGetProperty<MaterialKeywordProperty>("DECAL_OUTLINE");
            _outlineColorProperty = materialProperties.AddOrGetProperty<MaterialColorProperty>("_OutlineColor");
            _outlineWidthProperty = materialProperties.AddOrGetProperty<MaterialFloatProperty>("_OutlineWidth");
        }

        public void OnTextUpdate(string newText, DecalFont newFont, DecalTextStyle newStyle) {
            text = newText;
            _font = newFont;
            _style = newStyle;
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

            foreach (var counterpart in part.symmetryCounterparts) {
                var decal = counterpart.GetComponent<ModuleConformalText>();
                decal.fillEnabled = fillEnabled;
                decal.outlineEnabled = outlineEnabled;
                decal.UpdateTweakables();
            }
        }

        public void OnOutlineToggle(BaseField field, object obj) {
            // fill and outline cant both be disabled
            fillEnabled = fillEnabled || (!fillEnabled && !outlineEnabled);

            UpdateTweakables();

            foreach (var counterpart in part.symmetryCounterparts) {
                var decal = counterpart.GetComponent<ModuleConformalText>();
                decal.fillEnabled = fillEnabled;
                decal.outlineEnabled = outlineEnabled;
                decal.UpdateTweakables();
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
            fontName = _font.Name;
            style = (int) _style.FontStyle;
            vertical = _style.Vertical;
            lineSpacing = _style.LineSpacing;
            characterSpacing = _style.CharacterSpacing;
        }

        public void OnAfterDeserialize() {
            _font = DecalConfig.GetFont(fontName);
            _style = new DecalTextStyle((FontStyles) style, vertical, lineSpacing, characterSpacing);
        }

        public override void OnDestroy() {
            if (_currentText != null) TextRenderer.UnregisterText(_currentText);

            base.OnDestroy();
        }

        protected override void OnDetach() {
            // close all UIs
            if (_textEntryController != null) _textEntryController.OnClose();
            if (_fillColorPickerController != null) _fillColorPickerController.OnClose();
            if (_outlineColorPickerController != null) _outlineColorPickerController.OnClose();

            base.OnDetach();
        }

        private void UpdateTextRecursive() {
            UpdateText();

            foreach (var counterpart in part.symmetryCounterparts) {
                var decal = counterpart.GetComponent<ModuleConformalText>();
                decal.text = text;
                decal._font = _font;
                decal._style = _style;

                decal._currentJob = _currentJob;
                decal._currentText = _currentText;
                decal.UpdateText();
            }
        }

        private void UpdateText() {
            // Render text
            var newText = new DecalText(text, _font, _style);
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

        protected void UpdateCachedProperties() { }
    }
}