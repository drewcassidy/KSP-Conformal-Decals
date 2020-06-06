using System;
using System.Collections.Generic;
using ConformalDecals.MaterialModifiers;
using ConformalDecals.Util;
using UnityEngine;

namespace ConformalDecals {
    public abstract class ModuleConformalDecalBase : PartModule {
        [KSPField(guiName = "#LOC_ConformalDecals_gui-scale", guiActive = false, guiActiveEditor = true, isPersistant = true, guiFormat = "F2", guiUnits = "m"),
         UI_FloatRange(stepIncrement = 0.05f)]
        public float scale = 1.0f;

        [KSPField(guiName = "#LOC_ConformalDecals_gui-depth", guiActive = false, guiActiveEditor = true, isPersistant = true, guiFormat = "F2", guiUnits = "m"),
         UI_FloatRange(stepIncrement = 0.02f)]
        public float depth = 0.2f;

        [KSPField(guiName = "#LOC_ConformalDecals_gui-opacity", guiActive = false, guiActiveEditor = true, isPersistant = true, guiFormat = "P0"),
         UI_FloatRange(stepIncrement = 0.05f)]
        public float opacity = 1.0f;

        [KSPField(guiName = "#LOC_ConformalDecals_gui-cutoff", guiActive = false, guiActiveEditor = true, isPersistant = true, guiFormat = "P0"),
         UI_FloatRange(stepIncrement = 0.05f)]
        public float cutoff = 0.5f;

        [KSPField] public string shader = "ConformalDecals/Paint/Diffuse";

        [KSPField] public string decalFront     = string.Empty;
        [KSPField] public string decalBack      = string.Empty;
        [KSPField] public string decalModel     = string.Empty;
        [KSPField] public string decalProjector = string.Empty;

        [KSPField] public Transform decalFrontTransform;
        [KSPField] public Transform decalBackTransform;
        [KSPField] public Transform decalModelTransform;
        [KSPField] public Transform decalProjectorTransform;

        [KSPField] public bool scaleAdjustable   = true;
        [KSPField] public bool depthAdjustable   = true;
        [KSPField] public bool opacityAdjustable = true;
        [KSPField] public bool cutoffAdjustable  = true;

        [KSPField] public Vector2 scaleRange   = new Vector2(0, 4);
        [KSPField] public Vector2 depthRange   = new Vector2(0, 2);
        [KSPField] public Vector2 opacityRange = new Vector2(0, 1);
        [KSPField] public Vector2 cutoffRange  = new Vector2(0, 1);

        [KSPField] public bool updateBackScale = true;
        [KSPField] public bool useBaseNormal   = true;

        [KSPField] public MaterialPropertyCollection materialProperties;

        [KSPField] public Material backMaterial;
        [KSPField] public Vector2  backTextureBaseScale;

        private const  int DecalQueueMin      = 2100;
        private const  int DecalQueueMax      = 2400;
        private static int _decalQueueCounter = -1;

        private List<ProjectionTarget> _targets;

        private bool      _isAttached;
        private Matrix4x4 _orthoMatrix;
        private Bounds    _decalBounds;

        private Material _decalMaterial;
        private Material _previewMaterial;


        private int DecalQueue {
            get {
                _decalQueueCounter++;
                if (_decalQueueCounter > DecalQueueMax || _decalQueueCounter < DecalQueueMin) {
                    _decalQueueCounter = DecalQueueMin;
                }

                return _decalQueueCounter;
            }
        }

        public override void OnLoad(ConfigNode node) {
            this.Log("Loading module");
            try {
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
                    decalModelTransform = decalFrontTransform;
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

                // get back material if necessary
                if (updateBackScale) {
                    this.Log("Getting material and base scale for back material");
                    var backRenderer = decalBackTransform.GetComponent<MeshRenderer>();
                    if (backRenderer == null) {
                        this.LogError($"Specified decalBack transform {decalBack} has no renderer attached! Setting updateBackScale to false.");
                        updateBackScale = false;
                    }
                    else {
                        backMaterial = backRenderer.material;
                        if (backMaterial == null) {
                            this.LogError($"Specified decalBack transform {decalBack} has a renderer but no material! Setting updateBackScale to false.");
                            updateBackScale = false;
                        }
                        else {
                            if (backTextureBaseScale == default) backTextureBaseScale = backMaterial.GetTextureScale(PropertyIDs._MainTex);
                        }
                    }
                }
                
                DecalIconFixer.QueuePart(part.name);
                
                // set shader
                materialProperties.SetShader(shader);
            }
            catch (Exception e) {
                this.LogException("Exception parsing partmodule", e);
            }
            
            if (HighLogic.LoadedSceneIsGame) {
                UpdateMaterials();
                UpdateScale();
                UpdateProjection();
            }
            else {
                UpdateScale();
            }
        }

        public override void OnAwake() {
            base.OnAwake();

            if (materialProperties == null) {
                materialProperties = ScriptableObject.CreateInstance<MaterialPropertyCollection>();
            }
            else {
                materialProperties = ScriptableObject.Instantiate(materialProperties);
            }
        }

