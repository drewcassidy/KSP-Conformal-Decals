using System;
using System.Collections.Generic;
using ConformalDecals.Text;
using UniLinq;
using UnityEngine;
using UnityEngine.UI;

namespace ConformalDecals.UI {
    public class FontMenuController : MonoBehaviour {
        [SerializeField] private GameObject _menuItem;
        [SerializeField] private GameObject _menuList;

        public DecalFont currentFont;

        public delegate void FontUpdateReceiver(DecalFont font);

        public FontUpdateReceiver fontUpdateCallback;

        public static FontMenuController Create(IEnumerable<DecalFont> fonts, DecalFont currentFont, FontUpdateReceiver fontUpdateCallback) {
            var menu = Instantiate(UILoader.FontMenuPrefab, MainCanvasUtil.MainCanvas.transform, true);
            menu.AddComponent<DragPanel>();
            MenuNavigation.SpawnMenuNavigation(menu, Navigation.Mode.Automatic, true);

            var controller = menu.GetComponent<FontMenuController>();
            controller.fontUpdateCallback = fontUpdateCallback;
            controller.currentFont = currentFont;
            controller.Populate(fonts);
            return controller;
        }

        public void OnClose() {
            Destroy(gameObject);
        }

        public void OnFontSelected(DecalFont font) {
            currentFont = font ?? throw new ArgumentNullException(nameof(font));
            fontUpdateCallback(currentFont);
        }

        public void Populate(IEnumerable<DecalFont> fonts) {
            if (fonts == null) throw new ArgumentNullException(nameof(fonts));

            Toggle active = null;

            foreach (var font in fonts.OrderBy(x => x.title)) {
                Debug.Log(font.title);
                var listItem = GameObject.Instantiate(_menuItem, _menuList.transform);
                listItem.name = font.title;
                listItem.SetActive(true);

                var fontItem = listItem.AddComponent<FontMenuItem>();
                fontItem.Font = font;
                fontItem.fontSelectionCallback = OnFontSelected;

                if (font == currentFont) active = fontItem.toggle;
            }

            if (active != null) active.isOn = true;
        }
    }
}