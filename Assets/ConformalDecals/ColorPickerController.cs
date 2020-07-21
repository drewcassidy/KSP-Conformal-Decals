using System;
using ConformalDecals.Util;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ConformalDecals.UI {
    public class ColorPickerController : MonoBehaviour {
        [Serializable]
        public class ColorUpdateEvent : UnityEvent<Color, ColorHSL> { }

        [Serializable]
        public class ChannelUpdateEvent : UnityEvent<float, int, bool> { }

        [SerializeField] private ColorUpdateEvent _onColorChanged = new ColorUpdateEvent();

        [SerializeField] private Color      _value;
        [SerializeField] private Image      _previewImage;
        [SerializeField] private Selectable _hexTextBox;

        public void OnClose() { }

        public void OnHexColorUpdate(string text) { }

        public void OnChannelUpdate(float value, int channel, bool hsl) { }
    }
}