using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ConformalDecals.UI {
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class UILoader : MonoBehaviour {
        private static readonly string Path = KSPUtil.ApplicationRootPath + "GameData/ConformalDecals/Resources/";

        private static GameObject _textEntryPrefab;
        private static GameObject _fontMenuPrefab;

        public static GameObject FontMenuPrefab => _fontMenuPrefab;
        public static GameObject TextEntryPrefab => _textEntryPrefab;

        private void Awake() {
            var prefabs = AssetBundle.LoadFromFile(Path + "ui.conformaldecals");

            _textEntryPrefab = prefabs.LoadAsset("TextEntryPanel") as GameObject;
            _fontMenuPrefab = prefabs.LoadAsset("FontMenuPanel") as GameObject;

            ProcessWindow(_textEntryPrefab);
            ProcessWindow(_fontMenuPrefab);
        }

        private static void ProcessWindow(GameObject window) {
            var skin = UISkinManager.defaultSkin;
            var font = UISkinManager.TMPFont;

            var background = window.GetComponent<Image>();
            background.sprite = skin.window.normal.background;
            background.type = Image.Type.Sliced;

            var texts = window.GetComponentsInChildren<TextMeshProUGUI>(true);

            foreach (var text in texts) {
                ProcessText(text, font, Color.white);
            }

            var tags = window.GetComponentsInChildren<UITag>(true);

            foreach (var tag in tags) {
                Debug.Log($"Handling object {tag.gameObject.name}");
                switch (tag.type) {
                    case UITag.UIType.Window:
                        ProcessImage(tag.gameObject, skin.window);
                        break;
                    case UITag.UIType.Button:
                        ProcessSelectable(tag.gameObject, skin.button);
                        break;
                    case UITag.UIType.ButtonToggle:
                        ProcessToggleButton(tag.gameObject, skin.button);
                        break;
                    case UITag.UIType.RadioToggle:
                        ProcessSelectable(tag.gameObject, skin.toggle);
                        break;
                    case UITag.UIType.Slider:
                        ProcessSlider(tag.gameObject, skin.horizontalSlider, skin.horizontalSliderThumb, skin.verticalSlider, skin.verticalSliderThumb);
                        break;
                    case UITag.UIType.Box:
                        ProcessSelectable(tag.gameObject, skin.box);
                        break;
                    case UITag.UIType.Dropdown:
                        ProcessDropdown(tag.gameObject, skin.button, skin.window);
                        break;
                    case UITag.UIType.Label:
                        ProcessText(tag.GetComponent<TextMeshProUGUI>(), font, new Color(0.718f, 0.996f, 0.000f, 1.000f), 14);
                        break;
                    case UITag.UIType.Header:
                        ProcessText(tag.GetComponent<TextMeshProUGUI>(), font, new Color(0.718f, 0.996f, 0.000f, 1.000f), 16);
                        break;
                }
            }
        }

        private static void ProcessImage(GameObject gameObject, UIStyle style) {
            ProcessImage(gameObject.GetComponent<Image>(), style.normal);
        }

        private static void ProcessImage(Image image, UIStyleState state) {
            image.sprite = state.background;
            image.color = Color.white;
            image.type = Image.Type.Sliced;
        }

        private static void ProcessSelectable(GameObject gameObject, UIStyle style) {
            var selectable = gameObject.GetComponent<Selectable>();
            if (selectable == null) throw new FormatException("No Selectable component present");

            ProcessImage(selectable.image, style.normal);

            selectable.transition = Selectable.Transition.SpriteSwap;

            var state = selectable.spriteState;
            state.highlightedSprite = style.highlight.background;
            state.pressedSprite = style.active.background;
            state.disabledSprite = style.disabled.background;
            selectable.spriteState = state;
        }

        private static void ProcessToggleButton(GameObject gameObject, UIStyle style) {
            ProcessSelectable(gameObject, style);

            var toggle = gameObject.GetComponent<Toggle>();
            ProcessImage(toggle.graphic as Image, style.active);
        }

        private static void ProcessSlider(GameObject gameObject, UIStyle horizontalStyle, UIStyle horizontalThumbStyle, UIStyle verticalStyle, UIStyle verticalThumbStyle) {
            var slider = gameObject.GetComponent<Slider>();
            if (slider == null) throw new FormatException("No Slider component present");

            UIStyle sliderStyle;
            UIStyle thumbStyle;
            if (slider.direction == Slider.Direction.BottomToTop || slider.direction == Slider.Direction.TopToBottom) {
                sliderStyle = verticalStyle;
                thumbStyle = verticalThumbStyle;
            }
            else {
                sliderStyle = horizontalStyle;
                thumbStyle = horizontalThumbStyle;
            }

            ProcessSelectable(gameObject, thumbStyle);

            var back = gameObject.transform.Find("Background").GetComponent<Image>();
            if (back != null) {
                back.sprite = sliderStyle.normal.background;
                back.type = Image.Type.Sliced;
            }
        }

        private static void ProcessDropdown(GameObject gameObject, UIStyle buttonStyle, UIStyle windowStyle) {
            ProcessSelectable(gameObject, buttonStyle);

            var template = gameObject.transform.Find("Template").gameObject;
            ProcessImage(template, windowStyle);
        }

        private static void ProcessText(TextMeshProUGUI text, TMP_FontAsset font, Color color, int size = -1) {
            text.font = font;
            text.color = color;
            if (size > 0) text.fontSize = size;
        }
    }
}