using UnityEngine;
using UnityEngine.UI;

namespace ConformalDecals.UI {
    public class ColorChannelSlider : MonoBehaviour {
        [SerializeField] private float _value;
        [SerializeField] private int   _channel;
        [SerializeField] private bool  _hsv;

        [SerializeField] private Selectable _textBox;
        [SerializeField] private Slider     _slider;
        [SerializeField] private Image      _image;

        public void OnTextBoxUpdate(string text) { }

        public void OnSliderUpdate(float value) { }
    }
}