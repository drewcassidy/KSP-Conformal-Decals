using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UniLinq;
using UnityEngine;

namespace ConformalDecals.MaterialModifiers {
    public class MaterialPropertyCollection : ScriptableObject, ISerializationCallbackReceiver {
        private static readonly int OpacityId = Shader.PropertyToID("_DecalOpacity");
        private static readonly int CutoffId  = Shader.PropertyToID("_Cutoff");

        public Shader DecalShader => _shader;

        public Material DecalMaterial {
            get {
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

        public MaterialTextureProperty MainTexture {
            get => _mainTexture;
            set {
                if (!_materialProperties.ContainsValue(value))
                    throw new ArgumentException($"Texture property {value.name} is not part of this property collection.");

                _mainTexture = value;
            }
        }

        public float AspectRatio {
            get {
                if (MainTexture == null) {
                    Debug.Log("No main texture specified! returning 1 for aspect ratio");
                    return 1;
                }
                return MainTexture.AspectRatio;
            }
        }

        [SerializeField] private string[]           _serializedIDs;
        [SerializeField] private MaterialProperty[] _serializedProperties;

        [SerializeField] private Shader                  _shader;
        [SerializeField] private MaterialTextureProperty _mainTexture;

        private Dictionary<string, MaterialProperty> _materialProperties;

        private Material _decalMaterial;
        private Material _previewMaterial;

        public void OnBeforeSerialize() {
            Debug.Log($"Serializing MaterialPropertyCollection {this.GetInstanceID()}");
            if (_materialProperties == null) throw new SerializationException("Tried to serialize an unininitalized MaterialPropertyCollection");

            _serializedIDs = _materialProperties.Keys.ToArray();
            _serializedProperties = _materialProperties.Values.ToArray();
        }

        public void OnAfterDeserialize() {
            Debug.Log($"Deserializing MaterialPropertyCollection {this.GetInstanceID()}");
            if (_serializedIDs == null) throw new SerializationException("ID array is null");
            if (_serializedProperties == null) throw new SerializationException("Property array is null");
            if (_serializedProperties.Length != _serializedIDs.Length) throw new SerializationException("Material property arrays are different lengths.");

            _materialProperties ??= new Dictionary<string, MaterialProperty>();

            for (var i = 0; i < _serializedIDs.Length; i++) {
                var property = MaterialProperty.Instantiate(_serializedProperties[i]);
                Debug.Log($"insantiating {property.GetType().Name} {property.GetInstanceID()}");
                _materialProperties.Add(_serializedIDs[i], property);

                if (property is MaterialTextureProperty textureProperty) {
                    _mainTexture = textureProperty;
                }
            }
        }

        public void Awake() {
            Debug.Log($"MaterialPropertyCollection {this.GetInstanceID()} onAwake");
            _materialProperties ??= new Dictionary<string, MaterialProperty>();
        }

        public void AddProperty(MaterialProperty property) {
            if (property == null) throw new ArgumentNullException("Tried to add a null property");

            _materialProperties.Add(property.name, property);

            if (property is MaterialTextureProperty textureProperty) {
                if (textureProperty.isMain) _mainTexture ??= textureProperty;
            }
        }

        public void ParseProperty<T>(ConfigNode node) where T : MaterialProperty {
            var name = node.GetValue("name");
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("node has no name");

            Debug.Log($"Parsing material property {name}");

            T newProperty;

            if (_materialProperties.ContainsKey(name)) {
                if (_materialProperties[name] is T property) {
                    newProperty = property;
                    property.ParseNode(node);
                }
                else {
                    throw new ArgumentException("Material property already exists for {name} but it has a different type");
                }
            }
            else {
                newProperty = MaterialProperty.CreateInstance<T>();
                Debug.Log($"Adding new material property of type {newProperty.GetType().Name} {newProperty.GetInstanceID()}");
                newProperty.ParseNode(node);
                _materialProperties.Add(name, newProperty);
            }

            if (newProperty is MaterialTextureProperty textureProperty && textureProperty.isMain) {
                _mainTexture = textureProperty;
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

        public void UpdateScale(Vector2 scale) {
            foreach (var entry in _materialProperties) {
                if (entry.Value is MaterialTextureProperty textureProperty) {
                    textureProperty.UpdateScale(DecalMaterial, scale);
                    textureProperty.UpdateScale(PreviewMaterial, scale);
                }
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
            else {
                UpdateMaterial(_decalMaterial);
            }

            if (_previewMaterial == null) {
                _previewMaterial = PreviewMaterial;
            }
            else {
                UpdateMaterial(_previewMaterial);
            }
        }

        public void UpdateMaterial(Material material) {
            if (material == null) throw new ArgumentNullException(nameof(material));

            foreach (var entry in _materialProperties) {
                entry.Value.Modify(material);
            }
        }
    }
}