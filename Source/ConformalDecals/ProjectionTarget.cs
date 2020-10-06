using UnityEngine;
using UnityEngine.Rendering;

namespace ConformalDecals {
    public class ProjectionTarget {
        // Target object data
        public readonly Transform target;
        public readonly Part targetPart;

        private readonly Renderer _targetRenderer;
        private readonly Mesh     _targetMesh;
        private          bool     _projectionEnabled;

        // property block
        private readonly MaterialPropertyBlock _decalMPB;

        public ProjectionTarget(Part targetPart, MeshRenderer targetRenderer, Mesh targetMesh) {
            this.targetPart = targetPart;
            this.target = targetRenderer.transform;
            _targetRenderer = targetRenderer;
            _targetMesh = targetMesh;
            _decalMPB = new MaterialPropertyBlock();
        }

        public void Project(Matrix4x4 orthoMatrix, Transform projector, Bounds projectorBounds, bool useBaseNormal) {

            if (projectorBounds.Intersects(_targetRenderer.bounds)) {
                _projectionEnabled = true;
                var targetMaterial = _targetRenderer.sharedMaterial;
                var projectorToTargetMatrix = target.worldToLocalMatrix * projector.localToWorldMatrix;

                var projectionMatrix = orthoMatrix * projectorToTargetMatrix.inverse;
                var decalNormal = projectorToTargetMatrix.MultiplyVector(Vector3.back).normalized;
                var decalTangent = projectorToTargetMatrix.MultiplyVector(Vector3.right).normalized;

                _decalMPB.SetMatrix(DecalPropertyIDs._ProjectionMatrix, projectionMatrix);
                _decalMPB.SetVector(DecalPropertyIDs._DecalNormal, decalNormal);
                _decalMPB.SetVector(DecalPropertyIDs._DecalTangent, decalTangent);

                if (useBaseNormal && targetMaterial.HasProperty(DecalPropertyIDs._BumpMap)) {
                    _decalMPB.SetTexture(DecalPropertyIDs._BumpMap, targetMaterial.GetTexture(DecalPropertyIDs._BumpMap));

                    var normalScale = targetMaterial.GetTextureScale(DecalPropertyIDs._BumpMap);
                    var normalOffset = targetMaterial.GetTextureOffset(DecalPropertyIDs._BumpMap);

                    _decalMPB.SetVector(DecalPropertyIDs._BumpMap_ST, new Vector4(normalScale.x, normalScale.y, normalOffset.x, normalOffset.y));
                }
                else {
                    _decalMPB.SetTexture(DecalPropertyIDs._BumpMap, DecalConfig.BlankNormal);
                }
            }
            else {
                _projectionEnabled = false;
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