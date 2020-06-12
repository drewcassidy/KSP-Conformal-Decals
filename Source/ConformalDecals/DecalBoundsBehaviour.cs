using System;
using UnityEngine;

namespace ConformalDecals {
    public class DecalBoundsBehaviour : MonoBehaviour {
        public ModuleConformalDecal decalRenderer;
        
        private void OnWillRenderObject() {
            decalRenderer._shouldRender = true;
        }
    }
}