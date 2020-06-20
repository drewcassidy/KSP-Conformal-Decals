using System;
using ConformalDecals.Util;
using UnityEngine;

namespace ConformalDecals.MaterialProperties {
    public class MaterialFloatProperty : MaterialProperty {
        [SerializeField] public float value;

        public override void ParseNode(ConfigNode node) {
            base.ParseNode(node);

            ParseUtil.ParseFloatIndirect(ref value, node, "value");
        }

        public override void Modify(Material material) {
            if (material == null) throw new ArgumentNullException(nameof(material));

            material.SetFloat(_propertyID, value);
        }
    }
}