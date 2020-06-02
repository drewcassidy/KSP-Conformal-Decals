using System;
using UnityEngine;

namespace ConformalDecals.MaterialModifiers {
    public class MaterialFloatProperty : MaterialProperty {
        private readonly float _value;

        public MaterialFloatProperty(ConfigNode node) : base(node) {
            _value = ParsePropertyFloat(node, "value", false);
        }

        public override void Modify(Material material) {
            material.SetFloat(_propertyID, _value);
        }
    }
}