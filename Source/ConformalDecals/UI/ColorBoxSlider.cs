using UnityEngine;
using UnityEngine.UI;

namespace ConformalDecals.UI {
    public class ColorBoxSlider : MonoBehaviour {
        [SerializeField] public ColorPickerController.SVUpdateEvent onValueChanged = new ColorPickerController.SVUpdateEvent();

        [SerializeField] private Vector2    _value;

        [SerializeField] private BoxSlider _slider;
        [SerializeField] private Image     _image;
        
        private bool _ignoreUpdates;

        public Vector2 Value {
            get => _value;
            set {
                _value.x = Mathf.Clamp01(value.x);
                _value.y = Mathf.Clamp01(value.y);
                UpdateSlider();
                UpdateChannels();
            }
        }

        public void OnColorUpdate(Color rgb, Util.ColorHSV hsv) {
            if (_ignoreUpdates) return;

            _image.material.SetColor(PropertyIDs._Color, (Vector4) hsv);

            _value.x = hsv.s;
            _value.y = hsv.v;
            UpdateSlider();
        }
        
        public void OnSliderUpdate(Vector2 value) {
            if (_ignoreUpdates) return;
            
            _value = value;
            UpdateChannels();
        }

        private void Awake() {
            var boxSlider = gameObject.GetComponentInChildren<BoxSlider>();
            boxSlider.OnValueChanged.AddListener(OnSliderUpdate);
        }

        private void UpdateChannels() {
            _ignoreUpdates = true;
            
            onValueChanged.Invoke(_value);
            
            _ignoreUpdates = false;
        }
        
        private void UpdateSlider() {
            _ignoreUpdates = true;
            
            _slider.Value = _value;
            
            _ignoreUpdates = false;
        }
    }
}