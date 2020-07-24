using System;
using System.Text.RegularExpressions;

namespace ConformalDecals.Text {
    public class DecalText : IEquatable<DecalText> {
        public string Text { get; }

        public DecalFont Font { get; }

        public DecalTextStyle Style { get; }

        public string FormattedText {
            get {
                if (Style.Vertical) {
                    return Regex.Replace(Text, @"(.)", "$1\n");
                }
                else {
                    return Text;
                }
            }
        }

        public DecalText(string text, DecalFont font, DecalTextStyle style) {
            Text = text;
            Font = font;
            Style = style;
        }

        public bool Equals(DecalText other) {
            return other != null && (Text == other.Text && Equals(Font, other.Font) && Style.Equals(other.Style));
        }

        public override bool Equals(object obj) {
            return obj is DecalText other && Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (Text != null ? Text.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Font != null ? Font.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Style.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(DecalText left, DecalText right) {
            return left != null && left.Equals(right);
        }

        public static bool operator !=(DecalText left, DecalText right) {
            return left != null && !left.Equals(right);
        }
    }
}