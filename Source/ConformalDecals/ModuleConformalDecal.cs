using System;
using System.Collections.Generic;
using ConformalDecals.MaterialModifiers;
using UniLinq;
using UnityEngine;

namespace ConformalDecals {
    public class ModuleConformalDecal : PartModule {
        [KSPField] public string decalPreviewTransform = "";
        [KSPField] public string decalModelTransform   = "";

        [KSPField(guiName = "Scale", guiActive = false, guiActiveEditor = true, isPersistant = true, guiFormat = "F2", guiUnits = "m"),
         UI_FloatRange(minValue = 0.05f, maxValue = 4f, stepIncrement = 0.05f)]
        public float scale = 1.0f;

        [KSPField(guiName = "Depth", guiActive = false, guiActiveEditor = true, isPersistant = true, guiFormat = "F2", guiUnits = "m"),
         UI_FloatRange(minValue = 0.05f, maxValue = 4f, stepIncrement = 0.05f)]
        public float depth = 1.0f;

        [KSPField(guiActive = true, guiFormat = "F2", guiName = "Aspect Ratio")]
        public float aspectRatio = 1.0f;

        private List<ProjectionTarget>     _targets;
        private MaterialPropertyCollection _materialProperties;
        private Material                   _material;

        private Matrix4x4 _orthoMatrix;
        private Bounds    _decalBounds;

        private Transform _decalPreviewTransform;
        private Transform _decalModelTransform;

        private bool IsAttached => part.parent != null;

        public override void OnLoad(ConfigNode node) {
            if (HighLogic.LoadedSceneIsGame) {
                try {
                    var materialNode = node.GetNode("MATERIAL") ?? throw new FormatException("Missing MATERIAL node in module");
                    _materialProperties = new MaterialPropertyCollection(materialNode, this);
                    _material = _materialProperties.ParsedMaterial;

                    var mainTexture = _materialProperties.MainTextureProperty;
                    if (mainTexture != null) {
                        aspectRatio = mainTexture.AspectRatio;
                    }
                    else {
                        aspectRatio = 1;
                    }

                    _decalPreviewTransform = part.FindModelTransform(decalPreviewTransform);
                    if (_decalPreviewTransform == null) throw new FormatException("Missing decal preview reference");

                    _decalModelTransform = part.FindModelTransform(decalModelTransform);
                    if (_decalModelTransform == null) throw new FormatException("Missing decal mesh reference");
                }
                catch (Exception e) {
                    this.LogException("Exception parsing partmodule", e);
                }
            }
        }

        public override void OnStart(StartState state) {
            _orthoMatrix = Matrix4x4.identity;
            _orthoMatrix[0, 3] = 0.5f;
            _orthoMatrix[1, 3] = 0.5f;

            if ((state & StartState.Editor) != 0) {
                GameEvents.onEditorPartEvent.Add(OnEditorEvent);
                Fields[nameof(scale)].uiControlEditor.onFieldChanged = OnTweakEvent;
                Fields[nameof(depth)].uiControlEditor.onFieldChanged = OnTweakEvent;
            }
            else {
                if (IsAttached) Attach();
            }
        }

        public void OnDisable() {
            // remove from preCull delegate
            Camera.onPreCull -= Render;
        }

        public void OnTweakEvent(BaseField field, object obj) {
            // scale or depth values have been changed, so update the projection matrix for each target
            Project();
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
            if (!IsAttached) {
                this.LogError("Attach function called but part has no parent!");
                return;
            }

            // find all valid renderers
            var renderers = part.parent.transform.GetComponentsInChildren<MeshRenderer>(false).Where(o => o.GetComponent<MeshFilter>() != null);
            // generate ProjectionTarget objects for each valid meshrenderer
            _targets = renderers.Select(o => new ProjectionTarget(o, o.GetComponent<MeshFilter>().mesh, _materialProperties)).ToList();

            // hide preview model
            _decalModelTransform.gameObject.SetActive(false);

            // add to preCull delegate
            Camera.onPreCull += Render;
        }

        public void Detach() {
            if (IsAttached) {
                this.LogError("Detach function called but part still has parent!");
                return;
            }

            // unhide preview model
            _decalModelTransform.gameObject.SetActive(true);

            // remove from preCull delegate
            Camera.onPreCull -= Render;
        }

        [KSPEvent(guiActive = false, guiName = "Project", guiActiveEditor =true, active = true)]
        public void Project() {
            if (!IsAttached) return;

            // generate orthogonal matrix scale values
            _orthoMatrix[0, 0] = 1 / scale;
            _orthoMatrix[1, 1] = 1 / (aspectRatio * scale);
            _orthoMatrix[2, 2] = 1 / depth;

            // generate bounding box for decal for culling purposes
            _decalBounds.center = Vector3.forward * (depth / 2);
            _decalBounds.extents = new Vector3(scale / 2, aspectRatio * scale / 2, depth / 2);

            // project to each target object
            foreach (var target in _targets) {
                target.Project(_orthoMatrix, _decalBounds);
            }
        }

        public void Render(Camera camera) {
            if (!IsAttached) return;

            // render on each target object
            foreach (var target in _targets) {
                target.Render(_material, part.mpb, camera);
            }
        }
    }
}