using System;
using UnityEngine;

namespace ConformalDecals.MaterialModifiers {
    public class TexturePropertyMaterialModifier : MaterialModifier {
        private readonly string    _textureURL;
        private readonly Texture2D _texture;

        private Vector2 _textureOffset;
        private Vector2 _textureScale;

        public bool IsNormal { get; }
        public bool IsMain { get; }
        public bool AutoScale { get; }

        public Rect TileRect { get; }

        public TexturePropertyMaterialModifier(ConfigNode node) : base(node) {
            _textureURL = node.GetValue("textureURL");

            var textureInfo = GameDatabase.Instance.GetTextureInfo(_textureURL);

            if (textureInfo == null)
                throw new Exception($"Cannot find texture: '{_textureURL}'");

            _texture = IsNormal ? textureInfo.normalMap : textureInfo.texture;

            if (_texture == null)
                throw new Exception($"Cannot get texture from texture info '{_textureURL}' isNormalMap = {IsNormal}");

            IsNormal = ParsePropertyBool(node, "isNormalMap", true, false);
            IsMain = ParsePropertyBool(node, "isMain", true, false);
            AutoScale = ParsePropertyBool(node, "autoScale", true, false);
            TileRect = ParsePropertyRect(node, "tileRect", true, new Rect(0, 0, _texture.width, _texture.height));

            _textureScale.x = TileRect.width / _texture.width;
            _textureScale.y = TileRect.height / _texture.height;

            _textureOffset.x = TileRect.x / _texture.width;
            _textureOffset.y = TileRect.y / _texture.height;
        }

        public override void Modify(Material material) {
            material.SetTexture(_propertyID, _texture);
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