        public override void OnStart(StartState state) {
            this.Log("Starting module");

            // handle tweakables
            if (HighLogic.LoadedSceneIsEditor) {
                GameEvents.onEditorPartEvent.Add(OnEditorEvent);
                GameEvents.onVariantApplied.Add(OnVariantApplied);

                UpdateTweakables();
            }
            
            materialProperties.SetRenderQueue(DecalQueue);

            UpdateMaterials();
            
            if (HighLogic.LoadedSceneIsGame) {
                // set initial attachment state
                if (part.parent == null) {
                    OnDetach();
                }
                else {
                    OnAttach();
                }
            }
        }

        public virtual void OnDestroy() {
            // remove GameEvents
            GameEvents.onEditorPartEvent.Remove(OnEditorEvent);
            GameEvents.onVariantApplied.Remove(OnVariantApplied);

            // remove from preCull delegate
            Camera.onPreCull -= Render;

            // destroy material properties object
            Destroy(materialProperties);
        }

        protected void OnSizeTweakEvent(BaseField field, object obj) {
            // scale or depth values have been changed, so update scale
            // and update projection matrices if attached
            UpdateScale();
            if (_isAttached) UpdateProjection();
        }

        protected void OnMaterialTweakEvent(BaseField field, object obj) {
            materialProperties.SetOpacity(opacity);
            materialProperties.SetCutoff(cutoff);
        }

        protected void OnVariantApplied(Part eventPart, PartVariant variant) {
            if (_isAttached && eventPart == part.parent) {
                UpdateTargets();
            }
        }

        protected void OnEditorEvent(ConstructionEventType eventType, Part eventPart) {
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

        protected void OnAttach() {
            if (part.parent == null) {
                this.LogError("Attach function called but part has no parent!");
                _isAttached = false;
                return;
            }

            _isAttached = true;

            this.Log($"Decal attached to {part.parent.partName}");


            // hide preview model
            decalFrontTransform.gameObject.SetActive(false);
            decalBackTransform.gameObject.SetActive(false);

            // add to preCull delegate
            Camera.onPreCull += Render;

            UpdateScale();
            UpdateTargets();
            UpdateProjection();
        }

        protected void OnDetach() {
            _isAttached = false;

            // unhide preview model
            decalFrontTransform.gameObject.SetActive(true);
            decalBackTransform.gameObject.SetActive(true);

            // remove from preCull delegate
            Camera.onPreCull -= Render;

            UpdateScale();
        }

        protected void UpdateScale() {
            var aspectRatio = materialProperties.AspectRatio;
            this.Log($"Aspect ratio is {aspectRatio}");
            var size = new Vector2(scale, scale * aspectRatio);

            // update orthogonal matrix scale
            _orthoMatrix = Matrix4x4.identity;
            _orthoMatrix[0, 3] = 0.5f;
            _orthoMatrix[1, 3] = 0.5f;

            _orthoMatrix[0, 0] = 1 / size.x;
            _orthoMatrix[1, 1] = 1 / size.y;
            _orthoMatrix[2, 2] = 1 / depth;

            // generate bounding box for decal for culling purposes
            _decalBounds.center = Vector3.forward * (depth / 2);
            _decalBounds.extents = new Vector3(size.x / 2, size.y / 2, depth / 2);

            if (decalModelTransform == null) {
                this.LogError("decalModelTransform is null!");
            }

            // rescale preview model
            decalModelTransform.localScale = new Vector3(size.x, size.y, (size.x + size.y) / 2);

            // update back material scale
            if (updateBackScale) {
                backMaterial.SetTextureScale(PropertyIDs._MainTex, new Vector2(size.x * backTextureBaseScale.x, size.y * backTextureBaseScale.y));
            }

            // update material scale
            materialProperties.UpdateScale(size);
        }

        protected void UpdateMaterials() {
            materialProperties.UpdateMaterials();
            materialProperties.SetOpacity(opacity);
            materialProperties.SetCutoff(cutoff);
            
            _decalMaterial = materialProperties.DecalMaterial;
            _previewMaterial = materialProperties.PreviewMaterial;

            if (decalFrontTransform == null) {
                this.LogError("No reference to decal front transform");
                return;
            }

            decalFrontTransform.GetComponent<MeshRenderer>().material = _previewMaterial;
        }

        protected void UpdateProjection() {
            if (!_isAttached) return;

            var bounds = new OrientedBounds(decalProjectorTransform.localToWorldMatrix, _decalBounds);

            // project to each target object
            foreach (var target in _targets) {
                target.Project(_orthoMatrix, bounds, decalProjectorTransform, useBaseNormal);
            }
        }

        protected void UpdateTargets() {
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
                if (DecalConfig.IsBlacklisted(renderer.material.shader)) continue;

                var meshFilter = renderer.GetComponent<MeshFilter>();
                if (meshFilter == null) continue; // object has a meshRenderer with no filter, invalid
                var mesh = meshFilter.mesh;
                if (mesh == null) continue; // object has a null mesh, invalid

                // create new ProjectionTarget to represent the renderer
                var target = new ProjectionTarget(renderer, mesh);

                // add the target to the list
                _targets.Add(target);
            }
        }

        protected void UpdateTweakables() {
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

        protected void Render(Camera camera) {
            if (!_isAttached) return;

            // render on each target object
            foreach (var target in _targets) {
                target.Render(_decalMaterial, part.mpb, camera);
            }
        }
    }
}