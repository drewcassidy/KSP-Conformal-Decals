using System;
using ConformalDecals.MaterialModifiers;
using ConformalDecals.Util;
using UnityEngine;

namespace ConformalDecals {
    public class ModuleConformalDecalFlag : ModuleConformalDecalBase {
        [KSPField] public MaterialTextureProperty flagTextureProperty;

        public override void OnLoad(ConfigNode node) {

            if (materialProperties == null) {
                // materialProperties is null, so make a new one
                materialProperties = ScriptableObject.CreateInstance<MaterialPropertyCollection>();
                materialProperties.Initialize();
            }
            else {
                // materialProperties already exists, so make a copy
                materialProperties = ScriptableObject.Instantiate(materialProperties);
            }

            // set shader
            materialProperties.SetShader(decalShader);

            base.OnLoad(node);
        }

        public override void OnStart(StartState state) {
            base.OnStart(state);

            UpdateFlag(EditorLogic.FlagURL != string.Empty ? EditorLogic.FlagURL : HighLogic.CurrentGame.flagURL);
            GameEvents.onMissionFlagSelect.Add(UpdateFlag);
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
                flagTextureProperty = new MaterialTextureProperty("_Decal", flagTexture, isMain: true);
                materialProperties.AddProperty(flagTextureProperty);
            }
            else {
                flagTextureProperty.texture = flagTexture;
            }

            materialProperties.UpdateMaterials();
        }
    }
}