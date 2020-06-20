using System;
using UnityEngine;

namespace ConformalDecals.Test {
    public class TestLayers : PartModule {

        [KSPField(guiActive = true)]
        public int layer = 2;

        public override void OnStart(StartState state) {
            base.OnStart(state);


            Part.layerMask.value |= (1 << 3);
        }

        public void Update() {
            foreach (var collider in GameObject.FindObjectsOfType<Collider>()) {
                if (collider.gameObject.layer == 3) {
                    Debug.Log($"Has layer 3: {collider.gameObject.name}");
                }
            }
        }

        [KSPEvent(guiActive = true, guiActiveEditor = true, guiName = "switch layers")]
        public void Switch() {
            Debug.Log(Part.layerMask.value);

            var cube = part.FindModelTransform("test");
            layer = (layer + 1) % 32;
            cube.gameObject.layer = layer;
        }
    }
}