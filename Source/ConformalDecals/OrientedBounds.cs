using UnityEngine;

namespace ConformalDecals {
    public struct OrientedBounds {
        private Bounds _localBounds;

        public Bounds LocalBounds {
            get => _localBounds;
            set => _localBounds = value;
        }

        public Vector3 Center {
            get => _localBounds.center;
            set => _localBounds.center = value;
        }

        public Vector3 Extents {
            get => _localBounds.extents;
            set => _localBounds.extents = value;
        }

        public Vector3 Min {
            get => _localBounds.min;
            set => _localBounds.min = value;
        }

        public Vector3 Max {
            get => _localBounds.max;
            set => _localBounds.max = value;
        }

        public Vector3 Size {
            get => _localBounds.size;
            set => _localBounds.size = value;
        }

        public Vector3 UnitX {
            get => OrientationMatrix.GetColumn(0);
            private set => OrientationMatrix.SetColumn(0, value);
        }

        public Vector3 UnitY {
            get => OrientationMatrix.GetColumn(1);
            private set => OrientationMatrix.SetColumn(1, value);
        }

        public Vector3 UnitZ {
            get => OrientationMatrix.GetColumn(2);
            private set => OrientationMatrix.SetColumn(2, value);
        }

        public Matrix4x4 OrientationMatrix { get; private set; }


        public OrientedBounds(Matrix4x4 matrix, Bounds aabb) {
            Vector3 unitX = matrix.GetColumn(0);
            Vector3 unitY = matrix.GetColumn(1);
            Vector3 unitZ = matrix.GetColumn(2);
            var scale = new Vector3(unitX.magnitude, unitY.magnitude, unitZ.magnitude);

            _localBounds = new Bounds {
                center = matrix.MultiplyPoint3x4(aabb.center),
                extents = new Vector3(aabb.extents.x / scale.x, aabb.extents.y / scale.y, aabb.extents.z / scale.z)
            };

            OrientationMatrix = Matrix4x4.zero;
            UnitX = unitX / scale.x;
            UnitY = unitY / scale.y;
            UnitZ = unitZ / scale.z;
        }

        public bool Contains(Vector3 point) {
            var delta = point - Center;
            var localPoint = Center + OrientationMatrix.transpose.MultiplyVector(delta);
            return _localBounds.Contains(localPoint);
        }

        public bool Intersects(OrientedBounds other) {
            // OrientationMatrix should always be orthogonal,
            // so we can cheat and use the transpose for the inverse
            var inverseOrientationMatrix = OrientationMatrix.transpose;

            // matrix expressing other in our coordinate frame
            var R = inverseOrientationMatrix * other.OrientationMatrix;

            // compute translation vector t in our coordinate frame
            var t = inverseOrientationMatrix.MultiplyVector(other.Center - Center);

            return IntersectOBBSeperatingAxis(R, t, Extents, other.Extents);
        }

        public bool Intersects(Bounds other) {
            // matrix expressing other in our coordinate frame
            var R = OrientationMatrix.transpose;

            // compute translation vector t in our coordinate frame
            var t = R.MultiplyVector(other.center - Center);

            return IntersectOBBSeperatingAxis(R, t, Extents, other.extents);
        }

        private static bool IntersectOBBSeperatingAxis(Matrix4x4 R, Vector3 t, Vector3 a, Vector3 b) {
            // Compute common subexpressions. Add in an epsilon term to
            // counteract arithmetic errors when two edges are parallel and
            // their cross product is (near) null
            var absR = R;
            for (int i = 0; i < 3; i++) {
                for (int j = 0; j < 3; j++) {
                    absR[i, j] = Mathf.Abs(R[i, j]) + Mathf.Epsilon;
                }
            }

            float ra, rb;

            // Test axes L = A0, L = A1, L = A2
            for (int i = 0; i < 3; i++) {
                ra = a[i];
                rb = b[0] * absR[i, 0] + b[1] * absR[i, 1] + b[2] * absR[i, 2];
                if (Mathf.Abs(t[i]) > ra + rb) return false;
            }

            // Test axes L = B0, L = B1, L = B2
            for (int i = 0; i < 3; i++) {
                ra = a[0] * absR[0, i] + a[1] * absR[1, i] + a[2] * absR[2, i];
                rb = b[i];
                if (Mathf.Abs(t[0] * R[0, i] + t[1] * R[1, i] + t[2] * R[2, i]) > ra + rb) return false;
            }

            // Test axis L = A0 x B0
            ra = a[1] * absR[2, 0] + a[2] * absR[1, 0];
            rb = b[1] * absR[0, 2] + b[2] * absR[0, 1];
            if (Mathf.Abs(t[2] * R[1, 0] - t[1] * R[2, 0]) > ra + rb) return false;

            // Test axis L = A0 x B1
            ra = a[1] * absR[2, 1] + a[2] * absR[1, 1];
            rb = b[0] * absR[0, 2] + b[2] * absR[0, 0];
            if (Mathf.Abs(t[2] * R[1, 1] - t[1] * R[2, 1]) > ra + rb) return false;

            // Test axis L = A0 x B2
            ra = a[1] * absR[2, 2] + a[2] * absR[1, 2];
            rb = b[0] * absR[0, 1] + b[1] * absR[0, 0];
            if (Mathf.Abs(t[2] * R[1, 2] - t[1] * R[2, 2]) > ra + rb) return false;


            // Test axis L = A1 x B0
            ra = a[0] * absR[2, 0] + a[2] * absR[0, 0];
            rb = b[1] * absR[1, 2] + b[2] * absR[1, 1];
            if (Mathf.Abs(t[0] * R[2, 0] - t[2] * R[0, 0]) > ra + rb) return false;

            // Test axis L = A1 x B1
            ra = a[0] * absR[2, 1] + a[2] * absR[0, 1];
            rb = b[0] * absR[1, 2] + b[2] * absR[1, 0];
            if (Mathf.Abs(t[0] * R[2, 1] - t[2] * R[0, 1]) > ra + rb) return false;

            // Test axis L = A1 x B2
            ra = a[0] * absR[2, 2] + a[2] * absR[0, 2];
            rb = b[0] * absR[1, 1] + b[1] * absR[1, 0];
            if (Mathf.Abs(t[2] * R[1, 2] - t[1] * R[2, 2]) > ra + rb) return false;


            // Test axis L = A2 x B0
            ra = a[0] * absR[1, 0] + a[1] * absR[0, 1];
            rb = b[1] * absR[2, 2] + b[2] * absR[2, 1];
            if (Mathf.Abs(t[1] * R[0, 0] - t[0] * R[1, 0]) > ra + rb) return false;

            // Test axis L = A2 x B1
            ra = a[0] * absR[1, 1] + a[1] * absR[0, 1];
            rb = b[0] * absR[2, 2] + b[2] * absR[2, 0];
            if (Mathf.Abs(t[1] * R[0, 1] - t[0] * R[1, 1]) > ra + rb) return false;

            // Test axis L = A2 x B2
            ra = a[0] * absR[1, 2] + a[1] * absR[0, 2];
            rb = b[0] * absR[2, 1] + b[1] * absR[2, 0];
            if (Mathf.Abs(t[1] * R[0, 2] - t[0] * R[1, 2]) > ra + rb) return false;

            // Since no separating axis is found, the OBBs must be intersecting
            return true;
        }
    }
}