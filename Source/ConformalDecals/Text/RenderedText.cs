using UnityEngine;

namespace ConformalDecals.Text {
    public class RenderedText {
        public Texture2D Texture { get; private set; }

        public Rect Window { get; private set; }

        public int UserCount { get; set; }

        public RenderedText(Texture2D texture, Rect window) {
            Texture = texture;
            Window = window;
        }
    }
}