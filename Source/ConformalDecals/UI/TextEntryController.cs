using System;
using ConformalDecals.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ConformalDecals.UI {
    public class TextEntryController : MonoBehaviour {
        private FormattedText _text;

        [SerializeField] private Selectable _textBox;
        [SerializeField] private Toggle     _fontColorButton;
        [SerializeField] private Toggle     _fontButton;
        [SerializeField] private Toggle     _outlineColorButton;
        [SerializeField] private Slider     _outlineWidthSlider;

        [SerializeField] private Toggle _boldButton;
        [SerializeField] private Toggle _italicButton;
        [SerializeField] private Toggle _underlineButton;
        [SerializeField] private Toggle _smallCapsButton;
        [SerializeField] private Toggle _verticalButton;

        public delegate void TextUpdateReceiver(FormattedText text);

        public delegate void TextCancelReceiver();

        public TextUpdateReceiver textUpdateCallback;
        public TextCancelReceiver textCancelCallback;

        private void Start() {
            (_textBox as TMP_InputField).text = _text.text;

            _boldButton.isOn = (_text.style | FontStyles.Bold) != 0;
            _italicButton.isOn = (_text.style | FontStyles.Italic) != 0;
            _underlineButton.isOn = (_text.style | FontStyles.Underline) != 0;
            _smallCapsButton.isOn = (_text.style | FontStyles.SmallCaps) != 0;
            _verticalButton.isOn = _text.vertical;
        }

        public void Close() {
            Destroy(gameObject);
        }

        public void OnCancel() {
            textCancelCallback();
            Close();
        }

        public void OnApply() {
            textUpdateCallback(_text);
            Close();
        }

        public void OnTextUpdate(string text) {
            _text.text = text;
            textUpdateCallback(_text);

        }

        public void OnFontMenu(bool state) { }
        public void OnColorMenu(bool state) { }

        public void OnOutlineColorMenu(bool state) { }

        public void OnOutlineUpdate(float value) {
            _text.outlineWidth = value;
            textUpdateCallback(_text);

        }

        public void OnBoldUpdate(bool state) {
            if (state) _text.style |= FontStyles.Bold;
            else _text.style &= ~FontStyles.Bold;

            textUpdateCallback(_text);

        }

        public void OnItalicUpdate(bool state) {
            if (state) _text.style |= FontStyles.Italic;
            else _text.style &= ~FontStyles.Italic;

            textUpdateCallback(_text);

        }

        public void OnUnderlineUpdate(bool state) {
            if (state) _text.style |= FontStyles.Underline;
            else _text.style &= ~FontStyles.Underline;

            textUpdateCallback(_text);

        }

        public void OnSmallCapsUpdate(bool state) {
            if (state) _text.style |= FontStyles.SmallCaps;
            else _text.style &= ~FontStyles.SmallCaps;

            textUpdateCallback(_text);

        }

        public void OnVerticalUpdate(bool state) {
            _text.vertical = state;
            textUpdateCallback(_text);
        }
    }
}