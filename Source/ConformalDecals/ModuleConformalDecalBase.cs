using System;
using System.Collections.Generic;
using ConformalDecals.MaterialModifiers;
using ConformalDecals.Util;
using UnityEngine;

namespace ConformalDecals {
    public abstract class ModuleConformalDecalBase : PartModule {
        [KSPField(guiName = "#LOC_ConformalDecals_gui-scale", guiActive = false, guiActiveEditor = true, isPersistant = true, guiFormat = "F2", guiUnits = "m"),
         UI_FloatRange(minValue = 0.05f, maxValue = 4f, stepIncrement = 0.05f)]
        public float scale = 1.0f;

        [KSPField(guiName = "#LOC_ConformalDecals_gui-depth", guiActive = false, guiActiveEditor = true, isPersistant = true, guiFormat = "F2", guiUnits = "m"),
         UI_FloatRange(minValue = 0.05f, maxValue = 4f, stepIncrement = 0.05f)]
        public float depth = 1.0f;

        [KSPField(guiName = "#LOC_ConformalDecals_gui-opacity", guiActive = false, guiActiveEditor = true, isPersistant = true, guiFormat = "P0"),
         UI_FloatRange(minValue = 0.0f, maxValue = 1f, stepIncrement = 0.05f)]
        public float opacity = 1.0f;

        [KSPField(guiName = "#LOC_ConformalDecals_gui-cutoff", guiActive = false, guiActiveEditor = true, isPersistant = true, guiFormat = "P0"),
         UI_FloatRange(minValue = 0.0f, maxValue = 1f, stepIncrement = 0.05f)]
        public float cutoff = 0.5f;

        [KSPField] public string decalFront     = string.Empty;
        [KSPField] public string decalBack      = string.Empty;
        [KSPField] public string decalModel     = string.Empty;
        [KSPField] public string decalProjector = string.Empty;
        [KSPField] public string decalShader    = "ConformalDecals/Paint/Diffuse";

        [KSPField] public Transform decalFrontTransform;
        [KSPField] public Transform decalBackTransform;
        [KSPField] public Transform decalModelTransform;
        [KSPField] public Transform decalProjectorTransform;

        [KSPField] public bool scaleAdjustable   = true;
        [KSPField] public bool depthAdjustable   = true;
        [KSPField] public bool opacityAdjustable = true;
        [KSPField] public bool cutoffAdjustable  = true;

        [KSPField] public Vector2 scaleRange      = new Vector2(0, 4);
        [KSPField] public Vector2 depthRange      = new Vector2(0, 4);
        [KSPField] public Vector2 opacityRange    = new Vector2(0, 1);
        [KSPField] public Vector2 cutoffRange     = new Vector2(0, 1);
        [KSPField] public Vector2 decalQueueRange = new Vector2(2100, 2400);

        [KSPField] public bool updateBackScale = true;
        [KSPField] public bool useBaseNormal   = true;

        [KSPField] public MaterialPropertyCollection materialProperties;

        [KSPField] public Material backMaterial;

        private static int _decalQueueCounter = -1;

        private List<ProjectionTarget> _targets;

        private bool      _isAttached;
        private Matrix4x4 _orthoMatrix;
        private Bounds    _decalBounds;
        private Vector2   _backTextureBaseScale;

        public ModuleConformalDecalBase() {
            decalBackTransform = null;
            decalModelTransform = null;
            decalProjectorTransform = null;
        }

        private int DecalQueue {
            get {
                _decalQueueCounter++;
                if (_decalQueueCounter > decalQueueRange.y || _decalQueueCounter < decalQueueRange.x) {
                    _decalQueueCounter = (int) decalQueueRange.x;
                }

                return _decalQueueCounter;
            }
        }

