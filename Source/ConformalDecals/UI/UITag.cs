using UnityEngine;

namespace ConformalDecals.UI {
    public class UITag : MonoBehaviour {
        public enum UIType {
            None,
            Window,
            Box,
            Button,
            ButtonToggle,
            RadioToggle,
            Slider,
            Dropdown,
            Label,
            Header,
            BoxSlider
        }

        [SerializeField] public UIType type = UIType.None;
    }
}