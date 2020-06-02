using System;
using System.Collections.Generic;
using Smooth.Delegates;
using UnityEngine;

namespace ConformalDecals.MaterialModifiers {
    public class MaterialPropertyCollection : ScriptableObject {
        private static int _opacityID = Shader.PropertyToID("_Opacity");
        private static int _cutoffID  = Shader.PropertyToID("_Cutoff");

        public TextureMaterialProperty MainTextureProperty { get; set; }

        public bool UseBaseNormal { get; private set; }

        private List<MaterialProperty>        _materialProperties;
        private List<TextureMaterialProperty> _textureMaterialProperties;

        public String BaseNormalSrc { get; private set; }
        public String BaseNormalDest { get; private set; }

        public Material DecalMaterial {
            get {
                if (_protoDecalMaterial == null) {
                    _protoDecalMaterial = MakeMaterial(_decalShader);
                }

                return _protoDecalMaterial;
            }
        }

        private Shader _decalShader;
        private Material _protoDecalMaterial;

        private const string _normalTextureName = "_BumpMap";

        public void Initialize(ConfigNode node, PartModule module) {

            // Initialize fields
            _materialProperties = new List<MaterialProperty>();
            _textureMaterialProperties = new List<TextureMaterialProperty>();

            // Get shader
            var shaderString = node.GetValue("shader") ?? throw new FormatException("Missing shader name in material");

            _decalShader = Shabby.Shabby.FindShader(shaderString);

            // note to self: null coalescing does not work on UnityEngine classes
            if (_decalShader == null) {
                throw new FormatException($"Shader not found: '{shaderString}'");
            }

            // Get useBaseNormal value
            var useBaseNormalString = node.GetValue("useBaseNormal");

            if (useBaseNormalString != null) {
                if (bool.TryParse(useBaseNormalString, out var useBaseNormalRef)) {
                    UseBaseNormal = useBaseNormalRef;
                }
                else {
                    throw new FormatException($"Improperly formatted bool value for 'useBaseNormal' : {useBaseNormalString}");
                }
            }
            else {
                UseBaseNormal = false;
            }

            // Get basenormal source and destination property names
            BaseNormalSrc = node.GetValue("baseNormalSource") ?? _normalTextureName;
            BaseNormalDest = node.GetValue("baseNormalDestination") ?? _normalTextureName;

            // Parse all materialProperties
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
                                if (MainTextureProperty == null) {
                                    MainTextureProperty = textureModifier;
                                }
                                else {
                                    // multiple textures have been marked as main!
                                    // non-fatal issue, ignore this one and keep using current main texture
                                    module.LogWarning(
                                        $"Material texture property {textureModifier.TextureUrl} is marked as main, but material already has a main texture! \n" +
                                        $"Defaulting to {MainTextureProperty.TextureUrl}");
                                }
                            }

                            _textureMaterialProperties.Add(textureModifier);
                            break;

                        default:
                            throw new FormatException($"Invalid property type '{propertyNode.name}' in material");
                    }

                    _materialProperties.Add(property);
                }

                catch (Exception e) {
                    // Catch exception from parsing current material property
                    // And print it to the log as an Error
                    module.LogException("Exception while parsing material node", e);
                }
            }

            module.Log($"Parsed {_materialProperties.Count} properties");
        }
        
        public void SetScale(Material material, Vector2 scale) {
            foreach (var textureProperty in _textureMaterialProperties) {
                textureProperty.UpdateScale(material, scale);
            }
        }
        
        public void SetOpacity(Material material, float opacity) {
            material.SetFloat(_opacityID, opacity);
        }

        public void SetCutoff(Material material, float cutoff) {
            material.SetFloat(_cutoffID, cutoff);
        }

        private Material MakeMaterial(Shader shader) {
            var material = new Material(shader);
            foreach (MaterialProperty property in _materialProperties) {
                property.Modify(material);
            }

            return material;
        }
    }
}