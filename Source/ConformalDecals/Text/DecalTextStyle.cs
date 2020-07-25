using System;
using TMPro;
using UnityEngine;
// ReSharper disable NonReadonlyMemberInGetHashCode

namespace ConformalDecals.Text {
    public struct DecalTextStyle : IEquatable<DecalTextStyle> {
        public FontStyles FontStyle { get; set; }

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

        public bool Vertical { get; set; }

        public float LineSpacing { get; set; }

        public float CharacterSpacing { get; set; }

        public bool Equals(DecalTextStyle other) {
            return FontStyle == other.FontStyle && Vertical == other.Vertical && 
                   Mathf.Approximately(LineSpacing, other.LineSpacing) &&
                   Mathf.Approximately(CharacterSpacing, other.CharacterSpacing);
        }

        public override bool Equals(object obj) {
            return obj is DecalTextStyle other && Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (int) FontStyle;
                hashCode = (hashCode * 397) ^ Vertical.GetHashCode();
                hashCode = (hashCode * 397) ^ LineSpacing.GetHashCode();
                hashCode = (hashCode * 397) ^ CharacterSpacing.GetHashCode();
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