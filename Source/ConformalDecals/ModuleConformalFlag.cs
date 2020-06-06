using ConformalDecals.Util;

namespace ConformalDecals {
    public class ModuleConformalFlag : ModuleConformalDecal {
        private const string DefaultFlag = "Squad/Flags/default";

        public override void OnLoad(ConfigNode node) {
            base.OnLoad(node);

            UpdateFlag(GetDefaultFlag());
        }

        public override void OnStart(StartState state) {
            base.OnStart(state);

            if (HighLogic.LoadedSceneIsGame) {
                GameEvents.onMissionFlagSelect.Add(UpdateFlag);
            }

            UpdateFlag(GetDefaultFlag());
        }

        public override void OnDestroy() {
            GameEvents.onMissionFlagSelect.Remove(UpdateFlag);
            base.OnDestroy();
        }

        private string GetDefaultFlag() {
            if (HighLogic.LoadedSceneIsGame) {
                return EditorLogic.FlagURL != string.Empty ? EditorLogic.FlagURL : HighLogic.CurrentGame.flagURL;
            }
            else {
                return DefaultFlag;
            }
        }

        private void UpdateFlag(string flagUrl) {
            this.Log($"Loading flag texture '{flagUrl}'.");
            var flagTexture = GameDatabase.Instance.GetTexture(flagUrl, false);
            if (flagTexture == null) {
                this.LogWarning($"Unable to find flag texture '{flagUrl}'.");
                return;
            }

            materialProperties.AddOrGetTextureProperty("_Decal", true).texture = flagTexture;

            UpdateMaterials();
        }
    }
}