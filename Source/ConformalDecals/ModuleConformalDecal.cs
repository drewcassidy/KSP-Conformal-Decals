using System;
using System.Collections.Generic;
using ConformalDecals.MaterialModifiers;
using ConformalDecals.Util;
using UnityEngine;

namespace ConformalDecals {
    public class ModuleConformalDecal : PartModule {
        public enum DecalScaleMode {
            HEIGHT,
            WIDTH,
            AVERAGE,
            AREA,
            MINIMUM,
            MAXIMUM
        }

        // CONFIGURABLE VALUES

        /// <summary>
        /// Shader name. Should be one that supports decal projection.
        /// </summary>
        [KSPField] public string shader = "ConformalDecals/Paint/Diffuse";

        /// <summary>
        /// Decal front transform name. Required
        /// </summary>
        [KSPField] public string decalFront = string.Empty;

        /// <summary>
        /// Decal back transform name. Required if <see cref="updateBackScale"/> is true.
        /// </summary>
        [KSPField] public string decalBack = string.Empty;

        /// <summary>
        /// Decal model transform name. Is rescaled to preview the decal scale when unattached.
        /// </summary>
        /// <remarks>
        /// If unspecified, the decal front transform is used instead.
        /// </remarks>
        [KSPField] public string decalModel = string.Empty;

        /// <summary>
        /// Decal projector transform name. The decal will project along the +Z axis of this transform.
        /// </summary>
        /// <remarks>
        /// if unspecified, the part "model" transform will be used instead.
        /// </remarks>
        [KSPField] public string decalProjector = string.Empty;

        // Parameters

        [KSPField] public bool           scaleAdjustable = true;
        [KSPField] public float          defaultScale    = 1;
        [KSPField] public Vector2        scaleRange      = new Vector2(0, 4);
        [KSPField] public DecalScaleMode scaleMode       = DecalScaleMode.HEIGHT;

        [KSPField] public bool    depthAdjustable = true;
        [KSPField] public float   defaultDepth    = 0.1f;
        [KSPField] public Vector2 depthRange      = new Vector2(0, 2);

        [KSPField] public bool    opacityAdjustable = true;
        [KSPField] public float   defaultOpacity    = 1;
        [KSPField] public Vector2 opacityRange      = new Vector2(0, 1);

        [KSPField] public bool    cutoffAdjustable = true;
        [KSPField] public float   defaultCutoff    = 0.5f;
        [KSPField] public Vector2 cutoffRange      = new Vector2(0, 1);

        [KSPField] public bool    useBaseNormal = true;
        [KSPField] public float   defaultWear   = 100;
        [KSPField] public Vector2 wearRange     = new Vector2(0, 100);

        [KSPField] public Rect    tileRect = new Rect(-1, -1, 0, 0);
        [KSPField] public Vector2 tileSize;
        [KSPField] public int     tileIndex = -1;

        /// <summary>
        /// Should the back material scale be updated automatically?
        /// </summary>
        [KSPField] public bool updateBackScale = true;


        // INTERNAL VALUES

        /// <summary>
        /// Decal scale factor, in meters.
        /// </summary>
        [KSPField(guiName = "#LOC_ConformalDecals_gui-scale", guiActive = false, guiActiveEditor = true, isPersistant = true, guiFormat = "F2", guiUnits = "m"),
         UI_FloatRange(stepIncrement = 0.05f)]
        public float scale = 1.0f;

        /// <summary>
        /// Projection depth value for the decal projector, in meters.
        /// </summary>
        [KSPField(guiName = "#LOC_ConformalDecals_gui-depth", guiActive = false, guiActiveEditor = true, isPersistant = true, guiFormat = "F2", guiUnits = "m"),
         UI_FloatRange(stepIncrement = 0.02f)]
        public float depth = 0.2f;

        /// <summary>
        /// Opacity value for the decal shader.
        /// </summary>
        [KSPField(guiName = "#LOC_ConformalDecals_gui-opacity", guiActive = false, guiActiveEditor = true, isPersistant = true, guiFormat = "P0"),
         UI_FloatRange(stepIncrement = 0.05f)]
        public float opacity = 1.0f;

