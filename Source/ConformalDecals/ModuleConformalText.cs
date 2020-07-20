using System;
using ConformalDecals.Text;
using ConformalDecals.UI;
using ConformalDecals.Util;
using TMPro;
using UnityEngine;

namespace ConformalDecals {
    public class ModuleConformalText : ModuleConformalDecal {
        [KSPField(isPersistant = true)] public string text = "Hello World!";
        [KSPField(isPersistant = true)] public string font = "Calibri SDF";
        [KSPField(isPersistant = true)] public int    style;
        [KSPField(isPersistant = true)] public bool   vertical;
        [KSPField(isPersistant = true)] public Color  color        = Color.black;
        [KSPField(isPersistant = true)] public Color  outlineColor = Color.white;
        [KSPField(isPersistant = true)] public float  outlineWidth;

        private DecalText _text;

        private TextEntryController _textEntryController;

        public override void OnLoad(ConfigNode node) {
            base.OnLoad(node);
        }

        public override void OnStart(StartState state) {
            base.OnStart(state);

            var decalFont =  DecalConfig.GetFont(font);

            _text = new DecalText {
                text = text,
                font = decalFont,
                style = (FontStyles) style,
                vertical = vertical,
                color = color,
                outlineColor = outlineColor,
                outlineWidth = outlineWidth
            };
        }

        public void OnTextUpdate(DecalText newText) {
            _text = newText;
        }

        [KSPEvent(guiActive = false, guiActiveEditor = true, guiName = "#LOC_ConformalDecals_gui-select-flag")]
        public void SetText() {
            if (_textEntryController == null) {
                _textEntryController = TextEntryController.Create(_text, OnTextUpdate);
            }
        }
    }
}