using ConformalDecals.Util;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ConformalDecals.UI {
    public class ColorChannelSlider : MonoBehaviour {
        [SerializeField] private ColorPickerController.ChannelUpdateEvent _onChannelChanged = new ColorPickerController.ChannelUpdateEvent();

        [SerializeField] private float _value;
        [SerializeField] private int   _channel;
        [SerializeField] private bool  _hsl;

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
                OnChannelUpdate();
            }
        }

        public void OnTextBoxUpdate(string text) {
            if (_ignoreUpdates) return;

            if (byte.TryParse(text, out byte byteValue)) {
                _value = (float) byteValue / 255;
                UpdateSlider();
                OnChannelUpdate();
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
            OnChannelUpdate();
        }

        public void OnChannelUpdate() {
            _onChannelChanged.Invoke(_value, _channel, _hsl);
        }

        public void OnColorUpdate(Color rgb, ColorHSL hsl) {
            _image.material.SetColor(PropertyIDs._Color, rgb);
            Value = _hsl ? hsl[_channel] : rgb[_channel];
        }

        public void UpdateSlider() {
            _ignoreUpdates = true;
            _slider.value = _value;
            _ignoreUpdates = false;
        }

        public void UpdateTextbox() {
            if (_textBox == null) return;

            _ignoreUpdates = true;
            ((TMP_InputField) _textBox).text = ((byte) (255 * _value)).ToString();
            _ignoreUpdates = false;
        }
    }
}