        /// <summary>
        /// Alpha cutoff value for the decal shader.
        /// </summary>
        [KSPField(guiName = "#LOC_ConformalDecals_gui-cutoff", guiActive = false, guiActiveEditor = true, isPersistant = true, guiFormat = "P0"),
         UI_FloatRange(stepIncrement = 0.05f)]
        public float cutoff = 0.5f;

        /// <summary>
        /// Edge wear value for the decal shader. Only relevent when useBaseNormal is true and the shader is a paint shader
        /// </summary>
        [KSPField(guiName = "#LOC_ConformalDecals_gui-wear", guiActive = false, guiActiveEditor = true, isPersistant = true, guiFormat = "F0"),
         UI_FloatRange()]
        public float wear = 100;

        [KSPField] public MaterialPropertyCollection materialProperties;

        [KSPField] public Transform decalFrontTransform;
        [KSPField] public Transform decalBackTransform;
        [KSPField] public Transform decalModelTransform;
        [KSPField] public Transform decalProjectorTransform;

        [KSPField] public Material backMaterial;
        [KSPField] public Vector2  backTextureBaseScale;

        private const  int DecalQueueMin      = 2100;
        private const  int DecalQueueMax      = 2400;
        private static int _decalQueueCounter = -1;

        private List<ProjectionTarget> _targets;

        private bool      _isAttached;
        private Matrix4x4 _orthoMatrix;

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

        /// <inheritdoc />
        public override void OnAwake() {
            base.OnAwake();

            if (materialProperties == null) {
                materialProperties = ScriptableObject.CreateInstance<MaterialPropertyCollection>();
            }
            else {
                materialProperties = ScriptableObject.Instantiate(materialProperties);
            }
        }

        /// <inheritdoc />
        public override void OnLoad(ConfigNode node) {
            this.Log("Loading module");
            try {
                // SETUP TRANSFORMS

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

                // PARSE MATERIAL PROPERTIES

                // set shader
                materialProperties.SetShader(shader);

                // add texture nodes
                foreach (var textureNode in node.GetNodes("TEXTURE")) {
                    materialProperties.ParseProperty<MaterialTextureProperty>(textureNode);
                }

                // add float nodes
                foreach (var floatNode in node.GetNodes("FLOAT")) {
                    materialProperties.ParseProperty<MaterialTextureProperty>(floatNode);
                }

                // add color nodes
                foreach (var colorNode in node.GetNodes("COLOR")) {
                    materialProperties.ParseProperty<MaterialColorProperty>(colorNode);
                }

                // handle texture tiling parameters
                var tileString = node.GetValue("tile");
                this.Log(tileString);
                if (!string.IsNullOrEmpty(tileString)) {
                    var tileValid = ParseExtensions.TryParseRect(tileString, out tileRect);
                    if (!tileValid) throw new FormatException($"Invalid rect value for tile '{tileString}'");
                }

                if (tileRect.x >= 0) {
                    materialProperties.UpdateTile(tileRect);
                }
                else if (tileIndex >= 0) {
                    materialProperties.UpdateTile(tileIndex, tileSize);
                }

                // QUEUE PART FOR ICON FIXING IN VAB
                DecalIconFixer.QueuePart(part.name);
            }
            catch (Exception e) {
                this.LogException("Exception parsing partmodule", e);
            }

            UpdateMaterials();

            if (HighLogic.LoadedSceneIsEditor) {
                UpdateTweakables();
            }

            if (HighLogic.LoadedSceneIsGame) {
                UpdateScale();
            }
            else {
                scale = defaultScale;
                depth = defaultDepth;
                opacity = defaultOpacity;
                cutoff = defaultCutoff;
                wear = defaultWear;
            }
        }

        /// <inheritdoc />
        public override void OnIconCreate() {
            UpdateScale();
        }

