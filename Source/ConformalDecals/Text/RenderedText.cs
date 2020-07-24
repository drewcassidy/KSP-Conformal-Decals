using UnityEngine;

namespace ConformalDecals.Text {
    public class RenderedText : ScriptableObject {
        public Texture2D Texture { get; private set; }

        public Rect Window { get; private set; }

        public int UserCount { get; private set; }
    }
}