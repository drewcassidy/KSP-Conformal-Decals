using ConformalDecals.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ConformalDecals.UI {
    public class TextEntryController : MonoBehaviour {
        public delegate void TextUpdateReceiver(DecalText text);

        public TextUpdateReceiver textUpdateCallback;

        public  DecalText          decalText;
        private FontMenuController _fontMenu;

        [SerializeField] private Selectable _textBox;
        [SerializeField] private Button     _fontButton;
        [SerializeField] private Slider     _outlineWidthSlider;

        [SerializeField] private Toggle _boldButton;
        [SerializeField] private Toggle _italicButton;
        [SerializeField] private Toggle _underlineButton;
        [SerializeField] private Toggle _smallCapsButton;
        [SerializeField] private Toggle _verticalButton;

        public static TextEntryController Create(DecalText text, TextUpdateReceiver textUpdateCallback) {
            var window = Instantiate(UILoader.TextEntryPrefab, MainCanvasUtil.MainCanvas.transform, true);
            window.AddComponent<DragPanel>();
            MenuNavigation.SpawnMenuNavigation(window, Navigation.Mode.Automatic, true);

            var controller = window.GetComponent<TextEntryController>();
            controller.decalText = text;
            controller.textUpdateCallback = textUpdateCallback;
            text.font.SetupSample(controller._fontButton.GetComponentInChildren<TextMeshProUGUI>());

            return controller;
        }

        private void Start() {
            ((TMP_InputField) _textBox).text = decalText.text;

            _outlineWidthSlider.value = decalText.outlineWidth;
            _boldButton.isOn = (decalText.style & FontStyles.Bold) != 0;
            _italicButton.isOn = (decalText.style & FontStyles.Italic) != 0;
            _underlineButton.isOn = (decalText.style & FontStyles.Underline) != 0;
            _smallCapsButton.isOn = (decalText.style & FontStyles.SmallCaps) != 0;
            _verticalButton.isOn = decalText.vertical;

        }

        public void OnClose() {
            if (_fontMenu != null) _fontMenu.OnClose();
            Destroy(gameObject);
        }

        public void OnAnyUpdate() {
            textUpdateCallback(decalText);
        }

        public void OnTextUpdate(string newText) {
            this.decalText.text = newText;

            OnAnyUpdate();
        }

        public void OnFontMenu() {
            if (_fontMenu == null) _fontMenu = FontMenuController.Create(DecalConfig.Fonts, decalText.font, OnFontUpdate);
        }

        public void OnFontUpdate(DecalFont font) {
            decalText.font = font;
            font.SetupSample(_fontButton.GetComponentInChildren<TextMeshProUGUI>());

            var textBox = ((TMP_InputField) _textBox);
            textBox.textComponent.fontStyle = decalText.style | decalText.font.fontStyle;
            textBox.fontAsset = decalText.font.fontAsset;

            OnAnyUpdate();
        }

        public void OnColorMenu() { }

        public void OnColorUpdate(Color color) {
            decalText.color = color;
            OnAnyUpdate();
        }

        public void OnOutlineColorMenu() { }

        public void OnOutlineColorUpdate(Color color) {
            decalText.outlineColor = color;
            OnAnyUpdate();
        }

        public void OnOutlineUpdate(float value) {
            decalText.outlineWidth = value;
            OnAnyUpdate();
        }

        public void OnBoldUpdate(bool state) {
            if (state) decalText.style |= FontStyles.Bold;
            else decalText.style &= ~FontStyles.Bold;

            ((TMP_InputField) _textBox).textComponent.fontStyle = decalText.style | decalText.font.fontStyle;

            OnAnyUpdate();

        }

        public void OnItalicUpdate(bool state) {
            if (state) decalText.style |= FontStyles.Italic;
            else decalText.style &= ~FontStyles.Italic;

            ((TMP_InputField) _textBox).textComponent.fontStyle = decalText.style | decalText.font.fontStyle;

            OnAnyUpdate();

        }

        public void OnUnderlineUpdate(bool state) {
            if (state) decalText.style |= FontStyles.Underline;
            else decalText.style &= ~FontStyles.Underline;

            ((TMP_InputField) _textBox).textComponent.fontStyle = decalText.style | decalText.font.fontStyle;

            OnAnyUpdate();

        }

        public void OnSmallCapsUpdate(bool state) {
            if (state) decalText.style |= FontStyles.SmallCaps;
            else decalText.style &= ~FontStyles.SmallCaps;

            ((TMP_InputField) _textBox).textComponent.fontStyle = decalText.style | decalText.font.fontStyle;

            OnAnyUpdate();

        }

        public void OnVerticalUpdate(bool state) {
            decalText.vertical = state;
            OnAnyUpdate();
        }
    }
}