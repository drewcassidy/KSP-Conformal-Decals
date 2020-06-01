using System;
using System.Collections.Generic;
using ConformalDecals.MaterialModifiers;
using UnityEngine;

namespace ConformalDecals {
    public class ModuleConformalDecal : PartModule {
        [KSPField(guiName = "#LOC_ConformalDecals_gui-scale", guiActive = false, guiActiveEditor = true, isPersistant = true, guiFormat = "F2", guiUnits = "m"),
         UI_FloatRange(minValue = 0.05f, maxValue = 4f, stepIncrement = 0.05f)]
        public float scale = 1.0f;

        [KSPField(guiName = "#LOC_ConformalDecals_gui-depth", guiActive = false, guiActiveEditor = true, isPersistant = true, guiFormat = "F2", guiUnits = "m"),
         UI_FloatRange(minValue = 0.05f, maxValue = 4f, stepIncrement = 0.05f)]
        public float depth = 1.0f;

        [KSPField(guiName = "#LOC_ConformalDecals_gui-opacity", guiActive = false, guiActiveEditor = true, isPersistant = true, guiFormat = "F2", guiUnits = "m"),
         UI_FloatRange(minValue = 0.05f, maxValue = 4f, stepIncrement = 0.05f)]
        public float opacity = 1.0f;

        [KSPField(guiName = "#LOC_ConformalDecals_gui-cutoff", guiActive = false, guiActiveEditor = true, isPersistant = true, guiFormat = "F2", guiUnits = "m"),
         UI_FloatRange(minValue = 0.05f, maxValue = 4f, stepIncrement = 0.05f)]
        public float cutoff = 0.5f;

        [KSPField(guiName = "#LOC_ConformalDecals_gui-aspectratio", guiActive = true, guiFormat = "F2")]
        public float aspectRatio = 1.0f;

        [KSPField] public string decalFront     = string.Empty;
        [KSPField] public string decalBack      = string.Empty;
        [KSPField] public string decalModel     = string.Empty;
        [KSPField] public string decalProjector = string.Empty;

        [KSPField] public Transform decalFrontTransform;
        [KSPField] public Transform decalBackTransform;
        [KSPField] public Transform decalModelTransform;
        [KSPField] public Transform decalProjectorTransform;

        [KSPField] public bool adjustableScale   = true;
        [KSPField] public bool adjustableDepth   = true;
        [KSPField] public bool adjustableOpacity = true;
        [KSPField] public bool adjustableCutoff  = true;

        [KSPField] public bool updateBackScale = true;

        [KSPField] public MaterialPropertyCollection materialProperties;

        private List<ProjectionTarget> _targets;

        private bool _isAttached;

        private Matrix4x4 _orthoMatrix;
        private Bounds    _decalBounds;
        private Vector2   _backTextureBaseScale;
        private Material  _backMaterial;

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

                // find front transform
                decalFrontTransform = part.FindModelTransform(decalFront);
                if (decalFrontTransform == null) throw new FormatException($"Could not find decalFront transform: '{decalFront}'.");

                // find back transform
                this.Log($"decalBack name is {decalBack}");
                this.Log($"updateBaseScale is {updateBackScale}");
                if (string.IsNullOrEmpty(decalBack)) {
                    if (updateBackScale) {
                        this.LogWarning("updateBackScale is true but has no specified decalBack transform!");
                        this.LogWarning("Setting updateBackScale to false.");
                        updateBackScale = false;
                    }
                }
                else {
                    decalBackTransform = part.FindModelTransform(decalBack);
                    if (decalBackTransform == null) throw new FormatException($"Could not find decalBack transform: '{decalBack}'.");
                }

                // find model transform
                if (string.IsNullOrEmpty(decalModel)) {
                    decalModelTransform = decalFrontTransform;
                }
                else {
                    decalModelTransform = part.FindModelTransform(decalModel);
                    if (decalModelTransform == null) throw new FormatException($"Could not find decalModel transform: '{decalModel}'.");
                }

                // find projector transform
                if (string.IsNullOrEmpty(decalProjector)) {
                    decalProjectorTransform = part.transform.Find("model");
                }
                else {
                    decalProjectorTransform = part.FindModelTransform(decalProjector);
                    if (decalProjectorTransform == null) throw new FormatException($"Could not find decalProjector transform: '{decalProjector}'.");
                }
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

