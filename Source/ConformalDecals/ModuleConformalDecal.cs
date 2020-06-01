using System;
using System.Collections.Generic;
using ConformalDecals.MaterialModifiers;
using UniLinq;
using UnityEngine;

namespace ConformalDecals {
    public class ModuleConformalDecal : PartModule {
        [KSPField] public string decalPreviewTransform   = "";
        [KSPField] public string decalModelTransform     = "";
        [KSPField] public string decalProjectorTransform = "";

        [KSPField(guiName = "#LOC_ConformalDecals_gui-scale", guiActive = false, guiActiveEditor = true, isPersistant = true, guiFormat = "F2", guiUnits = "m"),
         UI_FloatRange(minValue = 0.05f, maxValue = 4f, stepIncrement = 0.05f)]
        public float scale = 1.0f;

        [KSPField(guiName = "#LOC_ConformalDecals_gui-depth", guiActive = false, guiActiveEditor = true, isPersistant = true, guiFormat = "F2", guiUnits = "m"),
         UI_FloatRange(minValue = 0.05f, maxValue = 4f, stepIncrement = 0.05f)]
        public float depth = 1.0f;
        
        [KSPField(guiName = "#LOC_ConformalDecals_gui-opacity", guiActive = false, guiActiveEditor = true, isPersistant = true, guiFormat = "F2", guiUnits = "m"),
         UI_FloatRange(minValue = 0.05f, maxValue = 4f, stepIncrement = 0.05f)]
        public float opacity= 1.0f; 
        
        [KSPField(guiName = "#LOC_ConformalDecals_gui-cutoff", guiActive = false, guiActiveEditor = true, isPersistant = true, guiFormat = "F2", guiUnits = "m"),
         UI_FloatRange(minValue = 0.05f, maxValue = 4f, stepIncrement = 0.05f)]
        public float cutoff = 0.5f; 

        [KSPField(guiName = "#LOC_ConformalDecals_gui-aspectratio", guiActive = true, guiFormat = "F2")]
        public float aspectRatio = 1.0f;

        [KSPField] public MaterialPropertyCollection materialProperties;

        [KSPField] public Transform   decalPreviewTransformRef;
        [KSPField] public Transform   decalModelTransformRef;
        [KSPField] public Transform   decalProjectorTransformRef;
        [KSPField] public Transform   modelTransformRef;
        [KSPField] public Transform   colliderTransformRef;
        [KSPField] public BoxCollider colliderRef;

        private List<ProjectionTarget> _targets;

        private Matrix4x4 _orthoMatrix;
        private Bounds    _decalBounds;

        private bool IsAttached => part.parent != null;

        public override void OnLoad(ConfigNode node) {
            this.Log("Loading module");
            try {
                // parse MATERIAL node
                var materialNode = node.GetNode("MATERIAL") ?? throw new FormatException("Missing MATERIAL node in module");
                materialProperties = ScriptableObject.CreateInstance<MaterialPropertyCollection>();
                materialProperties.Initialize(materialNode, this);

                // get aspect ratio from main texture, if it exists
                var mainTexture = materialProperties.MainTextureProperty;
                if (mainTexture != null) {
                    aspectRatio = mainTexture.AspectRatio;
                }
                else {
                    aspectRatio = 1;
                }

                // find preview object references
                modelTransformRef = part.transform.Find("model");

                decalPreviewTransformRef = part.FindModelTransform(decalPreviewTransform);
                if (decalPreviewTransformRef == null) throw new FormatException("Missing decal preview reference");

                if (String.IsNullOrEmpty(decalModelTransform)) {
                    decalModelTransformRef = decalPreviewTransformRef;
                }
                else {
                    decalModelTransformRef = part.FindModelTransform(decalModelTransform);
                    if (decalModelTransformRef == null) throw new FormatException("Missing decal mesh reference");
                }

                if (String.IsNullOrEmpty(decalProjectorTransform)) {
                    decalProjectorTransformRef = modelTransformRef;
                }
                else {
                    decalProjectorTransformRef = part.FindModelTransform(decalProjectorTransform);
                    if (decalProjectorTransform == null) throw new FormatException("Missing decal projector reference");
                }

                colliderTransformRef = new GameObject("Decal Collider").transform;
                colliderTransformRef.parent = modelTransformRef;
                colliderTransformRef.position = decalProjectorTransformRef.position;
                colliderTransformRef.rotation = decalProjectorTransformRef.rotation;
                colliderTransformRef.gameObject.SetActive(false);

                colliderRef = colliderTransformRef.gameObject.AddComponent<BoxCollider>();
            }
            catch (Exception e) {
                this.LogException("Exception parsing partmodule", e);
            }
        }

