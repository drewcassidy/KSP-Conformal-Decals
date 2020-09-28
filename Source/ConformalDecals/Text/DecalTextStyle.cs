using System;
using TMPro;
using UnityEngine;

// ReSharper disable NonReadonlyMemberInGetHashCode

namespace ConformalDecals.Text {
    public struct DecalTextStyle : IEquatable<DecalTextStyle> {
        private FontStyles _fontStyle;
        private bool       _vertical;
        private float      _lineSpacing;
        private float      _charSpacing;

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

        public float CharSpacing {
            get => _charSpacing;
            set => _charSpacing = value;
        }

        public DecalTextStyle(FontStyles fontStyle, bool vertical, float lineSpacing, float charSpacing) {
            _fontStyle = fontStyle;
            _vertical = vertical;
            _lineSpacing = lineSpacing;
            _charSpacing = charSpacing;
        }

        public bool Equals(DecalTextStyle other) {
            return FontStyle == other.FontStyle && Vertical == other.Vertical &&
                   Mathf.Approximately(LineSpacing, other.LineSpacing) &&
                   Mathf.Approximately(CharSpacing, other.CharSpacing);
        }

        public override bool Equals(object obj) {
            return obj is DecalTextStyle other && Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (int) FontStyle;
                hashCode = (hashCode * 397) ^ Vertical.GetHashCode();
                hashCode = (hashCode * 397) ^ LineSpacing.GetHashCode();
                hashCode = (hashCode * 397) ^ CharSpacing.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(DecalTextStyle left, DecalTextStyle right) {
            return left.Equals(right);
        }

        public static bool operator !=(DecalTextStyle left, DecalTextStyle right) {
            return !left.Equals(right);
        }
    }
}