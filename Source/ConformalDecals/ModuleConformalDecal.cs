using System;
using UnityEngine;

namespace ConformalDecals {
    public class ModuleConformalDecal : PartModule {
        [KSPField] public string decalPreviewTransform = "";

        public void OnStart(StartState state) {
            if ((state & StartState.Editor) != 0) {
                GameEvents.onEditorPartEvent.Add(OnEditorEvent);
            }
            else {
                Attach();
            }
        }

        public void OnEditorEvent(ConstructionEventType eventType, Part eventPart) {
            if (eventPart != this.part) return;
            switch (eventType) {
                case ConstructionEventType.PartAttached:
                    Attach();
                    break;
                case ConstructionEventType.PartDetached:
                    Detach();
                    break;
                case ConstructionEventType.PartOffsetting:
                case ConstructionEventType.PartRotated:
                case ConstructionEventType.PartDragging:
                    Project();
                    break;
            }

        }

        public void Attach() {
            if (part.parent == null) {
                this.LogError("Attach function called but part has no parent!");
                return;
            }

            Camera.onPreCull += Render;
        }

        public void Detach() {
            if (part.parent != null) {
                this.LogError("Detach function called but part still has parent!");
                return;
            }

            Camera.onPreCull -= Render;
        }

        public void OnDisable() {
            Camera.onPreCull -= Render;
        }

        public void Project() { }

        public void Render(Camera camera) { }
    }
}