Shader "ConformalDecals/Decal/Standard"
{
    Properties
    {
        [Header(Texture Maps)]
        _Decal("Decal Texture", 2D) = "gray" {}
        _BumpMap("Bump Map", 2D) = "bump" {}
        
        _EdgeWearStrength("Edge Wear Strength", Range(0,500)) = 100
        _EdgeWearOffset("Edge Wear Offset", Range(0,1)) = 0.1
    
        _Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
        _DecalOpacity("Opacity", Range(0,1) ) = 1
        _Background("Background Color", Color) = (0.9,0.9,0.9,0.7)
        
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", int) = 2
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
        Ztest LEqual  
        
        Pass
        {
            Name "FORWARD"
            Tags { "LightMode" = "ForwardBase" }
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
            #include "StandardDecal.cginc"
 
            ENDCG
        } 
        
        Pass
        {
            Name "FORWARD_ADD"
            Tags { "LightMode" = "ForwardAdd" }
            Blend One One

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
            #include "StandardDecal.cginc"

            ENDCG
        } 
        
        // shadow casting support
        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
}    