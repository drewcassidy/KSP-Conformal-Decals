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

        public void OnSliderUpdate(Vector2 value) { }

        public void OnColorUpdate(Color rgb, ColorHSL hsl) { }
    }
}