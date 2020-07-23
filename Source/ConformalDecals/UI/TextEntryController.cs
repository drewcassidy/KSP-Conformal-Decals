using System;
using ConformalDecals.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ConformalDecals.UI {
    public class TextEntryController : MonoBehaviour {
        [Serializable]
        public class TextUpdateEvent : UnityEvent<DecalText> { }

        [SerializeField] public TextUpdateEvent onTextUpdate = new TextUpdateEvent();

        [SerializeField] private Selectable _textBox;
        [SerializeField] private Button     _fontButton;
        [SerializeField] private Slider     _outlineWidthSlider;

        [SerializeField] private Toggle _boldButton;
        [SerializeField] private Toggle _italicButton;
        [SerializeField] private Toggle _underlineButton;
        [SerializeField] private Toggle _smallCapsButton;
        [SerializeField] private Toggle _verticalButton;
        
        private DecalText          _decalText;
        private FontMenuController _fontMenu;

        public static TextEntryController Create(DecalText text, UnityAction<DecalText> textUpdateCallback) {
            var window = Instantiate(UILoader.TextEntryPrefab, MainCanvasUtil.MainCanvas.transform, true);
            window.AddComponent<DragPanel>();
            MenuNavigation.SpawnMenuNavigation(window, Navigation.Mode.Automatic, true);

            var controller = window.GetComponent<TextEntryController>();
            controller._decalText = text;
            controller.onTextUpdate.AddListener(textUpdateCallback);

            return controller;
        }

        private void Start() {
            ((TMP_InputField) _textBox).text = _decalText.text;
            
            _decalText.font.SetupSample(_fontButton.GetComponentInChildren<TextMeshProUGUI>());

            _outlineWidthSlider.value = _decalText.outlineWidth;
            _boldButton.isOn = (_decalText.style & FontStyles.Bold) != 0;
            _italicButton.isOn = (_decalText.style & FontStyles.Italic) != 0;
            _underlineButton.isOn = (_decalText.style & FontStyles.Underline) != 0;
            _smallCapsButton.isOn = (_decalText.style & FontStyles.SmallCaps) != 0;
            _verticalButton.isOn = _decalText.vertical;

        }

        public void OnClose() {
            if (_fontMenu != null) _fontMenu.OnClose();
            Destroy(gameObject);
        }

        public void OnAnyUpdate() {
            onTextUpdate.Invoke(_decalText);
        }

        public void OnTextUpdate(string newText) {
            this._decalText.text = newText;

            OnAnyUpdate();
        }

        public void OnFontMenu() {
            if (_fontMenu == null) _fontMenu = FontMenuController.Create(DecalConfig.Fonts, _decalText.font, OnFontUpdate);
        }

        public void OnFontUpdate(DecalFont font) {
            _decalText.font = font;
            font.SetupSample(_fontButton.GetComponentInChildren<TextMeshProUGUI>());

            var textBox = ((TMP_InputField) _textBox);
            textBox.textComponent.fontStyle = _decalText.style | _decalText.font.fontStyle;
            textBox.fontAsset = _decalText.font.fontAsset;

            OnAnyUpdate();
        }

        public void OnColorMenu() { }

        public void OnColorUpdate(Color color) {
            _decalText.color = color;
            OnAnyUpdate();
        }

        public void OnOutlineColorMenu() { }

        public void OnOutlineColorUpdate(Color color) {
            _decalText.outlineColor = color;
            OnAnyUpdate();
        }

        public void OnOutlineUpdate(float value) {
            _decalText.outlineWidth = value;
            OnAnyUpdate();
        }

        public void OnBoldUpdate(bool state) {
            if (state) _decalText.style |= FontStyles.Bold;
            else _decalText.style &= ~FontStyles.Bold;

            ((TMP_InputField) _textBox).textComponent.fontStyle = _decalText.style | _decalText.font.fontStyle;

            OnAnyUpdate();

        }

        public void OnItalicUpdate(bool state) {
            if (state) _decalText.style |= FontStyles.Italic;
            else _decalText.style &= ~FontStyles.Italic;

            ((TMP_InputField) _textBox).textComponent.fontStyle = _decalText.style | _decalText.font.fontStyle;

            OnAnyUpdate();

        }

        public void OnUnderlineUpdate(bool state) {
            if (state) _decalText.style |= FontStyles.Underline;
            else _decalText.style &= ~FontStyles.Underline;

            ((TMP_InputField) _textBox).textComponent.fontStyle = _decalText.style | _decalText.font.fontStyle;

            OnAnyUpdate();

        }

        public void OnSmallCapsUpdate(bool state) {
            if (state) _decalText.style |= FontStyles.SmallCaps;
            else _decalText.style &= ~FontStyles.SmallCaps;

            ((TMP_InputField) _textBox).textComponent.fontStyle = _decalText.style | _decalText.font.fontStyle;

            OnAnyUpdate();

        }

        public void OnVerticalUpdate(bool state) {
            _decalText.vertical = state;
            OnAnyUpdate();
        }
    }
}