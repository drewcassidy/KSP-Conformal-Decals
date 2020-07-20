using UnityEngine;
using UnityEngine.UI;

namespace ConformalDecals.UI {
    public class TextEntryController : MonoBehaviour {
        [SerializeField] private Selectable _textBox;
        [SerializeField] private Button     _fontButton;
        [SerializeField] private Slider     _outlineWidthSlider;

        [SerializeField] private Toggle _boldButton;
        [SerializeField] private Toggle _italicButton;
        [SerializeField] private Toggle _underlineButton;
        [SerializeField] private Toggle _smallCapsButton;
        [SerializeField] private Toggle _verticalButton;

        public void OnClose() { }

        public void OnTextUpdate(string text) { }

        public void OnFontMenu() { }
        public void OnColorMenu() { }
        public void OnOutlineColorMenu() { }

        public void OnOutlineUpdate(float value) { }

        public void OnBoldUpdate(bool state) { }
        public void OnItalicUpdate(bool state) { }
        public void OnUnderlineUpdate(bool state) { }
        public void OnSmallCapsUpdate(bool state) { }
        public void OnVerticalUpdate(bool state) { }
    }
}