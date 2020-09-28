using System;
using System.Text.RegularExpressions;

namespace ConformalDecals.Text {
    public class DecalText : IEquatable<DecalText> {
        /// Raw text contents
        public string Text { get; }

        /// Font asset used by this text snippet
        public DecalFont Font { get; }

        /// Style used by this text snippet
        public DecalTextStyle Style { get; }

        /// The text formatted with newlines for vertical text
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
            if (font == null) throw new ArgumentNullException(nameof(font));
            Text = text;
            Font = font;
            Style = style;
        }

        public bool Equals(DecalText other) {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Text == other.Text && Equals(Font, other.Font) && Style.Equals(other.Style);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DecalText) obj);
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
            return Equals(left, right);
        }

        public static bool operator !=(DecalText left, DecalText right) {
            return !Equals(left, right);
        }
    }
}