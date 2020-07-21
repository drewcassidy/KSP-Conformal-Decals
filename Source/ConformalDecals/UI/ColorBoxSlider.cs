using ConformalDecals.Util;
using UnityEngine;
using UnityEngine.UI;

namespace ConformalDecals.UI {
    public class ColorBoxSlider : MonoBehaviour {
        [SerializeField] private ColorPickerController.ChannelUpdateEvent _onXChannelChanged = new ColorPickerController.ChannelUpdateEvent();
        [SerializeField] private ColorPickerController.ChannelUpdateEvent _onYChannelChanged = new ColorPickerController.ChannelUpdateEvent();

        [SerializeField] private Vector2    _value;
        [SerializeField] private Vector2Int _channel;
        [SerializeField] private bool       _hsl;

        [SerializeField] private BoxSlider _slider;
        [SerializeField] private Image     _image;

        private Material _imageMaterial;

        public Vector2 Value {
            get => _value;
            set {
                _value.x = Mathf.Clamp01(value.x);
                _value.y = Mathf.Clamp01(value.y);
                UpdateSlider();
                OnChannelUpdate();
            }
        }

        public void Awake() {
            _imageMaterial = _image.material;
        }

        public void OnSliderUpdate(Vector2 value) {
            _value = value;
            OnChannelUpdate();
        }

        public void OnChannelUpdate() {
            _onXChannelChanged.Invoke(_value.x, _channel.x, _hsl);
            _onYChannelChanged.Invoke(_value.y, _channel.y, _hsl);
        }

        public void OnColorUpdate(Color rgb, ColorHSL hsl) {
            Vector2 newValue;
            _imageMaterial.SetColor(PropertyIDs._Color, rgb);
            newValue.x = _hsl ? hsl[_channel.x] : rgb[_channel.x];
            newValue.y = _hsl ? hsl[_channel.y] : rgb[_channel.y];
            Value = newValue;
        }

        public void UpdateSlider() {
            _slider.Value = _value;
        }
    }
}