        public override void OnLoad(ConfigNode node) {
            this.Log("Loading module");
            try {
                if (HighLogic.LoadedSceneIsEditor) {
                    UpdateTweakables();
                }
                
                if (materialProperties == null) {
                    // materialProperties is null, so make a new one
                    materialProperties = ScriptableObject.CreateInstance<MaterialPropertyCollection>();
                    materialProperties.Initialize();
                }
                else {
                    // materialProperties already exists, so make a copy
                    materialProperties = ScriptableObject.Instantiate(materialProperties);
                }
                
                // set shader
                materialProperties.SetShader(decalShader);

                // get back material if necessary
                if (updateBackScale) {
                    this.Log("Getting material and base scale for back material");
                    var backRenderer = decalBackTransform.GetComponent<MeshRenderer>();
                    if (backRenderer == null) {
                        this.LogError($"Specified decalBack transform {decalBack} has no renderer attached! Setting updateBackScale to false.");
                        updateBackScale = false;
                    }
                    else if ((backMaterial = backRenderer.material) == null) {
                        this.LogError($"Specified decalBack transform {decalBack} has a renderer but no material! Setting updateBackScale to false.");
                        updateBackScale = false;
                    }
                    else {
                        _backTextureBaseScale = backMaterial.GetTextureScale(PropertyIDs._MainTex);
                    }
                }

                // find front transform
                decalFrontTransform = part.FindModelTransform(decalFront);
                if (decalFrontTransform == null) throw new FormatException($"Could not find decalFront transform: '{decalFront}'.");

                // find back transform
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
                    decalModelTransform = part.transform.Find("model");
                }
                else {
                    decalModelTransform = part.FindModelTransform(decalModel);
                    if (decalModelTransform == null) throw new FormatException($"Could not find decalModel transform: '{decalModel}'.");
                }

                // find projector transform
                if (string.IsNullOrEmpty(decalProjector)) {
                    decalProjectorTransform = part.transform;
                }
                else {
                    decalProjectorTransform = part.FindModelTransform(decalProjector);
                    if (decalProjectorTransform == null) throw new FormatException($"Could not find decalProjector transform: '{decalProjector}'.");
                }

                // update EVERYTHING if currently attached
                if (_isAttached) {
                    UpdateScale();
                    UpdateTargets();
                }
            }
            catch (Exception e) {
                this.LogException("Exception parsing partmodule", e);
            }
        }

        public override void OnStart(StartState state) {
            this.Log("Starting module");

            if (HighLogic.LoadedSceneIsEditor) {
                GameEvents.onEditorPartEvent.Add(OnEditorEvent);
                GameEvents.onVariantApplied.Add(OnVariantApplied);

                UpdateTweakables();
            }

            // generate orthogonal projection matrix and offset it by 0.5 on x and y axes
            _orthoMatrix = Matrix4x4.identity;
            _orthoMatrix[0, 3] = 0.5f;
            _orthoMatrix[1, 3] = 0.5f;

            // instantiate decal material and set uniqueish queue
            materialProperties.SetRenderQueue(DecalQueue);

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

        private void OnSizeTweakEvent(BaseField field, object obj) {
            // scale or depth values have been changed, so update scale
            // and update projection matrices if attached
            UpdateScale();
            if (_isAttached) UpdateProjection();
        }

        private void OnMaterialTweakEvent(BaseField field, object obj) {
            materialProperties.SetOpacity(opacity);
            materialProperties.SetCutoff(cutoff);
        }

        private void OnVariantApplied(Part eventPart, PartVariant variant) {
            if (_isAttached && eventPart == part.parent) {
                UpdateTargets();
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

            UpdateTargets();

            // hide preview model
            decalFrontTransform.gameObject.SetActive(false);
            decalBackTransform.gameObject.SetActive(false);

            // add to preCull delegate
            Camera.onPreCull += Render;

            UpdateScale();
            UpdateProjection();
        }

        private void OnDetach() {
            _isAttached = false;

            // unhide preview model
            decalFrontTransform.gameObject.SetActive(true);
            decalBackTransform.gameObject.SetActive(true);

            // remove from preCull delegate
            Camera.onPreCull -= Render;

            UpdateScale();
        }

        private void UpdateScale() {
            var size = new Vector2(scale, scale * materialProperties.AspectRatio);

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
                backMaterial.SetTextureScale(PropertyIDs._MainTex, new Vector2(size.x * _backTextureBaseScale.x, size.y * _backTextureBaseScale.y));
            }

            // update material scale
            materialProperties.SetScale(size);
        }

        private void UpdateProjection() {
            if (!_isAttached) return;

            // project to each target object
            foreach (var target in _targets) {
                target.Project(_orthoMatrix, new OrientedBounds(decalProjectorTransform.localToWorldMatrix, _decalBounds), decalProjectorTransform);
            }
        }

        private void UpdateTargets() {
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

                // skip blacklisted shaders
                if (ConformalDecalConfig.IsBlacklisted(renderer.material.shader)) {
                    this.Log($"Skipping blacklisted shader '{renderer.material.shader.name}' in '{renderer.gameObject.name}'");
                    continue;
                }

                var meshFilter = renderer.GetComponent<MeshFilter>();
                if (meshFilter == null) continue; // object has a meshRenderer with no filter, invalid
                var mesh = meshFilter.mesh;
                if (mesh == null) continue; // object has a null mesh, invalid

                this.Log($"Adding target for object {meshFilter.gameObject.name} with the mesh {mesh.name}");
                // create new ProjectionTarget to represent the renderer
                var target = new ProjectionTarget(renderer, mesh, useBaseNormal);

                this.Log("done.");

                // add the target to the list
                _targets.Add(target);
            }
        }

