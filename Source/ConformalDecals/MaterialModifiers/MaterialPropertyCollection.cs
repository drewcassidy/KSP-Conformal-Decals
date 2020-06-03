using System;
using System.Collections.Generic;
using UnityEngine;

namespace ConformalDecals.MaterialModifiers {
    public class MaterialPropertyCollection : ScriptableObject {
        private static readonly int OpacityId = Shader.PropertyToID("_Opacity");
        private static readonly int CutoffId  = Shader.PropertyToID("_Cutoff");

        public MaterialTextureProperty MainMaterialTextureProperty { get; set; }

        private List<MaterialProperty>        _materialProperties;
        private List<MaterialTextureProperty> _textureMaterialProperties;

        public Material DecalMaterial {
            get {
                if (_decalMaterial == null) {
                    _decalMaterial = new Material(_decalShader);
                    UpdateMaterial(_decalMaterial);
                }

                return _decalMaterial;
            }
        }

        public Shader DecalShader => _decalShader;

        public float AspectRatio => MainMaterialTextureProperty?.AspectRatio ?? 1f;

        [SerializeField] private Shader _decalShader;

        private Material _decalMaterial;

        public void Initialize() {
            _materialProperties = new List<MaterialProperty>();
            _textureMaterialProperties = new List<MaterialTextureProperty>();
        }

        public void AddProperty(MaterialProperty property) {
            if (property == null) throw new ArgumentNullException("Tried to add a null property");
            if (_materialProperties == null || _textureMaterialProperties == null) {
                Initialize();
                Debug.LogWarning("Tried to add a property to uninitialized property collection! correcting now.");
            }

            foreach (var p in _materialProperties) {
                if (p.PropertyName == property.PropertyName) {
                    _materialProperties.Remove(property);
                }
            }

            _materialProperties.Add(property);

            if (property is MaterialTextureProperty textureProperty) {
                foreach (var p in _textureMaterialProperties) {
                    if (p.PropertyName == textureProperty.PropertyName) {
                        _textureMaterialProperties.Remove(textureProperty);
                    }
                }

                _textureMaterialProperties.Add(textureProperty);

                if (textureProperty.IsMain) MainMaterialTextureProperty ??= textureProperty;
            }
        }

        public void SetShader(string shaderName) {
            if (string.IsNullOrEmpty(shaderName)) {
                if (_decalShader == null) {
                    Debug.Log("Using default decal shader");
                    shaderName = "ConformalDecals/Paint/Diffuse";
                }
                else {
                    return;
                }
            }

            var shader = Shabby.Shabby.FindShader(shaderName);

            if (shader == null) throw new FormatException($"Unable to find specified shader '{shaderName}'");

            _decalShader = shader;
        }

        public void SetRenderQueue(int queue) {
            DecalMaterial.renderQueue = queue;
        }
        
        public void SetScale(Vector2 scale) {
            foreach (var textureProperty in _textureMaterialProperties) {
                textureProperty.UpdateScale(DecalMaterial, scale);
            }
        }

        public void SetOpacity(float opacity) {
            DecalMaterial.SetFloat(OpacityId, opacity);
        }

        public void SetCutoff(float cutoff) {
            DecalMaterial.SetFloat(CutoffId, cutoff);
        }

        public void UpdateMaterials() {
            UpdateMaterial(_decalMaterial);
        }

        public void UpdateMaterial(Material material) {
            foreach (var property in _materialProperties) {
                property.Modify(material);
            }
        }
    }
}