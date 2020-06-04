using ConformalDecals.MaterialModifiers;
using ConformalDecals.Util;
using UnityEngine;

namespace ConformalDecals {
    public class ModuleConformalDecalFlag : ModuleConformalDecalBase {
        [KSPField] public MaterialTextureProperty flagTextureProperty;

        private const string defaultFlag = "Squad/Flags/default";

        public override void OnLoad(ConfigNode node) {
            base.OnLoad(node);


            if (HighLogic.LoadedSceneIsGame) {
                UpdateMaterials();
                UpdateScale();
                UpdateProjection();
            }
        }

        public override void OnStart(StartState state) {
            base.OnStart(state);

            if (HighLogic.LoadedSceneIsGame) {
                UpdateFlag(EditorLogic.FlagURL != string.Empty ? EditorLogic.FlagURL : HighLogic.CurrentGame.flagURL);
                GameEvents.onMissionFlagSelect.Add(UpdateFlag);
            }
            else {
                UpdateFlag(defaultFlag);
            }
        }

        public override void OnIconCreate() {
            this.Log("called OnIconCreate");
            OnStart(StartState.None);
            UpdateScale();
        }

        private void UpdateFlag(string flagUrl) {
            this.Log($"Loading flag texture '{flagUrl}'.");
            var flagTexture = GameDatabase.Instance.GetTexture(flagUrl, false);
            if (flagTexture == null) {
                this.LogWarning($"Unable to find flag texture '{flagUrl}'.");
                return;
            }

            if (flagTextureProperty == null) {
                this.Log("Initializing flag property");
                flagTextureProperty = ScriptableObject.CreateInstance<MaterialTextureProperty>();
                flagTextureProperty.Name = "_Decal";
                flagTextureProperty.isMain = true;
                materialProperties.AddProperty(flagTextureProperty);
            }
            else { }

            flagTextureProperty.texture = flagTexture;


            UpdateMaterials();
        }
    }
}