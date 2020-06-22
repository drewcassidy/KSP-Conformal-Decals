using System;
using UnityEngine;

namespace ConformalDecals.MaterialProperties {
    public abstract class MaterialProperty : ScriptableObject {
        public string PropertyName {
            get => _propertyName;
            set {
                _propertyName = value;
                _propertyID = Shader.PropertyToID(_propertyName);
            }
        }

        [SerializeField] protected int    _propertyID;
        [SerializeField] protected string _propertyName;

        public virtual void ParseNode(ConfigNode node) {
            if (node == null) throw new ArgumentNullException(nameof(node));

            PropertyName = node.GetValue("name");
        }

        public abstract void Modify(Material material);
    }
}