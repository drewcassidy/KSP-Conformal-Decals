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
            if (_outlineColorPickerCOntroller == null) {
                _outlineColorPickerCOntroller = ColorPickerController.Create(outlineColor, OnOutlineColorUpdate);
            }
        }

        private DecalTextStyle _style;
        private DecalFont      _font;

        private TextEntryController   _textEntryController;
        private ColorPickerController _fillColorPickerController;
        private ColorPickerController _outlineColorPickerCOntroller;

        private TextRenderJob _currentJob;
        private DecalText     _currentText;

        public override void OnLoad(ConfigNode node) {
            base.OnLoad(node);
            OnAfterDeserialize();
        }

        public override void OnSave(ConfigNode node) {
            OnBeforeSerialize();
            base.OnSave(node);
        }

        public override void OnStart(StartState state) {
            base.OnStart(state);

            UpdateTextRecursive();
        }

        public void OnTextUpdate(string newText, DecalFont newFont, DecalTextStyle newStyle) {
            text = newText;
            _font = newFont;
            _style = newStyle;
            UpdateTextRecursive();
        }

        public void OnFillColorUpdate(Color rgb, Util.ColorHSV hsv) {
            fillColor = rgb;
            Debug.Log($"new fill color: {rgb}, {hsv}");
        }

        public void OnOutlineColorUpdate(Color rgb, Util.ColorHSV hsv) {
            outlineColor = rgb;
            Debug.Log($"new outline color: {rgb}, {hsv}");
        }

        public void OnFillToggle() {
            if (!fillEnabled && !outlineEnabled) {
                outlineEnabled = true;
                OnOutlineToggle();
            }
        }

        public void OnOutlineToggle() {
            if (!fillEnabled && !outlineEnabled) {
                fillEnabled = true;
                OnFillToggle();
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
            TextRenderer.UnregisterText(_currentText);
            base.OnDestroy();
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
            var textureProperty = materialProperties.AddOrGetTextureProperty("_Decal", true);
            textureProperty.Texture = output.Texture;
            textureProperty.SetTile(output.Window);
            UpdateMaterials();
        }

        protected override void UpdateMaterials() {
            materialProperties.AddOrGetProperty<MaterialKeywordProperty>("DECAL_FILL").value = fillEnabled;
            materialProperties.AddOrGetProperty<MaterialKeywordProperty>("DECAL_OUTLINE").value = outlineEnabled;
            if (fillEnabled) {
                materialProperties.AddOrGetProperty<MaterialColorProperty>("_DecalColor").color = fillColor;
            }

            if (outlineEnabled) {
                materialProperties.AddOrGetProperty<MaterialColorProperty>("_OutlineColor").color = outlineColor;
                materialProperties.AddOrGetProperty<MaterialFloatProperty>("_OutlineWidth").value = outlineWidth;
            }

            base.UpdateMaterials();
        }
    }
}