using System;
using ConformalDecals.Util;
using TMPro;
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

        private bool _ignoreUpdate;

        public Color RGB {
            get => _value;
            set {
                _value = value;
                OnColorUpdate();
            }
        }

        public ColorHSL HSL {
            get => ColorHSL.RGB2HSL(_value);
            set {
                _value = ColorHSL.HSL2RGB(value);
                OnColorUpdate();
            }
        }


        public static ColorPickerController Create(Color rgb, UnityAction<Color, ColorHSL> colorUpdateCallback) {
            var menu = Instantiate(UILoader.ColorPickerPrefab, MainCanvasUtil.MainCanvas.transform, true);
            menu.AddComponent<DragPanel>();
            MenuNavigation.SpawnMenuNavigation(menu, Navigation.Mode.Automatic, true);

            var controller = menu.GetComponent<ColorPickerController>();
            controller.RGB = rgb;
            controller._onColorChanged.AddListener(colorUpdateCallback);
            return controller;
        }

        public void OnClose() {
            Destroy(gameObject);
        }

        public void OnColorUpdate() {
            _onColorChanged.Invoke(RGB, HSL);
            _previewImage.material.SetColor(PropertyIDs._Color, RGB);
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

        public void UpdateHexColor() {
            _ignoreUpdate = true;
            ((TMP_InputField) _hexTextBox).text = $"{RGB.r:x2}{RGB.g:x2}{RGB.b:x2}";
            _ignoreUpdate = false;
        }

        public void OnChannelUpdate(float value, int channel, bool hsl) {
            if (hsl) {
                var newHSL = HSL;
                newHSL[channel] = value;
                HSL = newHSL;
            }
            else {
                var newRGB = RGB;
                newRGB[channel] = value;
                RGB = newRGB;
            }
        }
    }
}