Shader "ConformalDecals/Feature/Bumped"
{
    Properties
    {
        [Header(Texture Maps)]
		_Decal("Decal Texture", 2D) = "gray" {}
		_DecalBumpMap("Decal Bump Map", 2D) = "bump" {}
		
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
        Tags { "Queue" = "Geometry+100" }
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
            #pragma multi_compile DECAL_PROJECT DECAL_PREVIEW
            
            sampler2D _Decal;
            sampler2D _DecalBumpMap;
            
            float4 _Decal_ST;
            float4 _DecalBumpMap_ST;

            float _RimFalloff;
            float4 _RimColor; 
            
            #define DECAL_NORMAL
            
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"
            #include "LightingKSP.cginc"
            #include "DecalsCommon.cginc"
            
            void surf (DecalSurfaceInput IN, inout SurfaceOutput o)
            {
                float4 color = tex2D(_Decal, IN.uv_decal);
                float3 normal = UnpackNormalDXT5nm(tex2D(_DecalBumpMap, IN.uv_bump));
 
                #ifdef DECAL_PROJECT
                    // clip alpha
                    clip(color.a - _Cutoff + 0.01);
                #endif //DECAL_PROJECT
                
                half rim = 1.0 - saturate(dot (normalize(IN.viewDir), normal));
                float3 emission = (_RimColor.rgb * pow(rim, _RimFalloff)) * _RimColor.a;

                o.Albedo = UnderwaterFog(IN.worldPosition, color).rgb;
                o.Alpha = color.a * _DecalOpacity;
                o.Emission = emission;
                o.Normal = normal;
            }

            ENDCG
        } 
        
        Pass
        {
            Name "FORWARD"
       		Tags { "LightMode" = "ForwardAdd" }
     		Blend One One

            CGPROGRAM
            #pragma vertex vert_forward
            #pragma fragment frag_forward

            #pragma multi_compile_fwdadd nolightmap nodirlightmap nodynlightmap
            #pragma multi_compile DECAL_PROJECT DECAL_PREVIEW
            
            sampler2D _Decal;
            sampler2D _DecalBumpMap;
            
            float4 _Decal_ST;
            float4 _DecalBumpMap_ST;
  
            float _RimFalloff;
            float4 _RimColor; 
            
            #define DECAL_NORMAL
            
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"
            #include "LightingKSP.cginc"
            #include "DecalsCommon.cginc"
            
            void surf (DecalSurfaceInput IN, inout SurfaceOutput o)
            {
                float4 color = tex2D(_Decal, IN.uv_decal);
                float3 normal = UnpackNormal(tex2D(_DecalBumpMap, IN.uv_bump));
 
                #ifdef DECAL_PROJECT
                    // clip alpha
                    clip(color.a - _Cutoff + 0.01);
                #endif //DECAL_PROJECT
                
                half rim = 1.0 - saturate(dot (normalize(IN.viewDir), normal));
                float3 emission = (_RimColor.rgb * pow(rim, _RimFalloff)) * _RimColor.a;

                o.Albedo = UnderwaterFog(IN.worldPosition, color).rgb;
                o.Alpha = color.a * _DecalOpacity;
                o.Emission = emission;
                o.Normal = normal;
            }

            ENDCG
        } 
        
        // shadow casting support
        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
}	