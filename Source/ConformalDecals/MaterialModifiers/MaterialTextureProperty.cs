using System;
using UnityEngine;

namespace ConformalDecals.MaterialModifiers {
    public class MaterialTextureProperty : MaterialProperty {
        [SerializeField] public Texture2D texture;

        [SerializeField] public bool isNormal;
        [SerializeField] public bool isMain;
        [SerializeField] public bool autoScale;

        [SerializeField] private bool    _hasTile;
        [SerializeField] private Rect    _tileRect;
        [SerializeField] private Vector2 _textureOffset = Vector2.zero;
        [SerializeField] private Vector2 _textureScale  = Vector2.one;

        public float AspectRatio {
            get {
                if (texture == null) {
                    Debug.Log("Returning 1");
                    return 1;
                }

                if (!_hasTile) {
                    Debug.Log("Returning texture aspect ratio");
                    return ((float) texture.height) / ((float) texture.width);
                }

                return _tileRect.height / _tileRect.width;
            }
        }

        public Rect TileRect {
            get => _tileRect;
            set {
                _hasTile = !(Mathf.Abs(value.width) < 0.1) || !(Mathf.Abs(value.height) < 0.1);

                _tileRect = value;
                UpdateTiling();
            }
        }

        public override void ParseNode(ConfigNode node) {
            base.ParseNode(node);

            isNormal = ParsePropertyBool(node, "isNormalMap", true, (PropertyName == "_BumpMap") || isNormal);
            isMain = ParsePropertyBool(node, "isMain", true, isMain);
            autoScale = ParsePropertyBool(node, "autoScale", true, autoScale);

            SetTexture(node.GetValue("textureUrl"));

            if (node.HasValue("tileRect")) {
                TileRect = ParsePropertyRect(node, "tileRect", true, _tileRect);
            }
        }

        public void SetTexture(string textureUrl) {
            if ((textureUrl == null && isNormal) || textureUrl == "Bump") {
                texture = Texture2D.normalTexture;
            }
            else if ((textureUrl == null && !isNormal) || textureUrl == "White") {
                texture = Texture2D.whiteTexture;
            }
            else if (textureUrl == "Black") {
                texture = Texture2D.blackTexture;
            }
            else {
                var textureInfo = GameDatabase.Instance.GetTextureInfo(textureUrl);

                if (textureInfo == null) throw new Exception($"Cannot find texture: '{textureUrl}'");

                texture = isNormal ? textureInfo.normalMap : textureInfo.texture;
            }

            if (texture == null) throw new Exception($"Cannot get texture from texture info '{textureUrl}', isNormalMap = {isNormal}");
            UpdateTiling();
        }

        public override void Modify(Material material) {
            if (material == null) throw new ArgumentNullException(nameof(material));
            if (texture == null) {
                texture = Texture2D.whiteTexture;
                throw new NullReferenceException("texture is null, but should not be");
            }

            material.SetTexture(_propertyID, texture);
            material.SetTextureOffset(_propertyID, _textureOffset);
            material.SetTextureScale(_propertyID, _textureScale);
        }

        public void UpdateScale(Material material, Vector2 scale) {
            if (autoScale) {
                material.SetTextureScale(_propertyID, new Vector2(_textureScale.x * scale.x, _textureScale.y * scale.y));
            }
        }

        private void UpdateTiling() {
            if (_hasTile) {
                _textureScale.x = Mathf.Approximately(0, _tileRect.width) ? 1 : _tileRect.width / texture.width;
                _textureScale.y = Mathf.Approximately(0, _tileRect.height) ? 1 : _tileRect.height / texture.height;

                _textureOffset.x = _tileRect.x / texture.width;
                _textureOffset.y = _tileRect.y / texture.height;
            }
            else {
                _textureScale = Vector2.one;
                _textureOffset = Vector2.zero;
            }
        }
    }
}