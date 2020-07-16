using TMPro;
using UnityEngine;

namespace ConformalDecals.Text {
    public struct FormattedText {
        public string        text;
        public TMP_FontAsset font;
        public FontStyles    style;
        public bool          vertical;

        public Color32 color;
        public Color32 outlineColor;
        public float   outlineWidth;
    }
}