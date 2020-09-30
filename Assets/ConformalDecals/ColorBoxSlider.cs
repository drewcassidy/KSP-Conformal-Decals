using UnityEngine;
using UnityEngine.UI;

namespace ConformalDecals.UI {
    public class ColorBoxSlider : MonoBehaviour{
        [SerializeField] private Vector2 _value;

        [SerializeField] private BoxSlider _slider;
        [SerializeField] private Image     _image;

        public void OnSliderUpdate(Vector2 value) { }
    }
}