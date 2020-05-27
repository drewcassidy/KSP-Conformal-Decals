using System;
using UnityEngine;

namespace ConformalDecals.MaterialModifiers {
    public class FloatPropertyMaterialModifier : MaterialModifier {
        private readonly float _value;

        public FloatPropertyMaterialModifier(ConfigNode node) : base(node) {
            _value = ParsePropertyFloat(node, "value", false);
        }

        public override void Modify(Material material) {
            material.SetFloat(_propertyID, _value);
        }
    }
}