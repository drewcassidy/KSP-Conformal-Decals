using ConformalDecals.Text;
using ConformalDecals.UI;
using ConformalDecals.Util;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ConformalDecals {
    public class ModuleConformalText: ModuleConformalDecal {
        [KSPField(isPersistant = true)] public string text = "Hello World!";

        private GameObject _textEntryGui;

        public override void OnLoad(ConfigNode node) {
            base.OnLoad(node);

        }

        public override void OnStart(StartState state) {
            base.OnStart(state);

        }

        [KSPEvent(guiActive = false, guiActiveEditor = true, guiName = "#LOC_ConformalDecals_gui-select-flag")]
        public void SetText()
        {
            if (_textEntryGui == null) {
                _textEntryGui = Instantiate(UILoader.textEntryPrefab, MainCanvasUtil.MainCanvas.transform, true);
                _textEntryGui.AddComponent<DragPanel>();
                MenuNavigation.SpawnMenuNavigation(_textEntryGui, Navigation.Mode.Automatic, true);
            }
        }
    }
}