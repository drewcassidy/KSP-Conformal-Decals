using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ConformalDecals.UI {
    public class ColorPickerController : MonoBehaviour {
        [SerializeField] private Color      _value;
        [SerializeField] private Image      _previewImage;
        [SerializeField] private Selectable _hexTextBox;

        public void OnClose() { }

        public void OnHexColorUpdate(string text) { }
    }
}