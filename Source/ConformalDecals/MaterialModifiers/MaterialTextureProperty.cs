using System;
using UnityEngine;

namespace ConformalDecals.MaterialModifiers {
    public class MaterialTextureProperty : MaterialProperty {
        [SerializeField] public Texture2D texture;

        [SerializeField] public bool isNormal;
        [SerializeField] public bool isMain;
        [SerializeField] public bool autoScale;
        [SerializeField] public bool autoTile;

        [SerializeField] private bool    _hasTile;
        [SerializeField] private Rect    _tileRect;
        [SerializeField] private Vector2 _baseTextureScale  = Vector2.one;
        
        [SerializeField] private Vector2 _textureOffset = Vector2.zero;
        [SerializeField] private Vector2 _textureScale  = Vector2.one;

        public float AspectRatio {
            get {
                if (texture == null) return 1;

                if (!_hasTile) return ((float) texture.height) / texture.width;

                return _tileRect.height / _tileRect.width;
            }
        }

        public Rect TileRect {
            get => _tileRect;
            set {
                if (autoTile) return;
                _hasTile = !(Mathf.Abs(value.width) < 0.1) || !(Mathf.Abs(value.height) < 0.1);

                _tileRect = value;
                UpdateTiling();
            }
        }

        public override void ParseNode(ConfigNode node) {
            base.ParseNode(node);

            isNormal = ParsePropertyBool(node, "isNormalMap", true, (PropertyName == "_BumpMap") || (PropertyName == "_DecalBumpMap") || isNormal);
            isMain = ParsePropertyBool(node, "isMain", true, isMain);
            autoScale = ParsePropertyBool(node, "autoScale", true, autoScale);
            autoTile = ParsePropertyBool(node, "autoTile", true, autoTile);

            SetTexture(node.GetValue("textureUrl"));

            if (node.HasValue("tileRect") && !autoTile) {
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

        public void UpdateScale(Vector2 scale) {
            if (autoScale) {
                _textureScale = _baseTextureScale * scale;
            }
        }

        public void UpdateTiling(Vector2 textureScale, Vector2 textureOffset) {
            if (autoTile) {
                _textureScale = textureScale;
                _textureOffset = textureOffset;
            }
        }

        private void UpdateTiling() {
            if (_hasTile) {
                _baseTextureScale.x = Mathf.Approximately(0, _tileRect.width) ? 1 : _tileRect.width / texture.width;
                _baseTextureScale.y = Mathf.Approximately(0, _tileRect.height) ? 1 : _tileRect.height / texture.height;
            }
            else {
                _baseTextureScale = Vector2.one;
            }
        }
    }
}