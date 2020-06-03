using ConformalDecals.MaterialModifiers;
using ConformalDecals.Util;
using UnityEngine;

namespace ConformalDecals {
    public class ModuleConformalDecalGeneric : ModuleConformalDecalBase {
        public override void OnLoad(ConfigNode node) {
            base.OnLoad(node);

            // add texture nodes
            foreach (var textureNode in node.GetNodes("TEXTURE")) {
                materialProperties.AddProperty(new MaterialTextureProperty(textureNode));
            }

            // add float nodes
            foreach (var floatNode in node.GetNodes("FLOAT")) {
                materialProperties.AddProperty(new MaterialFloatProperty(floatNode));
            }

            // add color nodes
            foreach (var colorNode in node.GetNodes("COLOR")) {
                materialProperties.AddProperty(new MaterialColorProperty(colorNode));
            }
        }
    }
}