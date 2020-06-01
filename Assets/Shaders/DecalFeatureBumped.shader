Shader "ConformalDecals/Feature/Bumped"
{
    Properties
    {
        [Header(Texture Maps)]
		_Decal("Decal Texture", 2D) = "gray" {}
		_BumpMap("Decal Bump Map", 2D) = "bump" {}
		
	    _Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
		_Opacity("_Opacity", Range(0,1) ) = 1
		
		[Header(Effects)]
		[PerRendererData]_Opacity("_Opacity", Range(0,1) ) = 1
			[PerRendererData]_RimFalloff("_RimFalloff", Range(0.01,5) ) = 0.1
			[PerRendererData]_RimColor("_RimColor", Color) = (0,0,0,0)
			[PerRendererData]_UnderwaterFogFactor ("Underwater Fog Factor", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "Queue" = "Geometry+100" }
        Cull Off
        
        Pass
        {
            Name "FORWARD"
       		Tags { "LightMode" = "ForwardBase" }
     		Blend SrcAlpha OneMinusSrcAlpha
     		ZWrite Off
     		ZTest LEqual

            CGPROGRAM
            #pragma vertex vert_forward
            #pragma fragment frag_forward

            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap
            
            sampler2D _Decal;
            sampler2D _BumpMap;
            
            float4 _Decal_ST;
            float4 _BumpMap_ST;
  
            float _Cutoff;
            float _Opacity;
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
                float3 normal = UnpackNormal(tex2D(_BumpMap, IN.uv_bump));
 
                // clip alpha
                clip(color.a - saturate(_Cutoff + 0.01));
                
                half rim = 1.0 - saturate(dot (normalize(IN.viewDir), normal));
                float3 emission = (_RimColor.rgb * pow(rim, _RimFalloff)) * _RimColor.a;

                o.Albedo = UnderwaterFog(IN.worldPosition, color).rgb;
                o.Alpha = color.a * _Opacity;
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
            ZWrite On
            ZTest Less
            Offset -1, -1

            CGPROGRAM
            #pragma vertex vert_forward
            #pragma fragment frag_forward

            #pragma multi_compile_fwdadd nolightmap nodirlightmap nodynlightmap
            
            sampler2D _Decal;
            sampler2D _BumpMap;
            
            float4 _Decal_ST;
            float4 _BumpMap_ST;
  
            float _Cutoff;
            float _Opacity;
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
                float3 normal = UnpackNormal(tex2D(_BumpMap, IN.uv_bump));
 
                // clip alpha
                clip(color.a - saturate(_Cutoff + 0.01));
                
                half rim = 1.0 - saturate(dot (normalize(IN.viewDir), normal));
                float3 emission = (_RimColor.rgb * pow(rim, _RimFalloff)) * _RimColor.a;

                o.Albedo = UnderwaterFog(IN.worldPosition, color).rgb;
                o.Alpha = color.a * _Opacity;
                o.Emission = emission;
                o.Normal = normal;
            }

            ENDCG
        } 
        
        // shadow casting support
        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
}	