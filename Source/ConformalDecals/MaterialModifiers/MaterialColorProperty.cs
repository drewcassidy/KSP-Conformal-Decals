using System;
using UnityEngine;

namespace ConformalDecals.MaterialModifiers {
    public class MaterialColorProperty : MaterialProperty {
        private readonly Color _color;

        public MaterialColorProperty(ConfigNode node) : base(node) {
            _color = ParsePropertyColor(node, "color", false);
        }

        public MaterialColorProperty(string name, Color value) : base(name) {
            _color = value;
        }

        public override void Modify(Material material) {
            material.SetColor(_propertyID, _color);
        }
    }
}