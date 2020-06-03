using System;
using UnityEngine;

namespace ConformalDecals.MaterialModifiers {
    public class MaterialTextureProperty : MaterialProperty {
        public Texture2D TextureRef { get; }

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
                TextureRef = Texture2D.normalTexture;
            }
            else if ((textureUrl == null && !IsNormal) || textureUrl == "White") {
                TextureRef = Texture2D.whiteTexture;
            }
            else if (textureUrl == "Black") {
                TextureRef = Texture2D.blackTexture;
            }
            else {
                var textureInfo = GameDatabase.Instance.GetTextureInfo(textureUrl);

                if (textureInfo == null) throw new Exception($"Cannot find texture: '{textureUrl}'");

                TextureRef = IsNormal ? textureInfo.normalMap : textureInfo.texture;
            }

            if (TextureRef == null) throw new Exception($"Cannot get texture from texture info '{textureUrl}' isNormalMap = {IsNormal}");

            _tileRect = ParsePropertyRect(node, "tileRect", true, new Rect(0, 0, TextureRef.width, TextureRef.height));

            _textureScale.x = _tileRect.width / TextureRef.width;
            _textureScale.y = _tileRect.height / TextureRef.height;

            _textureOffset.x = _tileRect.x / TextureRef.width;
            _textureOffset.y = _tileRect.y / TextureRef.height;
        }

        public MaterialTextureProperty(string name, Texture2D texture, Rect tileRect = default,
            bool isNormal = false, bool isMain = false, bool autoScale = false) : base(name) {

            TextureRef = texture;

            _tileRect = tileRect == default ? new Rect(0, 0, TextureRef.width, TextureRef.height) : tileRect;

            IsNormal = isNormal;
            IsMain = isMain;
            AutoScale = autoScale;

            _textureScale.x = _tileRect.width / TextureRef.width;
            _textureScale.y = _tileRect.height / TextureRef.height;

            _textureOffset.x = _tileRect.x / TextureRef.width;
            _textureOffset.y = _tileRect.y / TextureRef.height;
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