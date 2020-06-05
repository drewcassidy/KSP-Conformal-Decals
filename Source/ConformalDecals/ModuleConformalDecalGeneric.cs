using ConformalDecals.MaterialModifiers;
using ConformalDecals.Util;
using UnityEngine;

namespace ConformalDecals {
    public class ModuleConformalDecalGeneric : ModuleConformalDecalBase {
        public override void OnLoad(ConfigNode node) {

            // set shader
            materialProperties.SetShader(shader);
            // add texture nodes
            foreach (var textureNode in node.GetNodes("TEXTURE")) {
                materialProperties.ParseProperty<MaterialTextureProperty>(textureNode);
            }

            // add float nodes
            foreach (var floatNode in node.GetNodes("FLOAT")) {
                materialProperties.ParseProperty<MaterialTextureProperty>(floatNode);
            }

            // add color nodes
            foreach (var colorNode in node.GetNodes("COLOR")) {
                materialProperties.ParseProperty<MaterialColorProperty>(colorNode);
            }

            base.OnLoad(node);
        }

        public override void OnIconCreate() {
            this.Log("called OnIconCreate");
            UpdateScale();
        }
    }
}