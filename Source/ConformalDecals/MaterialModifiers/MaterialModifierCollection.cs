using System;
using System.Collections.Generic;
using UnityEngine;

namespace ConformalDecals.MaterialModifiers {
    public class MaterialModifierCollection {
        public Shader ShaderRef { get; }
        public TexturePropertyMaterialModifier MainTexture { get; }

        private List<MaterialModifier>                _materialModifiers;
        private List<TexturePropertyMaterialModifier> _texturePropertyMaterialModifiers;

        public MaterialModifierCollection(ConfigNode node) {
            var shaderString = node.GetValue("shader");

            if (shaderString == null)
                throw new FormatException($"Missing shader name in material");

            if (shaderString == string.Empty)
                throw new FormatException($"Empty shader name in material");

            _materialModifiers = new List<MaterialModifier>();
            _texturePropertyMaterialModifiers = new List<TexturePropertyMaterialModifier>();

            //TODO: USE SHABBY PROVIDED METHOD HERE INSTEAD
            ShaderRef = Shader.Find(shaderString);

            if (ShaderRef == null) throw new FormatException($"Shader not found: {shaderString}");

            foreach (ConfigNode propertyNode in node.nodes) {
                try {
                    MaterialModifier modifier;
                    switch (propertyNode.name) {
                        case "FLOAT":
                            modifier = new FloatPropertyMaterialModifier(propertyNode);
                            break;

                        case "COLOR":
                            modifier = new ColorPropertyMaterialModifier(propertyNode);
                            break;

                        case "TEXTURE":
                            modifier = new TexturePropertyMaterialModifier(propertyNode);
                            var textureModifier = modifier as TexturePropertyMaterialModifier;
                            if (textureModifier.IsMain) {
                                if (MainTexture != null) {
                                    MainTexture = textureModifier;
                                }
                                else {
                                    Debug.LogWarning(
                                        $"Material texture property {textureModifier.TextureUrl} is marked as main, but material already has a main texture! \n" +
                                        $"Defaulting to {MainTexture.TextureUrl}");
                                }
                            }

                            _texturePropertyMaterialModifiers.Add(textureModifier);
                            break;

                        default:
                            throw new FormatException($"Invalid property type '{propertyNode.name}' in material");
                            break;
                    }

                    _materialModifiers.Add(modifier);
                }

                catch (Exception e) {
                    Debug.LogError(e.Message);
                }
            }
        }
    }
}