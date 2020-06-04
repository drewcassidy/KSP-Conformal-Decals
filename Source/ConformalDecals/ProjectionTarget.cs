using ConformalDecals.Util;
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
        private readonly MaterialPropertyBlock _decalMPB;

        private static readonly int normalID        = Shader.PropertyToID("_BumpMap");
        private static readonly int normalIDST      = Shader.PropertyToID("_BumpMap_ST");

        public ProjectionTarget(MeshRenderer targetRenderer, Mesh targetMesh, bool useBaseNormal) {
            target = targetRenderer.transform;
            _targetRenderer = targetRenderer;
            _targetMesh = targetMesh;
            _decalMPB = new MaterialPropertyBlock();
        }

        public void Project(Matrix4x4 orthoMatrix, OrientedBounds projectorBounds, Transform projector, bool useBaseNormal) {
            var targetBounds = _targetRenderer.bounds;
            if (projectorBounds.Intersects(targetBounds)) {
                _projectionEnabled = true;

                var targetMaterial = _targetRenderer.sharedMaterial;
                var projectorToTargetMatrix = target.worldToLocalMatrix * projector.localToWorldMatrix;

                var projectionMatrix = orthoMatrix * projectorToTargetMatrix.inverse;
                var decalNormal = projectorToTargetMatrix.MultiplyVector(Vector3.back).normalized;
                var decalTangent = projectorToTargetMatrix.MultiplyVector(Vector3.right).normalized;

                _decalMPB.SetMatrix(_projectionMatrixID, projectionMatrix);
                _decalMPB.SetVector(_decalNormalID, decalNormal);
                _decalMPB.SetVector(_decalTangentID, decalTangent);
                Debug.Log($"Projection enabled for {target.gameObject}");
                
                if (useBaseNormal && targetMaterial.HasProperty(normalID)) {
                    var normal = targetMaterial.GetTexture(normalID);
                    if (normal != null) {

                        _decalMPB.SetTexture(normalID, targetMaterial.GetTexture(normalID));

                        var normalScale = targetMaterial.GetTextureScale(normalID);
                        var normalOffset = targetMaterial.GetTextureOffset(normalID);

                        _decalMPB.SetVector(normalIDST, new Vector4(normalScale.x, normalScale.y, normalOffset.x, normalOffset.y));
                    }
                }
            }
            else {
                _projectionEnabled = false;
                Debug.Log($"Projection disabled for {target.gameObject}");
            }
        }

        public bool Render(Material decalMaterial, MaterialPropertyBlock partMPB, Camera camera) {
            if (_projectionEnabled) {
                _decalMPB.SetFloat(PropertyIDs._RimFalloff, partMPB.GetFloat(PropertyIDs._RimFalloff));
                _decalMPB.SetColor(PropertyIDs._RimColor, partMPB.GetColor(PropertyIDs._RimColor));

                Graphics.DrawMesh(_targetMesh, target.localToWorldMatrix, decalMaterial, 0, camera, 0, _decalMPB, ShadowCastingMode.Off, true);

                return true;
            }

            return false;
        }
    }
}