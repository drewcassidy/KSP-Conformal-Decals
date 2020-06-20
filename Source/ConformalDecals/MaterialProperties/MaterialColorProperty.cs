using System;
using ConformalDecals.Util;
using UnityEngine;

namespace ConformalDecals.MaterialProperties {
    public class MaterialColorProperty : MaterialProperty {
        [SerializeField] public Color32 color = new Color32(0, 0, 0, byte.MaxValue);

        public override void ParseNode(ConfigNode node) {
            base.ParseNode(node);

            ParseUtil.ParseColor32Indirect(ref color, node, "color");
        }

        public override void Modify(Material material) {
            if (material == null) throw new ArgumentNullException(nameof(material));

            material.SetColor(_propertyID, color);
        }
    }
}