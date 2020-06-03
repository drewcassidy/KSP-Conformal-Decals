using System;
using UnityEngine;

namespace ConformalDecals.MaterialModifiers {
    public class MaterialTextureProperty : MaterialProperty {
        public Texture2D texture;

        public bool IsNormal { get; }
        public bool IsMain { get; }
        public bool AutoScale { get; }

        private readonly Rect _tileRect;

        public float AspectRatio => _tileRect.height / _tileRect.width;

        private readonly Vector2 _textureOffset;
        private readonly Vector2 _textureScale;

        public MaterialTextureProperty(ConfigNode node) : base(node) {
            IsNormal = ParsePropertyBool(node, "isNormalMap", true, PropertyName == "_BumpMap");
            IsMain = ParsePropertyBool(node, "isMain", true);
            AutoScale = ParsePropertyBool(node, "autoScale", true);
            var textureUrl = node.GetValue("textureURL");

            if ((textureUrl == null && IsNormal) || textureUrl == "Bump") {
                texture = Texture2D.normalTexture;
            }
            else if ((textureUrl == null && !IsNormal) || textureUrl == "White") {
                texture = Texture2D.whiteTexture;
            }
            else if (textureUrl == "Black") {
                texture = Texture2D.blackTexture;
            }
            else {
                var textureInfo = GameDatabase.Instance.GetTextureInfo(textureUrl);

                if (textureInfo == null) throw new Exception($"Cannot find texture: '{textureUrl}'");

                texture = IsNormal ? textureInfo.normalMap : textureInfo.texture;
            }

            if (texture == null) throw new Exception($"Cannot get texture from texture info '{textureUrl}' isNormalMap = {IsNormal}");

            _tileRect = ParsePropertyRect(node, "tileRect", true, new Rect(0, 0, texture.width, texture.height));

            _textureScale.x = _tileRect.width / texture.width;
            _textureScale.y = _tileRect.height / texture.height;

            _textureOffset.x = _tileRect.x / texture.width;
            _textureOffset.y = _tileRect.y / texture.height;
        }

        public MaterialTextureProperty(string name, Texture2D texture, Rect tileRect = default,
            bool isNormal = false, bool isMain = false, bool autoScale = false) : base(name) {

            this.texture = texture;

            _tileRect = tileRect == default ? new Rect(0, 0, this.texture.width, this.texture.height) : tileRect;

            IsNormal = isNormal;
            IsMain = isMain;
            AutoScale = autoScale;

            _textureScale.x = _tileRect.width / this.texture.width;
            _textureScale.y = _tileRect.height / this.texture.height;

            _textureOffset.x = _tileRect.x / this.texture.width;
            _textureOffset.y = _tileRect.y / this.texture.height;
        }
        
        public override void Modify(Material material) {
            material.SetTexture(_propertyID, texture);
            material.SetTextureOffset(_propertyID, _textureOffset);
            material.SetTextureScale(_propertyID, _textureScale);
        }

        public void UpdateScale(Material material, Vector2 scale) {
            if (AutoScale) {
                material.SetTextureScale(_propertyID, new Vector2(_textureScale.x * scale.x, _textureScale.y * scale.y));
            }
        }
    }
}