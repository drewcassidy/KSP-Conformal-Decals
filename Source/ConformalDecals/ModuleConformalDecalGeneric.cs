using ConformalDecals.MaterialModifiers;
using ConformalDecals.Util;
using UnityEngine;

namespace ConformalDecals {
    public class ModuleConformalDecalGeneric : ModuleConformalDecalBase {
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

            base.OnLoad(node);
        }
    }
}