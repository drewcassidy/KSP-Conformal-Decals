using System;
using UnityEngine;

namespace ConformalDecals.MaterialProperties {
    public class MaterialFloatProperty : MaterialProperty {
        [SerializeField] public float value;

        public override void ParseNode(ConfigNode node) {
            base.ParseNode(node);

            value = ParsePropertyFloat(node, "value", true, value);
        }

        public override void Modify(Material material) {
            if (material == null) throw new ArgumentNullException("material cannot be null");

            material.SetFloat(_propertyID, value);
        }
    }
}