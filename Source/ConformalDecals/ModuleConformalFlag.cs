using ConformalDecals.Util;
using UnityEngine;

namespace ConformalDecals {
    public class ModuleConformalFlag : ModuleConformalDecal {
        private const string DefaultFlag = "Squad/Flags/default";

        [KSPField(isPersistant = true)] public string flagUrl = DefaultFlag;

        [KSPField(isPersistant = true)] public bool useCustomFlag;

        public string MissionFlagUrl {
            get {
                if (HighLogic.LoadedSceneIsEditor) {
                    return string.IsNullOrEmpty(EditorLogic.FlagURL) ? HighLogic.CurrentGame.flagURL : EditorLogic.FlagURL;
                }

                if (HighLogic.LoadedSceneIsFlight) {
                    return string.IsNullOrEmpty(part.flagURL) ? HighLogic.CurrentGame.flagURL : part.flagURL;
                }

                return DefaultFlag;
            }
        }

        public override void OnLoad(ConfigNode node) {
            base.OnLoad(node);

            if (useCustomFlag) {
                SetFlag(flagUrl);
            }
            else {
                SetFlag(MissionFlagUrl);
            }
        }

        public override void OnStart(StartState state) {
            base.OnStart(state);

            if (HighLogic.LoadedSceneIsGame) {
                GameEvents.onMissionFlagSelect.Add(OnEditorFlagSelected);
            }

            if (HighLogic.LoadedSceneIsEditor) {
                Events[nameof(ResetFlag)].guiActiveEditor = useCustomFlag;
            }

            if (useCustomFlag) {
                SetFlag(flagUrl);
            }
            else {
                SetFlag(MissionFlagUrl);
            }
        }

        public override void OnDestroy() {
            GameEvents.onMissionFlagSelect.Remove(SetFlag);
            base.OnDestroy();
        }

        [KSPEvent(guiActive = false, guiActiveEditor = true, guiName = "Select Flag")]
        public void SelectFlag() {
            var flagBrowser = (Instantiate((Object) (new FlagBrowserGUIButton(null, null, null, null)).FlagBrowserPrefab) as GameObject).GetComponent<FlagBrowser>();
            flagBrowser.OnFlagSelected = OnCustomFlagSelected;
        }

        [KSPEvent(guiActive = false, guiActiveEditor = true, guiName = "Reset Flag")]
        public void ResetFlag() {
            SetFlag(MissionFlagUrl);
            SetFlagSymmetryCounterparts(MissionFlagUrl);

            useCustomFlag = false;
            Events[nameof(ResetFlag)].guiActiveEditor = false;
        }

        private void OnCustomFlagSelected(FlagBrowser.FlagEntry newFlagEntry) {
            SetFlag(newFlagEntry.textureInfo.name);
            SetFlagSymmetryCounterparts(newFlagEntry.textureInfo.name);

            useCustomFlag = true;
            Events[nameof(ResetFlag)].guiActiveEditor = true;
        }

        private void OnEditorFlagSelected(string newFlagUrl) {
            if (useCustomFlag) {
                SetFlag(newFlagUrl);
                SetFlagSymmetryCounterparts(newFlagUrl);
            }
        }

        private void SetFlag(string newFlagUrl) {
            this.Log($"Loading flag texture '{newFlagUrl}'.");

            flagUrl = newFlagUrl;
            materialProperties.AddOrGetTextureProperty("_Decal", true).TextureUrl = newFlagUrl;

            UpdateMaterials();
        }

        private void SetFlagSymmetryCounterparts(string newFlagUrl) {
            foreach (var counterpart in part.symmetryCounterparts) {
                var decal = counterpart.GetComponent<ModuleConformalFlag>();

                decal.SetFlag(newFlagUrl);
                decal.useCustomFlag = useCustomFlag;
            }
        }
    }
}