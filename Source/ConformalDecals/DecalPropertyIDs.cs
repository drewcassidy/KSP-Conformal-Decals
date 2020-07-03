using UnityEngine;

// ReSharper disable InconsistentNaming

namespace ConformalDecals {
    public static class DecalPropertyIDs {
        public static readonly int _BumpMap          = Shader.PropertyToID("_BumpMap");
        public static readonly int _BumpMap_ST       = Shader.PropertyToID("_BumpMap_ST");
        public static readonly int _Cull             = Shader.PropertyToID("_Cull");
        public static readonly int _Cutoff           = Shader.PropertyToID("_Cutoff");
        public static readonly int _DecalNormal      = Shader.PropertyToID("_DecalNormal");
        public static readonly int _DecalOpacity     = Shader.PropertyToID("_DecalOpacity");
        public static readonly int _DecalTangent     = Shader.PropertyToID("_DecalTangent");
        public static readonly int _EdgeWearStrength = Shader.PropertyToID("_EdgeWearStrength");
        public static readonly int _ProjectionMatrix = Shader.PropertyToID("_ProjectionMatrix");
        public static readonly int _ZWrite           = Shader.PropertyToID("_ZWrite");
    }
}