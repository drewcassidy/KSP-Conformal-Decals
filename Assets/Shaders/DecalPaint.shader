Shader "ConformalDecals/Paint/Diffuse"
{
    Properties
    {
        [Header(Texture Maps)]
		_Decal("Decal Texture", 2D) = "gray" {}
		_BumpMap("Bump Map", 2D) = "bump" {}
		
		_EdgeWearStrength("Edge Wear Strength", Range(0,100)) = 0
		_EdgeWearOffset("Edge Wear Offset", Range(0,1)) = 0
	
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
        Tags { "Queue" = "Geometry+400" }
        ZWrite Off
        ZTest LEqual
        Offset -1, -1
        
        Pass
        {
            Name "FORWARD"
       		Tags { "LightMode" = "ForwardBase" }
     		Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert_forward
            #pragma fragment frag_forward

            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap
            
            sampler2D _Decal;
            sampler2D _BumpMap;
            
            float4 _Decal_ST;
            float4 _BumpMap_ST;
            
            float _EdgeWearStrength;
            float _EdgeWearOffset;
            
            float _Cutoff;
            float _Opacity;
            float _RimFalloff;
            float4 _RimColor;
            
            #define DECAL_BASE_NORMAL
            
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"
            #include "LightingKSP.cginc"
            #include "DecalsCommon.cginc"

            void surf (DecalSurfaceInput IN, inout SurfaceOutput o)
            {
                 fixed4 uv_projected = UNITY_PROJ_COORD(IN.uv_decal);

                // since I cant easily affect the clamping mode in KSP, do it here
                clip(uv_projected.xyz);
                clip(1-uv_projected.xyz);
                
                // clip backsides
                clip(dot(_DecalNormal, IN.normal));
                
                float2 uv_decal = TRANSFORM_TEX(uv_projected, _Decal);
                float4 color = tex2D(_Decal, uv_decal);
                
                // clip alpha
                clip(color.a - _Cutoff);

                float3 normal = UnpackNormal(tex2D(_BumpMap, IN.uv_base));
                half rim = 1.0 - saturate(dot (normalize(IN.viewDir), normal));
                float3 emission = (_RimColor.rgb * pow(rim, _RimFalloff)) * _RimColor.a;
                
                float wearFactor = 1 - normal.z;
                float wearFactorAlpha = saturate(_EdgeWearStrength * wearFactor);

                color.a *= saturate(1 + _EdgeWearOffset - saturate(_EdgeWearStrength * wearFactor));
                
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

            CGPROGRAM
            #pragma vertex vert_forward
            #pragma fragment frag_forward

            #pragma multi_compile_fwdadd nolightmap nodirlightmap nodynlightmap
            
            sampler2D _Decal;
            sampler2D _BumpMap;
            
            float4 _Decal_ST;
            float4 _BumpMap_ST;
            
            float _EdgeWearStrength;
            float _EdgeWearOffset;
            
            float _Cutoff;
            float _Opacity;
            float _RimFalloff;
            float4 _RimColor;
            
            #define DECAL_BASE_NORMAL
            
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"
            #include "LightingKSP.cginc"
            #include "DecalsCommon.cginc"
            
            void surf (DecalSurfaceInput IN, inout SurfaceOutput o)
            {
                fixed4 uv_projected = UNITY_PROJ_COORD(IN.uv_decal);

                // since I cant easily affect the clamping mode in KSP, do it here
                clip(uv_projected.xyz);
                clip(1-uv_projected.xyz);
                
                // clip backsides
                clip(dot(_DecalNormal, IN.normal));
                
                float2 uv_decal = TRANSFORM_TEX(uv_projected, _Decal);
                float4 color = tex2D(_Decal, uv_decal);
                
                // clip alpha
                clip(color.a - _Cutoff);

                float3 normal = UnpackNormal(tex2D(_BumpMap, IN.uv_base));
                half rim = 1.0 - saturate(dot (normalize(IN.viewDir), normal));
                float3 emission = (_RimColor.rgb * pow(rim, _RimFalloff)) * _RimColor.a;
                
                float wearFactor = 1 - normal.z;
                float wearFactorAlpha = saturate(_EdgeWearStrength * wearFactor);

                color.a *= saturate(1 + _EdgeWearOffset - saturate(_EdgeWearStrength * wearFactor));
                
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