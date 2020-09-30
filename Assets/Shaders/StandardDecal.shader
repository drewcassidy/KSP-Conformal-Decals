﻿Shader "ConformalDecals/Decal/Standard"
{
    Properties
    {
        [Header(Decal)]
        _Decal("Decal Texture", 2D) = "gray" {}
        [Toggle(DECAL_SDF_ALPHA)] _Decal_SDF_Alpha ("SDF in Alpha", int) = 0


        [Header(Normal)]
        [Toggle(DECAL_BASE_NORMAL)] _BaseNormal ("Use Base Normal", int) = 0
        [Toggle(DECAL_BUMPMAP)] _Decal_BumpMap ("Has BumpMap", int) = 0
        _BumpMap("Bump Map", 2D) = "bump" {}
        _EdgeWearStrength("Edge Wear Strength", Range(0,500)) = 100
        _EdgeWearOffset("Edge Wear Offset", Range(0,1)) = 0.1
    
        [Header(Specularity)]
        [Toggle(DECAL_SPECMAP)] _Decal_SpecMap ("Has SpecMap", int) = 0
        _SpecMap ("Specular Map)", 2D) = "black" {}
        _SpecColor ("_SpecColor", Color) = (0.25, 0.25, 0.25, 1)
        _Shininess ("Shininess", Range (0.03, 10)) = 0.3

        [Header(Emissive)]
        [Toggle(DECAL_EMISSIVE)] _Decal_Emissive ("Has Emissive", int) = 0
        _Emissive("_Emissive", 2D) = "black" {}
        _EmissiveColor("_EmissiveColor", Color) = (0,0,0,1)

        _Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
        _DecalOpacity("Opacity", Range(0,1) ) = 1
        _Background("Background Color", Color) = (0.9,0.9,0.9,0.7)
        
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", int) = 2
        [Toggle] _ZWrite ("ZWrite", Float) = 1.0

        [Toggle(DECAL_PREVIEW)] _Preview ("Preview", int) = 0

        [Header(Effects)]
        [PerRendererData]_Opacity("_Opacity", Range(0,1) ) = 1
        [PerRendererData]_Color("_Color", Color) = (1,1,1,1)
        [PerRendererData]_RimFalloff("_RimFalloff", Range(0.01,5) ) = 0.1
        [PerRendererData]_RimColor("_RimColor", Color) = (0,0,0,0)
        [PerRendererData]_UnderwaterFogFactor ("Underwater Fog Factor", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "Queue" = "Geometry+100" "IgnoreProjector" = "true" "DisableBatching" = "true"}
        Cull [_Cull]
        
        Pass
        {
            Name "FORWARD"
            Tags { "LightMode" = "ForwardBase" }
            ZWrite [_ZWrite] 
            ZTest LEqual  
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert_forward
            #pragma fragment frag_forward

            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap
            #pragma skip_variants SHADOWS_DEPTH SHADOWS_CUBE SHADOWS_SHADOWMASK LIGHTMAP_SHADOW_MIXING POINT_COOKIE
            #pragma multi_compile_local __ DECAL_PREVIEW
            #pragma multi_compile_local __ DECAL_BASE_NORMAL DECAL_BUMPMAP
            #pragma multi_compile_local __ DECAL_SPECMAP
            #pragma multi_compile_local __ DECAL_EMISSIVE
            #pragma multi_compile_local __ DECAL_SDF_ALPHA

            #include "UnityCG.cginc"
            #include "DecalsCommon.cginc"
            #include "DecalsSurface.cginc"
            #include "SDF.cginc"
            #include "StandardDecal.cginc"
 
            ENDCG
        } 
        
        Pass
        {
            Name "FORWARD"
            Tags { "LightMode" = "ForwardAdd" }
            ZWrite Off
            ZTest LEqual  
            Blend One One
            Offset -1, -1

            CGPROGRAM
            #pragma vertex vert_forward
            #pragma fragment frag_forward

            #pragma multi_compile_fwdadd nolightmap nodirlightmap nodynlightmap
            #pragma skip_variants SHADOWS_DEPTH SHADOWS_CUBE SHADOWS_SHADOWMASK LIGHTMAP_SHADOW_MIXING POINT_COOKIE
            #pragma multi_compile_local __ DECAL_PREVIEW
            #pragma multi_compile_local __ DECAL_BASE_NORMAL DECAL_BUMPMAP
            #pragma multi_compile_local __ DECAL_SPECMAP
            #pragma multi_compile_local __ DECAL_EMISSIVE
            #pragma multi_compile_local __ DECAL_SDF_ALPHA
  
            #include "UnityCG.cginc"
            #include "DecalsCommon.cginc"
            #include "DecalsSurface.cginc"
            #include "SDF.cginc"
            #include "StandardDecal.cginc"

            ENDCG
        } 
        
        // shadow casting support
        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
}    