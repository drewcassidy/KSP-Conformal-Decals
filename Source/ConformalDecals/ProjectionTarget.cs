using System;
using ConformalDecals.MaterialModifiers;
using UnityEngine;
using UnityEngine.Rendering;

namespace ConformalDecals {
    public class ProjectionTarget {
        private static readonly int _projectionMatrixID = Shader.PropertyToID("_ProjectionMatrix");
        private static readonly int _decalNormalID      = Shader.PropertyToID("_DecalNormal");
        private static readonly int _decalTangentID     = Shader.PropertyToID("_DecalTangent");

        // Target object data
        public readonly Transform target;

        private readonly Renderer _targetRenderer;
        private readonly Mesh     _targetMesh;
        private          bool     _projectionEnabled;

        // property block
        public readonly MaterialPropertyBlock decalMPB;

        public ProjectionTarget(MeshRenderer targetRenderer, Mesh targetMesh, MaterialPropertyCollection properties) {
            target = targetRenderer.transform;
            _targetRenderer = targetRenderer;
            _targetMesh = targetMesh;
            var targetMaterial = targetRenderer.sharedMaterial;

            decalMPB = new MaterialPropertyBlock();

            if (properties.UseBaseNormal) {
                var normalSrcID = Shader.PropertyToID(properties.BaseNormalSrc);
                var normalDestID = Shader.PropertyToID(properties.BaseNormalDest);
                var normalDestIDST = Shader.PropertyToID(properties.BaseNormalDest + "_ST");

                var normal = targetMaterial.GetTexture(normalSrcID);
                if (normal != null) {

                    decalMPB.SetTexture(normalDestID, targetMaterial.GetTexture(normalSrcID));

                    var normalScale = targetMaterial.GetTextureScale(normalSrcID);
                    var normalOffset = targetMaterial.GetTextureOffset(normalSrcID);

                    decalMPB.SetVector(normalDestIDST, new Vector4(normalScale.x, normalScale.y, normalOffset.x, normalOffset.y));
                }
            }
        }

        public void Project(Matrix4x4 orthoMatrix, OrientedBounds projectorBounds, Transform projector) {
            var targetBounds = _targetRenderer.bounds;
            if (projectorBounds.Intersects(targetBounds)) {
                _projectionEnabled = true;
                var projectorToTargetMatrix = target.worldToLocalMatrix * projector.localToWorldMatrix;

                var projectionMatrix = orthoMatrix * projectorToTargetMatrix.inverse;
                var decalNormal = projectorToTargetMatrix.MultiplyVector(Vector3.back).normalized;
                var decalTangent = projectorToTargetMatrix.MultiplyVector(Vector3.right).normalized;

                decalMPB.SetMatrix(_projectionMatrixID, projectionMatrix);
                decalMPB.SetVector(_decalNormalID, decalNormal);
                decalMPB.SetVector(_decalTangentID, decalTangent);
                Debug.Log($"Projection enabled for {target.gameObject}");
            }
            else {
                _projectionEnabled = false;
                Debug.Log($"Projection disabled for {target.gameObject}");
            }
        }

        public bool Render(Material decalMaterial, MaterialPropertyBlock partMPB, Camera camera) {
            if (_projectionEnabled) {
                decalMPB.SetFloat(PropertyIDs._RimFalloff, partMPB.GetFloat(PropertyIDs._RimFalloff));
                decalMPB.SetColor(PropertyIDs._RimColor, partMPB.GetColor(PropertyIDs._RimFalloff));

                Graphics.DrawMesh(_targetMesh, target.localToWorldMatrix, decalMaterial, 0, camera, 0, decalMPB, ShadowCastingMode.Off, true);

                return true;
            }

            return false;
        }
    }
}