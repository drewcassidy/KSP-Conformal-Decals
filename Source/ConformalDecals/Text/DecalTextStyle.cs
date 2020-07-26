using System;
using ConformalDecals.Util;
using TMPro;
using UnityEngine;

// ReSharper disable NonReadonlyMemberInGetHashCode

namespace ConformalDecals.Text {
    public class DecalTextStyle : ScriptableObject, IEquatable<DecalTextStyle> {
        private FontStyles _fontStyle;
        private bool       _vertical;
        private float      _lineSpacing;
        private float      _characterSpacing;

        public FontStyles FontStyle {
            get => _fontStyle;
            set => _fontStyle = value;
        }

        public bool Bold {
            get => (FontStyle & FontStyles.Bold) != 0;
            set {
                if (value) FontStyle |= FontStyles.Bold;
                else FontStyle &= ~FontStyles.Bold;
            }
        }

        public bool Italic {
            get => (FontStyle & FontStyles.Italic) != 0;
            set {
                if (value) FontStyle |= FontStyles.Italic;
                else FontStyle &= ~FontStyles.Italic;
            }
        }

        public bool Underline {
            get => (FontStyle & FontStyles.Underline) != 0;
            set {
                if (value) FontStyle |= FontStyles.Underline;
                else FontStyle &= ~FontStyles.Underline;
            }
        }

        public bool SmallCaps {
            get => (FontStyle & FontStyles.SmallCaps) != 0;
            set {
                if (value) FontStyle |= FontStyles.SmallCaps;
                else FontStyle &= ~FontStyles.SmallCaps;
            }
        }

        public bool Vertical {
            get => _vertical;
            set => _vertical = value;
        }

        public float LineSpacing {
            get => _lineSpacing;
            set => _lineSpacing = value;
        }

        public float CharacterSpacing {
            get => _characterSpacing;
            set => _characterSpacing = value;
        }

        public static DecalTextStyle Load(ConfigNode node) {
            var style = CreateInstance<DecalTextStyle>();
            style._fontStyle = (FontStyles) ParseUtil.ParseInt(node, "fontStyle", true);
            style._vertical = ParseUtil.ParseBool(node, "vertical", true);
            style._lineSpacing = ParseUtil.ParseFloat(node, "lineSpacing", true);
            style._characterSpacing = ParseUtil.ParseFloat(node, "characterSpacing", true);
            return style;
        }

        public ConfigNode Save() {
            var node = new ConfigNode("STYLE");
            node.AddValue("fontStyle", (int) _fontStyle);
            node.AddValue("vertical", _vertical);
            node.AddValue("lineSpacing", _lineSpacing);
            node.AddValue("characterSpacing", _characterSpacing);
            return node;
        }

        public bool Equals(DecalTextStyle other) {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && _fontStyle == other._fontStyle && _vertical == other._vertical && _lineSpacing.Equals(other._lineSpacing) && _characterSpacing.Equals(other._characterSpacing);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DecalTextStyle) obj);
        }

        public override int GetHashCode() {
            unchecked {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) _fontStyle;
                hashCode = (hashCode * 397) ^ _vertical.GetHashCode();
                hashCode = (hashCode * 397) ^ _lineSpacing.GetHashCode();
                hashCode = (hashCode * 397) ^ _characterSpacing.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(DecalTextStyle left, DecalTextStyle right) {
            return Equals(left, right);
        }

        public static bool operator !=(DecalTextStyle left, DecalTextStyle right) {
            return !Equals(left, right);
        }
    }
}