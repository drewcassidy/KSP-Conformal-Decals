using System;
using System.Collections.Generic;
using UnityEngine;

namespace ConformalDecals.MaterialModifiers {
    public class MaterialPropertyCollection : ScriptableObject {
        private static readonly int OpacityId = Shader.PropertyToID("_DecalOpacity");
        private static readonly int CutoffId  = Shader.PropertyToID("_Cutoff");

        public MaterialTextureProperty MainMaterialTextureProperty => _mainTexture;

        public Shader DecalShader => _shader;

        public Material DecalMaterial {
            get {
                Debug.Log($"{_textureMaterialProperties == null}");
                if (_decalMaterial == null) {
                    _decalMaterial = new Material(_shader);
                    UpdateMaterial(_decalMaterial);
                }

                return _decalMaterial;
            }
        }

        public Material PreviewMaterial {
            get {
                if (_previewMaterial == null) {
                    _previewMaterial = new Material(_shader);
                    UpdateMaterial(_previewMaterial);
                    _previewMaterial.EnableKeyword("DECAL_PREVIEW");
                }

                return _previewMaterial;
            }
        }

        public float AspectRatio {
            get {
                if (MainMaterialTextureProperty == null) return 1;
                return MainMaterialTextureProperty.AspectRatio;
            }
        }

        [SerializeField] private Shader                        _shader;
        [SerializeField] private List<MaterialProperty>        _materialProperties;
        [SerializeField] private List<MaterialTextureProperty> _textureMaterialProperties;
        [SerializeField] private MaterialTextureProperty       _mainTexture;

        private Material _decalMaterial;
        private Material _previewMaterial;

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
                if (p.Name == property.Name) {
                    _materialProperties.Remove(property);
                }
            }

            _materialProperties.Add(property);

            if (property is MaterialTextureProperty textureProperty) {
                foreach (var p in _textureMaterialProperties) {
                    if (p.Name == textureProperty.Name) {
                        _textureMaterialProperties.Remove(textureProperty);
                    }
                }

                _textureMaterialProperties.Add(textureProperty);

                if (textureProperty.isMain) _mainTexture ??= textureProperty;
            }
        }

        public void SetShader(string shaderName) {
            if (string.IsNullOrEmpty(shaderName)) {
                if (_shader == null) {
                    Debug.Log("Using default decal shader");
                    shaderName = "ConformalDecals/Paint/Diffuse";
                }
                else {
                    return;
                }
            }

            var shader = Shabby.Shabby.FindShader(shaderName);

            if (shader == null) throw new FormatException($"Unable to find specified shader '{shaderName}'");

            _shader = shader;
            _decalMaterial = null;
            _previewMaterial = null;
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
            if (_decalMaterial == null) {
                _decalMaterial = DecalMaterial;
            }

            if (_previewMaterial == null) {
                _previewMaterial = PreviewMaterial;
            }

            UpdateMaterial(_decalMaterial);
            UpdateMaterial(_previewMaterial);
        }

        public void UpdateMaterial(Material material) {
            if (material == null) throw new ArgumentNullException("material cannot be null");

            foreach (var property in _materialProperties) {
                property.Modify(material);
            }
        }
    }
}