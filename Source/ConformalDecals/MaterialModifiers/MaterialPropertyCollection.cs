using System;
using System.Collections.Generic;
using UnityEngine;

namespace ConformalDecals.MaterialModifiers {
    public class MaterialPropertyCollection {
        public Shader ShaderRef { get; }
        public TextureMaterialProperty MainTextureMaterial { get; }

        private List<MaterialProperty>                _materialModifiers;
        private List<TextureMaterialProperty> _texturePropertyMaterialModifiers;

        public MaterialPropertyCollection(ConfigNode node) {
            var shaderString = node.GetValue("shader");

            if (shaderString == null)
                throw new FormatException("Missing shader name in material");

            if (shaderString == string.Empty)
                throw new FormatException("Empty shader name in material");

            _materialModifiers = new List<MaterialProperty>();
            _texturePropertyMaterialModifiers = new List<TextureMaterialProperty>();

            //TODO: USE SHABBY PROVIDED METHOD HERE INSTEAD
            ShaderRef = Shader.Find(shaderString);

            if (ShaderRef == null) throw new FormatException($"Shader not found: {shaderString}");

            foreach (ConfigNode propertyNode in node.nodes) {
                try {
                    MaterialProperty property;
                    switch (propertyNode.name) {
                        case "FLOAT":
                            property = new FloatMaterialProperty(propertyNode);
                            break;

                        case "COLOR":
                            property = new ColorMaterialProperty(propertyNode);
                            break;

                        case "TEXTURE":
                            property = new TextureMaterialProperty(propertyNode);
                            var textureModifier = (TextureMaterialProperty) property;
                            if (textureModifier.IsMain) {
                                if (MainTextureMaterial == null) {
                                    MainTextureMaterial = textureModifier;
                                }
                                else {
                                    Debug.LogWarning(
                                        $"Material texture property {textureModifier.TextureUrl} is marked as main, but material already has a main texture! \n" +
                                        $"Defaulting to {MainTextureMaterial.TextureUrl}");
                                }
                            }

                            _texturePropertyMaterialModifiers.Add(textureModifier);
                            break;

                        default:
                            throw new FormatException($"Invalid property type '{propertyNode.name}' in material");
                    }

                    _materialModifiers.Add(property);
                }

                catch (Exception e) {
                    Debug.LogError(e.Message);
                }
            }
        }
    }
}