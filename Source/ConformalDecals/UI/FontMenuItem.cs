using System;
using ConformalDecals.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ConformalDecals.UI {
    public class FontMenuItem : MonoBehaviour {
        public DecalFont Font {
            get => _font;
            set {
                _font = value;
                _font.SetupSample(_label);
            }
        }

        public delegate void FontSelectionReceiver(DecalFont font);

        public FontSelectionReceiver fontSelectionCallback;
        public Toggle                toggle;

        private DecalFont _font;
        private TMP_Text  _label;

        private void Awake() {
            _label = gameObject.GetComponentInChildren<TextMeshProUGUI>();
            toggle = gameObject.GetComponent<Toggle>();
            toggle.isOn = false;
            toggle.onValueChanged.AddListener(delegate { OnToggle(toggle); });
        }

        public void OnToggle(Toggle change) {
            if (change.isOn) fontSelectionCallback?.Invoke(_font);
        }
    }
}