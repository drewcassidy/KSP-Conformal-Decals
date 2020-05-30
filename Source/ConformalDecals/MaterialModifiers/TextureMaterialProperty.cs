using System;
using UnityEngine;

namespace ConformalDecals.MaterialModifiers {
    public class TextureMaterialProperty : MaterialProperty {
        public string TextureUrl { get; }
        public Texture2D TextureRef { get; }
        
        public bool IsNormal { get; }
        public bool IsMain { get; }
        public bool AutoScale { get; }

        public Rect TileRect { get; }
        
        public float AspectRatio => TileRect.height / TileRect.width;

        private readonly Vector2 _textureOffset;
        private readonly Vector2 _textureScale;

        public TextureMaterialProperty(ConfigNode node) : base(node) {
            TextureUrl = node.GetValue("textureURL");

            var textureInfo = GameDatabase.Instance.GetTextureInfo(TextureUrl);

            if (textureInfo == null)
                throw new Exception($"Cannot find texture: '{TextureUrl}'");

            TextureRef = IsNormal ? textureInfo.normalMap : textureInfo.texture;

            if (TextureRef == null)
                throw new Exception($"Cannot get texture from texture info '{TextureUrl}' isNormalMap = {IsNormal}");

            IsNormal = ParsePropertyBool(node, "isNormalMap", true);
            IsMain = ParsePropertyBool(node, "isMain", true);
            AutoScale = ParsePropertyBool(node, "autoScale", true);
            TileRect = ParsePropertyRect(node, "tileRect", true, new Rect(0, 0, TextureRef.width, TextureRef.height));

            _textureScale.x = TileRect.width / TextureRef.width;
            _textureScale.y = TileRect.height / TextureRef.height;

            _textureOffset.x = TileRect.x / TextureRef.width;
            _textureOffset.y = TileRect.y / TextureRef.height;
        }

        public override void Modify(Material material) {
            material.SetTexture(_propertyID, TextureRef);
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