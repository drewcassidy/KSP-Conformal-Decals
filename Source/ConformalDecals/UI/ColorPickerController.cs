using System;
using ConformalDecals.Util;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ConformalDecals.UI {
    public class ColorPickerController : MonoBehaviour {
        [Serializable]
        public class ColorUpdateEvent : UnityEvent<Color, Util.ColorHSV> { }

        [Serializable]
        public class ChannelUpdateEvent : UnityEvent<float, int, bool> { }

        [Serializable]
        public class SVUpdateEvent : UnityEvent<Vector2> { }

        [SerializeField] public ColorUpdateEvent onColorChanged = new ColorUpdateEvent();

        [SerializeField] private Color      _value;
        [SerializeField] private Image      _previewImage;
        [SerializeField] private Selectable _hexTextBox;

        private bool _ignoreUpdate;

        public Color RGB {
            get => _value;
            set {
                _value = value;
                OnColorUpdate();
            }
        }

        public Util.ColorHSV HSV {
            get => Util.ColorHSV.RGB2HSV(_value);
            set {
                _value = Util.ColorHSV.HSV2RGB(value);
                OnColorUpdate();
            }
        }


        public static ColorPickerController Create(Color rgb, UnityAction<Color, Util.ColorHSV> colorUpdateCallback) {
            var menu = Instantiate(UILoader.ColorPickerPrefab, MainCanvasUtil.MainCanvas.transform, true);
            menu.AddComponent<DragPanel>();
            MenuNavigation.SpawnMenuNavigation(menu, Navigation.Mode.Automatic, true);

            var controller = menu.GetComponent<ColorPickerController>();
            controller.RGB = rgb;
            controller.onColorChanged.AddListener(colorUpdateCallback);
            return controller;
        }

        public void OnClose() {
            Destroy(gameObject);
        }

        public void OnChannelUpdate(float value, int channel, bool hsv) {
            if (hsv) {
                var newHSV = HSV;
                newHSV[channel] = value;
                HSV = newHSV;
            }
            else {
                var newRGB = RGB;
                newRGB[channel] = value;
                RGB = newRGB;
            }
        }

        public void OnSVUpdate(Vector2 sv) {
            var newHSV = HSV;
            newHSV.s = sv.x;
            newHSV.v = sv.y;
            HSV = newHSV;
        }

        public void OnColorUpdate() {
            onColorChanged.Invoke(RGB, HSV);
            _previewImage.material.SetColor(PropertyIDs._Color, RGB);
            UpdateHexColor();
        }

        public void OnHexColorUpdate(string text) {
            if (_ignoreUpdate) return;

            if (ParseUtil.TryParseHexColor(text, out var newRGB)) {
                RGB = newRGB;
                OnColorUpdate();
            }
            else {
                UpdateHexColor();
            }
        }

        private void Awake() {
            foreach (var slider in gameObject.GetComponentsInChildren<ColorChannelSlider>()) {
                slider.onChannelChanged.AddListener(OnChannelUpdate);
                onColorChanged.AddListener(slider.OnColorUpdate);
            }

            foreach (var box in gameObject.GetComponentsInChildren<ColorBoxSlider>()) {
                box.onValueChanged.AddListener(OnSVUpdate);
                onColorChanged.AddListener(box.OnColorUpdate);
            }
        }

        private void UpdateHexColor() {
            _ignoreUpdate = true;
            var byteColor = (Color32) RGB;
            var hexColor = $"{byteColor.r:x2}{byteColor.g:x2}{byteColor.b:x2}";
            ((TMP_InputField) _hexTextBox).text = hexColor;
            _ignoreUpdate = false;
        }
    }
}