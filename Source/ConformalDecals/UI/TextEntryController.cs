using System;
using ConformalDecals.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ConformalDecals.UI {
    public class TextEntryController : MonoBehaviour {
        [Serializable]
        public class TextUpdateEvent : UnityEvent<string, DecalFont, DecalTextStyle> { }

        [SerializeField] public TextUpdateEvent onValueChanged = new TextUpdateEvent();

        [SerializeField] private Selectable _textBox;
        [SerializeField] private Button     _fontButton;

        [SerializeField] private Toggle _boldButton;
        [SerializeField] private Toggle _italicButton;
        [SerializeField] private Toggle _underlineButton;
        [SerializeField] private Toggle _smallCapsButton;
        [SerializeField] private Toggle _verticalButton;

        private string         _text;
        private DecalFont      _font;
        private DecalTextStyle _style;

        private FontMenuController _fontMenu;

        public static TextEntryController Create(string text, DecalFont font, DecalTextStyle style, UnityAction<string, DecalFont, DecalTextStyle> textUpdateCallback) {

            var window = Instantiate(UILoader.TextEntryPrefab, MainCanvasUtil.MainCanvas.transform, true);
            window.AddComponent<DragPanel>();
            MenuNavigation.SpawnMenuNavigation(window, Navigation.Mode.Automatic, true);

            var controller = window.GetComponent<TextEntryController>();
            controller._text = text;
            controller._font = font;
            controller._style = style;
            controller.onValueChanged.AddListener(textUpdateCallback);

            return controller;
        }


        public void OnClose() {
            if (_fontMenu != null) _fontMenu.OnClose();
            Destroy(gameObject);
        }

        public void OnTextUpdate(string newText) {
            this._text = newText;

            OnValueChanged();
        }

        public void OnFontMenu() {
            if (_fontMenu == null) _fontMenu = FontMenuController.Create(DecalConfig.Fonts, _font, OnFontUpdate);
        }

        public void OnFontUpdate(DecalFont font) {
            _font = font;
            font.SetupSample(_fontButton.GetComponentInChildren<TextMeshProUGUI>());

            var textBox = ((TMP_InputField) _textBox);
            textBox.textComponent.fontStyle = _style.FontStyle | _font.FontStyle;
            textBox.fontAsset = _font.FontAsset;

            UpdateStyleButtons();
            OnValueChanged();
        }

        public void OnBoldUpdate(bool state) {
            _style.Bold = state;

            OnValueChanged();
        }

        public void OnItalicUpdate(bool state) {
            _style.Italic = state;
            OnValueChanged();

        }

        public void OnUnderlineUpdate(bool state) {
            _style.Underline = state;
            OnValueChanged();

        }

        public void OnSmallCapsUpdate(bool state) {
            _style.SmallCaps = state;
            OnValueChanged();

        }

        public void OnVerticalUpdate(bool state) {
            _style.Vertical = state;
            OnValueChanged();
        }
        
        
        private void Start() {
            ((TMP_InputField) _textBox).text = _text;

            _font.SetupSample(_fontButton.GetComponentInChildren<TextMeshProUGUI>());

            _boldButton.isOn = _style.Bold;
            _italicButton.isOn = _style.Italic;
            _underlineButton.isOn = _style.Underline;
            _smallCapsButton.isOn = _style.SmallCaps;
            _verticalButton.isOn = _style.Vertical;
            UpdateStyleButtons();
        }

        private void OnValueChanged() {
            onValueChanged.Invoke(_text, _font, _style);
        }

        private void UpdateStyleButtons() {
            _boldButton.interactable = !_font.Bold && !_font.BoldMask;
            _italicButton.interactable = !_font.Italic && !_font.ItalicMask;
            _underlineButton.interactable = !_font.Underline && !_font.UnderlineMask;
            _smallCapsButton.interactable = !_font.SmallCaps && !_font.SmallCapsMask;
        }
    }
}