using System;
using TMPro;
using UnityEngine;

namespace ConformalDecals.Text {
    public struct DecalText {
        public string     text;
        public DecalFont  font;
        public FontStyles style;
        public bool       vertical;

        public Color color;
        public Color outlineColor;
        public float outlineWidth;
    }
}