        public override void OnStart(StartState state) {
            this.Log("Starting module");
            // generate orthogonal projection matrix and offset it by 0.5 on x and y axes
            _orthoMatrix = Matrix4x4.identity;
            _orthoMatrix[0, 3] = 0.5f;
            _orthoMatrix[1, 3] = 0.5f;

            if ((state & StartState.Editor) != 0) {
                // setup OnTweakEvent for scale and depth fields in editor
                GameEvents.onEditorPartEvent.Add(OnEditorEvent);
                GameEvents.onVariantApplied.Add(OnVariantApplied);
                Fields[nameof(scale)].uiControlEditor.onFieldChanged = OnTweakEvent;
                Fields[nameof(depth)].uiControlEditor.onFieldChanged = OnTweakEvent;
            }
            else {
                // if we start in the flight scene attached, call Attach
                if (IsAttached) Attach();
            }
        }

        private void OnDestroy() {
            GameEvents.onEditorPartEvent.Remove(OnEditorEvent);
            GameEvents.onVariantApplied.Remove(OnVariantApplied);
            
            // remove from preCull delegate
            Camera.onPreCull -= Render;
        }


        public void OnTweakEvent(BaseField field, object obj) {
            // scale or depth values have been changed, so update the projection matrix for each target
            Project();
        }

        public void OnVariantApplied(Part eventPart, PartVariant variant) {
            if (IsAttached && eventPart == part.parent) {
                Detach();
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
                case ConstructionEventType.PartRotating:
                case ConstructionEventType.PartDragging:
                    Project();
                    break;
            }
        }

        public void Attach() {
            if (!IsAttached) {
                this.LogError("Attach function called but part has no parent!");
                return;
            }

            this.Log($"Decal attached to {part.parent.partName}");
            this.Log($"{materialProperties == null}");
            this.Log($"{decalModelTransformRef == null}");

            if (_targets == null) {
                _targets = new List<ProjectionTarget>();
            }
            else {
                _targets.Clear();
            }

            // find all valid renderers
            var renderers = part.parent.FindModelComponents<MeshRenderer>();
            foreach (var renderer in renderers) {
                // skip disabled renderers
                if (renderer.gameObject.activeInHierarchy == false) continue;
                
                var meshFilter = renderer.GetComponent<MeshFilter>();
                if (meshFilter == null) continue; // object has a meshRenderer with no filter, invalid
                var mesh = meshFilter.mesh;
                if (mesh == null) continue; // object has a null mesh, invalid

                this.Log($"Adding target for object {meshFilter.gameObject.name} with the mesh {mesh.name}");
                // create new ProjectionTarget to represent the renderer
                var target = new ProjectionTarget(renderer, mesh, materialProperties);

                this.Log("done.");

                // add the target to the list
                _targets.Add(target);
            }

            // hide preview model
            decalModelTransformRef.gameObject.SetActive(false);

            // enable decal collider
            colliderTransformRef.gameObject.SetActive(true);

            // add to preCull delegate
            Camera.onPreCull += Render;
            
            Project();
        }

        public void Detach() {
            // unhide preview model
            decalModelTransformRef.gameObject.SetActive(true);

            // enable decal collider
            colliderTransformRef.gameObject.SetActive(false);

            // remove from preCull delegate
            Camera.onPreCull -= Render;
        }

        [KSPEvent(guiActive = false, guiName = "Project", guiActiveEditor = true, active = true)]
        public void Project() {
            if (!IsAttached) return;

            float width = scale;
            float height = scale * aspectRatio;
            // generate orthogonal matrix scale values
            _orthoMatrix[0, 0] = 1 / width;
            _orthoMatrix[1, 1] = 1 / height;
            _orthoMatrix[2, 2] = 1 / depth;

            // generate bounding box for decal for culling purposes
            _decalBounds.center = Vector3.forward * (depth / 2);
            _decalBounds.extents = new Vector3(width / 2, height / 2, depth / 2);

            // rescale preview model
            decalModelTransformRef.localScale = new Vector3(width, height, (width + height) / 2);

            // assign dimensions to collider
            colliderRef.center = _decalBounds.center;
            colliderRef.size = _decalBounds.size;

            // project to each target object
            foreach (var target in _targets) {
                target.Project(_orthoMatrix, colliderRef.bounds, decalProjectorTransformRef);
            }
        }

        public void Render(Camera camera) {
            if (!IsAttached) return;

            // render on each target object
            foreach (var target in _targets) {
                target.Render(materialProperties.parsedMaterial, part.mpb, camera);
            }
        }
    }
}