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

        private bool _ignoreUpdates;

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
            if (_ignoreUpdates) return;

            _font = font;
            font.SetupSample(_fontButton.GetComponentInChildren<TextMeshProUGUI>());

            var textBox = ((TMP_InputField) _textBox);
            textBox.textComponent.fontStyle = _style.FontStyle | _font.FontStyle;
            textBox.fontAsset = _font.FontAsset;

            UpdateStyleButtons();
            OnValueChanged();
        }

        public void OnBoldUpdate(bool state) {
            if (_ignoreUpdates) return;

            _style.Bold = state;
            OnValueChanged();
        }

        public void OnItalicUpdate(bool state) {
            if (_ignoreUpdates) return;

            _style.Italic = state;
            OnValueChanged();

        }

        public void OnUnderlineUpdate(bool state) {
            if (_ignoreUpdates) return;

            _style.Underline = state;
            OnValueChanged();

        }

        public void OnSmallCapsUpdate(bool state) {
            if (_ignoreUpdates) return;

            _style.SmallCaps = state;
            OnValueChanged();

        }

        public void OnVerticalUpdate(bool state) {
            if (_ignoreUpdates) return;

            _style.Vertical = state;
            OnValueChanged();
        }


        private void Start() {
            ((TMP_InputField) _textBox).text = _text;

            _font.SetupSample(_fontButton.GetComponentInChildren<TextMeshProUGUI>());

            UpdateStyleButtons();
        }

        private void OnValueChanged() {
            onValueChanged.Invoke(_text, _font, _style);
        }

        private void UpdateStyleButtons() {
            _ignoreUpdates = true;

            if (_font.Bold) {
                _boldButton.interactable = false;
                _boldButton.isOn = true;
            }
            else if (_font.BoldMask) {
                _boldButton.interactable = false;
                _boldButton.isOn = false;
            }
            else {
                _boldButton.interactable = true;
                _boldButton.isOn = _style.Bold;
            }

            if (_font.Italic) {
                _italicButton.interactable = false;
                _italicButton.isOn = true;
            }
            else if (_font.ItalicMask) {
                _italicButton.interactable = false;
                _italicButton.isOn = false;
            }
            else {
                _italicButton.interactable = true;
                _italicButton.isOn = _style.Italic;
            }

            if (_font.Underline) {
                _underlineButton.interactable = false;
                _underlineButton.isOn = true;
            }
            else if (_font.UnderlineMask) {
                _underlineButton.interactable = false;
                _underlineButton.isOn = false;
            }
            else {
                _underlineButton.interactable = true;
                _underlineButton.isOn = _style.Underline;
            }

            if (_font.SmallCaps) {
                _smallCapsButton.interactable = false;
                _smallCapsButton.isOn = true;
            }
            else if (_font.SmallCapsMask) {
                _smallCapsButton.interactable = false;
                _smallCapsButton.isOn = false;
            }
            else {
                _smallCapsButton.interactable = true;
                _smallCapsButton.isOn = _style.SmallCaps;
            }

            _verticalButton.isOn = _style.Vertical;

            _ignoreUpdates = false;
        }
    }
}