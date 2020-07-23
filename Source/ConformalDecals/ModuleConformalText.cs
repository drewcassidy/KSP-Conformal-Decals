using ConformalDecals.Text;
using ConformalDecals.UI;
using TMPro;
using UnityEngine;

namespace ConformalDecals {
    public class ModuleConformalText : ModuleConformalDecal {
        [KSPField(isPersistant = true)] public string text = "Hello World!";
        [KSPField(isPersistant = true)] public string font = "Calibri SDF";
        [KSPField(isPersistant = true)] public int    style;
        [KSPField(isPersistant = true)] public bool   vertical;
        [KSPField(isPersistant = true)] public Color  fillColor    = Color.black;
        [KSPField(isPersistant = true)] public Color  outlineColor = Color.white;
        [KSPField(isPersistant = true)] public float  outlineWidth;

        private DecalText _text;

        private TextEntryController   _textEntryController;
        private ColorPickerController _fillColorPickerController;
        private ColorPickerController _outlineColorPickerCOntroller;

        public override void OnStart(StartState state) {
            base.OnStart(state);

            var decalFont = DecalConfig.GetFont(font);

            _text = new DecalText {
                text = text,
                font = decalFont,
                style = (FontStyles) style,
                vertical = vertical,
                color = fillColor,
                outlineColor = outlineColor,
                outlineWidth = outlineWidth
            };
        }

        public void OnTextUpdate(DecalText newText) {
            _text = newText;
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
                _textEntryController = TextEntryController.Create(_text, OnTextUpdate);
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