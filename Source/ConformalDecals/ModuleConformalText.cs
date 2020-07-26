using ConformalDecals.Text;
using ConformalDecals.UI;
using UnityEngine;

namespace ConformalDecals {
    public class ModuleConformalText : ModuleConformalDecal {
        [KSPField(isPersistant = true)] public string text = "Hello World!";
        [KSPField(isPersistant = true)] public string fontName = "Calibri SDF";
        [KSPField(isPersistant = true)] public int    style;
        [KSPField(isPersistant = true)] public bool   vertical;
        [KSPField(isPersistant = true)] public Color  fillColor    = Color.black;
        [KSPField(isPersistant = true)] public Color  outlineColor = Color.white;
        [KSPField(isPersistant = true)] public float  outlineWidth;

        private DecalTextStyle _style;
        private DecalFont _font;
        
        private TextEntryController   _textEntryController;
        private ColorPickerController _fillColorPickerController;
        private ColorPickerController _outlineColorPickerCOntroller;

        public override void OnStart(StartState state) {
            base.OnStart(state);

            _font = DecalConfig.GetFont(fontName);
            _style = new DecalTextStyle();

            var decalText = new DecalText("Hello World!", _font, _style);
            
            //TextRenderer.Instance.RenderText(decalText, out var texture, out var window);
            //materialProperties.AddOrGetTextureProperty("_Decal", true).Texture = texture;
            UpdateMaterials();
            UpdateScale();
        }

        public void OnTextUpdate(string newText, DecalFont newFont, DecalTextStyle newStyle) {
            text = newText;
            _font = newFont;
            _style = newStyle;
        }

        public void OnFillColorUpdate(Color rgb, Util.ColorHSV hsv) {
            Debug.Log($"new fill color: {rgb}, {hsv}");
        }

        public void OnOutlineColorUpdate(Color rgb, Util.ColorHSV hsv) {
            Debug.Log($"new outline color: {rgb}, {hsv}");
        }

        [KSPEvent(guiActive = false, guiActiveEditor = true, guiName = "#LOC_ConformalDecals_gui-select-flag")]
        public void SetText() {
            if (_textEntryController == null) {
                _textEntryController = TextEntryController.Create(text, _font, _style, OnTextUpdate);
            }
        }

        [KSPEvent(guiActive = false, guiActiveEditor = true, guiName = "Set Fill Color")]
        public void SetFillColor() {
            if (_fillColorPickerController == null) {
                _fillColorPickerController = ColorPickerController.Create(fillColor, OnFillColorUpdate);
            }
        }

        [KSPEvent(guiActive = false, guiActiveEditor = true, guiName = "Set Outline Color")]
        public void SetOutlineColor() {
            if (_outlineColorPickerCOntroller == null) {
                _outlineColorPickerCOntroller = ColorPickerController.Create(outlineColor, OnOutlineColorUpdate);
            }
        }
    }
}