        private void UpdateTweakables() {
            // setup tweakable fields
            var scaleField = Fields[nameof(scale)];
            var depthField = Fields[nameof(depth)];
            var opacityField = Fields[nameof(opacity)];
            var cutoffField = Fields[nameof(cutoff)];

            scaleField.guiActiveEditor = scaleAdjustable;
            depthField.guiActiveEditor = depthAdjustable;
            opacityField.guiActiveEditor = opacityAdjustable;
            cutoffField.guiActiveEditor = cutoffAdjustable;

            if (scaleAdjustable) {
                var minValue = Mathf.Max(Mathf.Epsilon, scaleRange.x);
                var maxValue = Mathf.Max(minValue, scaleRange.y);

                ((UI_FloatRange) scaleField.uiControlEditor).minValue = minValue;
                ((UI_FloatRange) scaleField.uiControlEditor).maxValue = maxValue;
                scaleField.uiControlEditor.onFieldChanged = OnSizeTweakEvent;
            }

            if (depthAdjustable) {
                var minValue = Mathf.Max(Mathf.Epsilon, depthRange.x);
                var maxValue = Mathf.Max(minValue, depthRange.y);
                ((UI_FloatRange) depthField.uiControlEditor).minValue = minValue;
                ((UI_FloatRange) depthField.uiControlEditor).maxValue = maxValue;
                depthField.uiControlEditor.onFieldChanged = OnSizeTweakEvent;
            }

            if (opacityAdjustable) {
                var minValue = Mathf.Max(0, opacityRange.x);
                var maxValue = Mathf.Max(minValue, opacityRange.y);
                maxValue = Mathf.Min(1, maxValue);

                ((UI_FloatRange) opacityField.uiControlEditor).minValue = minValue;
                ((UI_FloatRange) opacityField.uiControlEditor).maxValue = maxValue;
                opacityField.uiControlEditor.onFieldChanged = OnMaterialTweakEvent;
            }

            if (cutoffAdjustable) {
                var minValue = Mathf.Max(0, cutoffRange.x);
                var maxValue = Mathf.Max(minValue, cutoffRange.y);
                maxValue = Mathf.Min(1, maxValue);

                ((UI_FloatRange) cutoffField.uiControlEditor).minValue = minValue;
                ((UI_FloatRange) cutoffField.uiControlEditor).maxValue = maxValue;
                cutoffField.uiControlEditor.onFieldChanged = OnMaterialTweakEvent;
            }
        }

        private void Render(Camera camera) {
            if (!_isAttached) return;

            // render on each target object
            foreach (var target in _targets) {
                target.Render(materialProperties.DecalMaterial, part.mpb, camera);
            }
        }
    }
}