        /// <inheritdoc />
        public override void OnStart(StartState state) {
            this.Log("Starting module");

            // handle tweakables
            if (HighLogic.LoadedSceneIsEditor) {
                GameEvents.onEditorPartEvent.Add(OnEditorEvent);
                GameEvents.onVariantApplied.Add(OnVariantApplied);

                UpdateTweakables();
            }

            materialProperties.RenderQueue = DecalQueue;

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

            foreach (var counterpart in part.symmetryCounterparts) {
                var decal = counterpart.GetComponent<ModuleConformalDecal>();
                decal.UpdateScale();
            }
        }

        protected void OnMaterialTweakEvent(BaseField field, object obj) {
            materialProperties.SetOpacity(opacity);
            materialProperties.SetCutoff(cutoff);
            if (useBaseNormal) {
                materialProperties.SetWear(wear);
            }

            foreach (var counterpart in part.symmetryCounterparts) {
                var decal = counterpart.GetComponent<ModuleConformalDecal>();
                decal.materialProperties.SetOpacity(opacity);
                decal.materialProperties.SetCutoff(cutoff);
                if (useBaseNormal) {
                    decal.materialProperties.SetWear(wear);
                }
            }
        }

        protected void OnVariantApplied(Part eventPart, PartVariant variant) {
            if (_isAttached && eventPart == part.parent) {
                UpdateTargets();
            }
        }

