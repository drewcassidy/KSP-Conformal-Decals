using System;
using ConformalDecals.MaterialModifiers;
using UnityEngine;
using UnityEngine.Rendering;

namespace ConformalDecals {
    public class ProjectionTarget {
        private static readonly int _projectionMatrixID = Shader.PropertyToID("_ProjectionMatrix");
        private static readonly int _decalNormalID      = Shader.PropertyToID("_DecalNormal");
        private static readonly int _decalTangentID     = Shader.PropertyToID("_DecalTangent");

        // Projector object data
        public Transform Projector;

        // Target object data
        public readonly Transform Target;

        private readonly Renderer _targetRenderer;
        private readonly Mesh     _targetMesh;
        private          bool     _projectionEnabled;

        // property block
        public readonly MaterialPropertyBlock DecalMPB;

        public ProjectionTarget(MeshRenderer targetRenderer, Mesh targetMesh, MaterialPropertyCollection properties) {
            Target = targetRenderer.transform;
            _targetRenderer = targetRenderer;
            _targetMesh = targetMesh;
            var targetMaterial = targetRenderer.sharedMaterial;

            DecalMPB = new MaterialPropertyBlock();

            if (properties.UseBaseNormal) {
                var normalSrcID = Shader.PropertyToID(properties.BaseNormalSrc);
                var normalDestID = Shader.PropertyToID(properties.BaseNormalDest);
                var normalDestIDST = Shader.PropertyToID(properties.BaseNormalDest + "_ST");

                var normal = targetMaterial.GetTexture(normalSrcID);
                if (normal != null) {

                    DecalMPB.SetTexture(normalDestID, targetMaterial.GetTexture(normalSrcID));

                    var normalScale = targetMaterial.GetTextureScale(normalSrcID);
                    var normalOffset = targetMaterial.GetTextureOffset(normalSrcID);

                    DecalMPB.SetVector(normalDestIDST, new Vector4(normalScale.x, normalScale.y, normalOffset.x, normalOffset.y));
                }
            }
        }

        public void Project(Matrix4x4 orthoMatrix, Bounds projectorBounds) {
            var projectorToTargetMatrix = Target.worldToLocalMatrix * Projector.localToWorldMatrix;

            var projectionMatrix = orthoMatrix * projectorToTargetMatrix.inverse;
            var decalNormal = projectorToTargetMatrix.MultiplyVector(Vector3.back).normalized;
            var decalTangent = projectorToTargetMatrix.MultiplyVector(Vector3.right).normalized;

            DecalMPB.SetMatrix(_projectionMatrixID, projectionMatrix);
            DecalMPB.SetVector(_decalNormalID, decalNormal);
            DecalMPB.SetVector(_decalTangentID, decalTangent);

            var targetBounds = new OrientedBounds(Target.localToWorldMatrix, _targetRenderer.bounds);
            _projectionEnabled = targetBounds.Intersects(projectorBounds);
        }

        public bool Render(Material decalMaterial, MaterialPropertyBlock partMPB, Camera camera) {
            if (_projectionEnabled) {
                DecalMPB.SetFloat(PropertyIDs._RimFalloff, partMPB.GetFloat(PropertyIDs._RimFalloff));
                DecalMPB.SetColor(PropertyIDs._RimFalloff, partMPB.GetColor(PropertyIDs._RimFalloff));

                Graphics.DrawMesh(_targetMesh, Target.localToWorldMatrix, decalMaterial, 0, camera, 0, DecalMPB, ShadowCastingMode.Off, true);

                return true;
            }

            return false;
        }
    }
}