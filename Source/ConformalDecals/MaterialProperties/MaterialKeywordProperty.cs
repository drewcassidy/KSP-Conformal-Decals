using ConformalDecals.Util;
using UnityEngine;

namespace ConformalDecals.MaterialProperties {
    public class MaterialKeywordProperty : MaterialProperty {
        [SerializeField] public bool value = true;
        
        public override void ParseNode(ConfigNode node) {
            base.ParseNode(node);

            ParseUtil.ParseBoolIndirect(ref value, node, "value");
        }

        public override void Modify(Material material) {
            if (value) material.EnableKeyword(_propertyName);
            else material.DisableKeyword(_propertyName);
        }
    }
}