using System;
using TMPro;

namespace ConformalDecals.Text {
    public class DecalFont {
        public readonly string        title;
        public readonly TMP_FontAsset fontAsset;
        public readonly FontStyles    fontStyle;

        public DecalFont(string title, TMP_FontAsset fontAsset, FontStyles fontStyle) {
            if (fontAsset == null) throw new ArgumentNullException(nameof(fontAsset));
            
            this.title = title;
            this.fontAsset = fontAsset;
            this.fontStyle = fontStyle;
        }

        public void SetupSample(TMP_Text tmp) {
            if (tmp == null) throw new ArgumentNullException(nameof(tmp));
            if (fontAsset == null) throw new InvalidOperationException("DecalFont has not been initialized and Font is null.");

            tmp.text = title;
            tmp.font = fontAsset;
            tmp.fontStyle = fontStyle;
        }
    }
}