        protected void OnEditorEvent(ConstructionEventType eventType, Part eventPart) {
            if (this.part != eventPart && !part.symmetryCounterparts.Contains(eventPart)) return;
            switch (eventType) {
                case ConstructionEventType.PartAttached:
                    OnAttach();
                    break;
                case ConstructionEventType.PartDetached:
                    OnDetach();
                    break;
                case ConstructionEventType.PartOffsetting:
                case ConstructionEventType.PartRotating:
                    UpdateScale();
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

            UpdateMaterials();
            UpdateTargets();
            UpdateScale();
        }

        protected void OnDetach() {
            _isAttached = false;

            // unhide preview model
            decalFrontTransform.gameObject.SetActive(true);
            decalBackTransform.gameObject.SetActive(true);

            // remove from preCull delegate
            Camera.onPreCull -= Render;

            UpdateMaterials();
            UpdateScale();
        }

        protected void UpdateScale() {
            var aspectRatio = materialProperties.AspectRatio;
            Vector2 size;
            
            switch (scaleMode) {
                default:
                case DecalScaleMode.HEIGHT:
                    size = new Vector2(scale, scale * aspectRatio);
                    break;
                case DecalScaleMode.WIDTH:
                    size = new Vector2(scale / aspectRatio, scale);
                    break;
                case DecalScaleMode.AVERAGE:
                    var width1 = 2 * scale / (1 + aspectRatio);
                    size = new Vector2(width1, width1 * aspectRatio);
                    break;
                case DecalScaleMode.AREA:
                    var width2 = Mathf.Sqrt(scale / aspectRatio);
                    size = new Vector2(width2, width2 * aspectRatio);
                    break;
                case DecalScaleMode.MINIMUM:
                    if (aspectRatio > 1) goto case DecalScaleMode.WIDTH;
                    else goto case DecalScaleMode.HEIGHT;
                case DecalScaleMode.MAXIMUM:
                    if (aspectRatio > 1) goto case DecalScaleMode.HEIGHT;
                    else goto case DecalScaleMode.WIDTH;
            }

            // update material scale
            materialProperties.UpdateScale(size);

            if (_isAttached) {
                // update orthogonal matrix
                _orthoMatrix = Matrix4x4.identity;
                _orthoMatrix[0, 3] = 0.5f;
                _orthoMatrix[1, 3] = 0.5f;

                _orthoMatrix[0, 0] = 1 / size.x;
                _orthoMatrix[1, 1] = 1 / size.y;
                _orthoMatrix[2, 2] = 1 / depth;

                // update projection
                foreach (var target in _targets) {
                    target.Project(_orthoMatrix, decalProjectorTransform, useBaseNormal);
                }
            }
            else {
                // rescale preview model
                decalModelTransform.localScale = new Vector3(size.x, size.y, (size.x + size.y) / 2);

                // update back material scale
                if (updateBackScale) {
                    backMaterial.SetTextureScale(PropertyIDs._MainTex, new Vector2(size.x * backTextureBaseScale.x, size.y * backTextureBaseScale.y));
                }
            }
        }

        protected void UpdateMaterials() {
            materialProperties.UpdateMaterials();
            materialProperties.SetOpacity(opacity);
            materialProperties.SetCutoff(cutoff);
            if (useBaseNormal) {
                materialProperties.SetWear(wear);
            }

            _decalMaterial = materialProperties.DecalMaterial;
            _previewMaterial = materialProperties.PreviewMaterial;

            decalFrontTransform.GetComponent<MeshRenderer>().material = _previewMaterial;
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
            var wearField = Fields[nameof(wear)];

            scaleField.guiActiveEditor = scaleAdjustable;
            depthField.guiActiveEditor = depthAdjustable;
            opacityField.guiActiveEditor = opacityAdjustable;
            cutoffField.guiActiveEditor = cutoffAdjustable;
            wearField.guiActiveEditor = useBaseNormal;

            var steps = 20;

            if (scaleAdjustable) {
                var minValue = Mathf.Max(Mathf.Epsilon, scaleRange.x);
                var maxValue = Mathf.Max(minValue, scaleRange.y);

                var scaleEditor = (UI_FloatRange) scaleField.uiControlEditor;
                scaleEditor.minValue = minValue;
                scaleEditor.maxValue = maxValue;
                scaleEditor.stepIncrement = (maxValue - minValue) / steps;
                scaleEditor.onFieldChanged = OnSizeTweakEvent;
            }

            if (depthAdjustable) {
                var minValue = Mathf.Max(Mathf.Epsilon, depthRange.x);
                var maxValue = Mathf.Max(minValue, depthRange.y);

                var depthEditor = (UI_FloatRange) depthField.uiControlEditor;
                depthEditor.minValue = minValue;
                depthEditor.maxValue = maxValue;
                depthEditor.stepIncrement = (maxValue - minValue) / steps;
                depthEditor.onFieldChanged = OnSizeTweakEvent;
            }

            if (opacityAdjustable) {
                var minValue = Mathf.Max(0, opacityRange.x);
                var maxValue = Mathf.Max(minValue, opacityRange.y);
                maxValue = Mathf.Min(1, maxValue);

                var opacityEditor = (UI_FloatRange) opacityField.uiControlEditor;
                opacityEditor.minValue = minValue;
                opacityEditor.maxValue = maxValue;
                opacityEditor.stepIncrement = (maxValue - minValue) / steps;
                opacityEditor.onFieldChanged = OnMaterialTweakEvent;
            }

            if (cutoffAdjustable) {
                var minValue = Mathf.Max(0, cutoffRange.x);
                var maxValue = Mathf.Max(minValue, cutoffRange.y);
                maxValue = Mathf.Min(1, maxValue);

                var cutoffEditor = (UI_FloatRange) cutoffField.uiControlEditor;
                cutoffEditor.minValue = minValue;
                cutoffEditor.maxValue = maxValue;
                cutoffEditor.stepIncrement = (maxValue - minValue) / steps;
                cutoffEditor.onFieldChanged = OnMaterialTweakEvent;
            }

            if (useBaseNormal) {
                var minValue = Mathf.Max(0, wearRange.x);
                var maxValue = Mathf.Max(minValue, wearRange.y);

                var wearEditor = (UI_FloatRange) wearField.uiControlEditor;
                wearEditor.minValue = minValue;
                wearEditor.maxValue = maxValue;
                wearEditor.stepIncrement = (maxValue - minValue) / steps;
                wearEditor.onFieldChanged = OnMaterialTweakEvent;
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