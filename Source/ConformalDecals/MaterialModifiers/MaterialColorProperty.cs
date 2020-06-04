using System;
using UnityEngine;

namespace ConformalDecals.MaterialModifiers {
    public class MaterialColorProperty : MaterialProperty {
        [SerializeField] public Color color;

        public override void ParseNode(ConfigNode node) {
            base.ParseNode(node);

            color = ParsePropertyColor(node, "color", true, color);
        }

        public override void Modify(Material material) {
            if (material == null) throw new ArgumentNullException("material cannot be null");

            material.SetColor(_propertyID, color);
        }
    }
}