using System;
using ConformalDecals.MaterialModifiers;
using ConformalDecals.Util;
using UnityEngine;

namespace ConformalDecals {
    public class ModuleConformalDecalFlag : ModuleConformalDecalBase {
        
        [KSPField]
        private MaterialTextureProperty _flagTextureProperty;

        public override void OnLoad(ConfigNode node) {
            base.OnLoad(node);

            _flagTextureProperty = new MaterialTextureProperty("_MainTex", Texture2D.whiteTexture);
            materialProperties.AddProperty(_flagTextureProperty);
        }

        public override void OnStart(StartState state) {
            base.OnStart(state);

            UpdateFlag(EditorLogic.FlagURL != string.Empty ? EditorLogic.FlagURL : HighLogic.CurrentGame.flagURL);
            GameEvents.onMissionFlagSelect.Add(UpdateFlag);
        }

        private void UpdateFlag(string flagUrl) {
            _flagTextureProperty.texture = GameDatabase.Instance.GetTexture(flagUrl, false);

            materialProperties.UpdateMaterials();
        }
    }
}