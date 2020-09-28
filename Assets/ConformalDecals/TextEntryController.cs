using UnityEngine;
using UnityEngine.UI;

namespace ConformalDecals.UI {
    public class TextEntryController : MonoBehaviour {
        [SerializeField] private Selectable _textBox;
        [SerializeField] private Button     _fontButton;

        [SerializeField] private Slider _lineSpacingSlider;
        [SerializeField] private Selectable _lineSpacingTextBox;

        [SerializeField] private Slider _charSpacingSlider;
        [SerializeField] private Selectable _charSpacingTextBox;
        
        [SerializeField] private Toggle _boldButton;
        [SerializeField] private Toggle _italicButton;
        [SerializeField] private Toggle _underlineButton;
        [SerializeField] private Toggle _smallCapsButton;
        [SerializeField] private Toggle _verticalButton;

        public void Close() { }

        public void OnTextUpdate(string text) { }

        public void OnFontMenu() { }

        public void OnLineSpacingUpdate(float value) { }
        public void OnLineSpacingUpdate(string text) { }
        public void OnCharSpacingUpdate(float value) { }
        public void OnCharSpacingUpdate(string text) { }
        public void OnBoldUpdate(bool state) { }
        public void OnItalicUpdate(bool state) { }
        public void OnUnderlineUpdate(bool state) { }
        public void OnSmallCapsUpdate(bool state) { }
        public void OnVerticalUpdate(bool state) { }
    }
}