            // setup OnTweakEvent for scale and depth fields in editor
            if ((state & StartState.Editor) != 0) {
                GameEvents.onEditorPartEvent.Add(OnEditorEvent);
                GameEvents.onVariantApplied.Add(OnVariantApplied);
                Fields[nameof(scale)].uiControlEditor.onFieldChanged = OnScaleTweakEvent;
                Fields[nameof(depth)].uiControlEditor.onFieldChanged = OnScaleTweakEvent;
            }

            // get back material if necessary
            if (updateBackScale) {
                this.Log("Getting material and base scale for back material");
                var backRenderer = decalBackTransform.GetComponent<MeshRenderer>();
                if (backRenderer == null) {
                    this.LogError($"Specified decalBack transform {decalBack} has no renderer attached! Setting updateBackScale to false.");
                    updateBackScale = false;
                }
                else if ((_backMaterial = backRenderer.material) == null) {
                    this.LogError($"Specified decalBack transform {decalBack} has a renderer but no material! Setting updateBackScale to false.");
                    updateBackScale = false;
                }
                else {
                    _backTextureBaseScale = _backMaterial.GetTextureScale(PropertyIDs._MainTex);
                }
            }

            // set initial attachment state
            if (part.parent == null) {
                OnDetach();
            }
            else {
                OnAttach();
            }
        }

        public void OnDestroy() {
            GameEvents.onEditorPartEvent.Remove(OnEditorEvent);
            GameEvents.onVariantApplied.Remove(OnVariantApplied);

            // remove from preCull delegate
            Camera.onPreCull -= Render;
        }

        private void OnScaleTweakEvent(BaseField field, object obj) {
            // scale or depth values have been changed, so update scale
            // and update projection matrices if attached
            UpdateScale();
            if (_isAttached) UpdateProjection();
        }

        private void OnVariantApplied(Part eventPart, PartVariant variant) {
            if (_isAttached && eventPart == part.parent) {
                OnDetach();
                OnAttach();
            }
        }

        private void OnEditorEvent(ConstructionEventType eventType, Part eventPart) {
            if (eventPart != this.part) return;
            switch (eventType) {
                case ConstructionEventType.PartAttached:
                    OnAttach();
                    break;
                case ConstructionEventType.PartDetached:
                    OnDetach();
                    break;
                case ConstructionEventType.PartOffsetting:
                case ConstructionEventType.PartRotating:
                case ConstructionEventType.PartDragging:
                    UpdateProjection();
                    break;
            }
        }

        private void OnAttach() {
            if (part.parent == null) {
                this.LogError("Attach function called but part has no parent!");
                _isAttached = false;
                return;
            }

            _isAttached = true;

            this.Log($"Decal attached to {part.parent.partName}");

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
            decalModelTransform.gameObject.SetActive(false);

            // add to preCull delegate
            Camera.onPreCull += Render;

            UpdateScale();
            UpdateProjection();
        }

        private void OnDetach() {
            _isAttached = false;

            // unhide preview model
            decalModelTransform.gameObject.SetActive(true);

            // remove from preCull delegate
            Camera.onPreCull -= Render;

            UpdateScale();
        }

        private void UpdateScale() {
            var size = new Vector2(scale, scale * aspectRatio);

            // update orthogonal matrix scale
            _orthoMatrix[0, 0] = 1 / size.x;
            _orthoMatrix[1, 1] = 1 / size.y;
            _orthoMatrix[2, 2] = 1 / depth;

            // generate bounding box for decal for culling purposes
            _decalBounds.center = Vector3.forward * (depth / 2);
            _decalBounds.extents = new Vector3(size.x / 2, size.y / 2, depth / 2);

            // rescale preview model
            decalModelTransform.localScale = new Vector3(size.x, size.y, (size.x + size.y) / 2);

            // update back material scale
            if (updateBackScale) {
                _backMaterial.SetTextureScale(PropertyIDs._MainTex, new Vector2(size.x * _backTextureBaseScale.x, size.y * _backTextureBaseScale.y));
            }

            // update material scale
            materialProperties.UpdateMaterial(size);
        }

        private void UpdateProjection() {
            if (!_isAttached) return;
            // project to each target object
            foreach (var target in _targets) {
                target.Project(_orthoMatrix, new OrientedBounds(decalProjectorTransform.localToWorldMatrix, _decalBounds), decalProjectorTransform);
            }
        }

        private void Render(Camera camera) {
            if (!_isAttached) return;

            // render on each target object
            foreach (var target in _targets) {
                target.Render(materialProperties.parsedMaterial, part.mpb, camera);
            }
        }
    }
}