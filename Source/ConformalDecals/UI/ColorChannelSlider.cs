using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ConformalDecals.UI {
    public class ColorChannelSlider : MonoBehaviour {
        [SerializeField] public ColorPickerController.ChannelUpdateEvent onChannelChanged = new ColorPickerController.ChannelUpdateEvent();

        [SerializeField] private float _value;
        [SerializeField] private int   _channel;
        [SerializeField] private bool  _hsv;

        [SerializeField] private Selectable _textBox;
        [SerializeField] private Slider     _slider;
        [SerializeField] private Image      _image;

        private bool _ignoreUpdates;

        public float Value {
            get => _value;
            set {
                _value = Mathf.Clamp01(value);
                UpdateSlider();
                UpdateTextbox();
                UpdateChannel();
            }
        }
        
        public void OnColorUpdate(Color rgb, Util.ColorHSV hsv) {
            if (_ignoreUpdates) return;

            _image.material.SetColor(PropertyIDs._Color, _hsv ? (Color) (Vector4) hsv : rgb);
            
            _value = _hsv ? hsv[_channel] : rgb[_channel];
            UpdateSlider();
            UpdateTextbox();
        }
        
        public void OnTextBoxUpdate(string text) {
            if (_ignoreUpdates) return;

            if (byte.TryParse(text, out byte byteValue)) {
                _value = (float) byteValue / 255;
                UpdateSlider();
                UpdateChannel();
            }
            else {
                // value is invalid, reset value
                UpdateTextbox();
            }
        }

        public void OnSliderUpdate(float value) {
            if (_ignoreUpdates) return;

            _value = value;
            UpdateTextbox();
            UpdateChannel();
        }

        private void UpdateChannel() {
            onChannelChanged.Invoke(_value, _channel, _hsv);
        }

        private void UpdateSlider() {
            _ignoreUpdates = true;
            _slider.value = _value;
            _ignoreUpdates = false;
        }

        private void UpdateTextbox() {
            if (_textBox == null) return;

            _ignoreUpdates = true;
            ((TMP_InputField) _textBox).text = ((byte) (255 * _value)).ToString();
            _ignoreUpdates = false;
        }
    }
}