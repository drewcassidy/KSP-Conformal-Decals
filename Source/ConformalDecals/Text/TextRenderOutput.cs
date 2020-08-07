using UnityEngine;

namespace ConformalDecals.Text {
    public class TextRenderOutput {
        public Texture2D Texture { get; private set; }

        public Rect Window { get; private set; }
        
        public float Weight { get; private set; }

        public int UserCount { get; set; }

        public TextRenderOutput(Texture2D texture, Rect window, float weight) {
            Texture = texture;
            Window = window;
            Weight = weight;
        }
    }
}