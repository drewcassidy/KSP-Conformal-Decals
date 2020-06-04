using ConformalDecals.MaterialModifiers;
using ConformalDecals.Util;
using UnityEngine;

namespace ConformalDecals {
    public class ModuleConformalDecalGeneric : ModuleConformalDecalBase {
        public override void OnLoad(ConfigNode node) {
            base.OnLoad(node);

            // set shader
            materialProperties.SetShader(shader);
            // add texture nodes
            foreach (var textureNode in node.GetNodes("TEXTURE")) {
                var textureProperty = ScriptableObject.CreateInstance<MaterialTextureProperty>();
                textureProperty.ParseNode(textureNode);
                materialProperties.AddProperty(textureProperty);
            }

            // add float nodes
            foreach (var floatNode in node.GetNodes("FLOAT")) {
                var floatProperty = ScriptableObject.CreateInstance<MaterialFloatProperty>();
                floatProperty.ParseNode(floatNode);
                materialProperties.AddProperty(floatProperty);
            }

            // add color nodes
            foreach (var colorNode in node.GetNodes("COLOR")) {
                var colorProperty = ScriptableObject.CreateInstance<MaterialColorProperty>();
                colorProperty.ParseNode(colorNode);
                materialProperties.AddProperty(colorProperty);
            }

            if (HighLogic.LoadedSceneIsGame) {
                UpdateMaterials();
                UpdateScale();
                UpdateProjection();
            }
        }

        public override void OnIconCreate() {
            this.Log("called OnIconCreate");
            OnStart(StartState.None);
            UpdateScale();
        }
    }
}