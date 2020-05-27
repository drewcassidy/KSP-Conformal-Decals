using System;
using UnityEngine;

namespace ConformalDecals.MaterialModifiers {
    public abstract class MaterialModifier {
        public string Name { get; }

        protected readonly int _propertyID;


        protected MaterialModifier(ConfigNode node) {
            Name = node.GetValue("name");

            if (Name == null)
                throw new FormatException("name not found, cannot create material modifier");

            if (Name == string.Empty)
                throw new FormatException("name is empty, cannot create material modifier");

            _propertyID = Shader.PropertyToID(Name);
        }

        public abstract void Modify(Material material);

        private delegate bool TryParseDelegate<T>(string valueString, out T value);

        protected bool ParsePropertyBool(ConfigNode node, string valueName, bool isOptional = false, bool defaultValue = false) {
            return ParseProperty<bool>(node, valueName, bool.TryParse, isOptional, defaultValue);
        }

        protected float ParsePropertyFloat(ConfigNode node, string valueName, bool isOptional = false, float defaultValue = 0.0f) {
            return ParseProperty<float>(node, valueName, float.TryParse, isOptional, defaultValue);
        }

        protected int ParsePropertyInt(ConfigNode node, string valueName, bool isOptional = false, int defaultValue = 0) {
            return ParseProperty<int>(node, valueName, int.TryParse, isOptional, defaultValue);
        }

        protected Color ParsePropertyColor(ConfigNode node, string valueName, bool isOptional = false, Color defaultValue = default(Color)) {
            return ParseProperty<Color>(node, valueName, ParseExtensions.TryParseColor, isOptional, defaultValue);
        }

        protected Rect ParsePropertyRect(ConfigNode node, string valueName, bool isOptional = false, Rect defaultValue = default(Rect)) {
            return ParseProperty<Rect>(node, valueName, ParseExtensions.TryParseRect, isOptional, defaultValue);
        }
        
        protected Vector2 ParsePropertyVector2(ConfigNode node, string valueName, bool isOptional = false, Vector2 defaultValue = default(Vector2)) {
            return ParseProperty<Vector2>(node, valueName, ParseExtensions.TryParseVector2, isOptional, defaultValue);
        }

        private T ParseProperty<T>(ConfigNode node, string valueName, TryParseDelegate<T> tryParse, bool isOptional = false, T defaultValue = default(T)) {
            string valueString = node.GetValue(valueName);

            if (isOptional) {
                if (string.IsNullOrEmpty(valueString)) return defaultValue;
            }
            else {
                if (valueString == null)
                    throw new FormatException($"Missing {typeof(T)} value {valueName} in property '{Name}'");

                if (valueString == string.Empty)
                    throw new FormatException($"Empty {typeof(T)} value {valueName} in property '{Name}'");
            }

            if (tryParse(valueString, out var value)) {
                return value;
            }

            if (isOptional) {
                return defaultValue;
            }

            else {
                throw new FormatException($"Improperly formatted {typeof(T)} value {valueName} in property '{Name}'");
            }
        }
    }
}