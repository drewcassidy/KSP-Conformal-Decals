using System;
using System.Collections.Generic;
using ConformalDecals.Util;
using TMPro;
using UniLinq;

namespace ConformalDecals.Text {
    public class DecalFont : IEquatable<DecalFont> {
        public string Title { get; }

        public TMP_FontAsset FontAsset { get; }

        public string Name => FontAsset.name;


        public FontStyles FontStyle { get; }

        public bool Bold => (FontStyle & FontStyles.Bold) != 0;

        public bool Italic => (FontStyle & FontStyles.Italic) != 0;

        public bool Underline => (FontStyle & FontStyles.Underline) != 0;

        public bool SmallCaps => (FontStyle & FontStyles.SmallCaps) != 0;


        public FontStyles FontStyleMask { get; }

        public bool BoldMask => (FontStyleMask & FontStyles.Bold) != 0;

        public bool ItalicMask => (FontStyleMask & FontStyles.Italic) != 0;

        public bool UnderlineMask => (FontStyleMask & FontStyles.Underline) != 0;

        public bool SmallCapsMask => (FontStyleMask & FontStyles.SmallCaps) != 0;


        public DecalFont(ConfigNode node, IEnumerable<TMP_FontAsset> fontAssets) {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (fontAssets == null) throw new ArgumentNullException(nameof(fontAssets));

            var name = ParseUtil.ParseString(node, "name");
            FontAsset = fontAssets.First(o => o.name == name);
            if (FontAsset == null) {
                throw new FormatException($"Could not find font asset named {name}");
            }

            Title = ParseUtil.ParseString(node, "title", true, name);
            FontStyle = (FontStyles) ParseUtil.ParseInt(node, "style", true);
            FontStyleMask = (FontStyles) ParseUtil.ParseInt(node, "styleMask", true);
        }


        public void SetupSample(TMP_Text tmp) {
            if (tmp == null) throw new ArgumentNullException(nameof(tmp));
            if (FontAsset == null) throw new InvalidOperationException("DecalFont has not been initialized and Font is null.");

            tmp.text = Title;
            tmp.font = FontAsset;
            tmp.fontStyle = FontStyle;
        }

        public bool Equals(DecalFont other) {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Title == other.Title && Equals(FontAsset, other.FontAsset) && FontStyle == other.FontStyle && FontStyleMask == other.FontStyleMask;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DecalFont) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (Title != null ? Title.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (FontAsset != null ? FontAsset.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) FontStyle;
                hashCode = (hashCode * 397) ^ (int) FontStyleMask;
                return hashCode;
            }
        }

        public static bool operator ==(DecalFont left, DecalFont right) {
            return Equals(left, right);
        }

        public static bool operator !=(DecalFont left, DecalFont right) {
            return !Equals(left, right);
        }
    }
}