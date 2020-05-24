Shader "ConformalDecals/Feature/Bumped"
{
    Properties
    {
        [Header(Texture Maps)]
		_Decal("Decal Texture", 2D) = "gray" {}
		_DecalBumpMap("Decal Bump Map", 2D) = "bump" {}
		
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
            
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"
            #include "LightingKSP.cginc"
            #include "DecalsCommon.cginc"

            sampler2D _Decal;
            sampler2D _DecalBumpMap;
  
            float _Cutoff;
            float _Opacity;
            float _RimFalloff;
            float4 _RimColor;
            
            void surf (DecalSurfaceInput IN, inout SurfaceOutput o)
            {
                fixed4 projUV = UNITY_PROJ_COORD(IN.uv_decal);

                // since I cant easily affect the clamping mode in KSP, do it here
                clip(projUV.xyz);
                clip(1-projUV.xyz);
                
                // clip backsides
                clip(dot(_DecalNormal, IN.normal));

                float4 color = tex2D(_Decal, projUV);
                float3 normal = UnpackNormal(tex2D(_DecalBumpMap, projUV));
                half rim = 1.0 - saturate(dot (normalize(IN.viewDir), normal));
                float3 emission = (_RimColor.rgb * pow(rim, _RimFalloff)) * _RimColor.a;

                // clip alpha
                clip(color.a - _Cutoff);
                
                o.Albedo = UnderwaterFog(IN.worldPosition, color).rgb;
                o.Alpha = color.a * _Opacity;
                o.Emission = emission;
                o.Normal = DECAL_ORIENT_NORMAL(normal, IN);
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
            
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"
            #include "LightingKSP.cginc"
            #include "DecalsCommon.cginc"

            sampler2D _Decal;
            sampler2D _DecalBumpMap;
            
            float _Cutoff;
            float _Opacity;
            float _RimFalloff;
            float4 _RimColor;
            
            void surf (DecalSurfaceInput IN, inout SurfaceOutput o)
            {
                fixed4 projUV = UNITY_PROJ_COORD(IN.uv_decal);

                // since I cant easily affect the clamping mode in KSP, do it here
                clip(projUV.xyz);
                clip(1-projUV.xyz);
                
                // clip backsides
                clip(dot(_DecalNormal, IN.normal));

                float4 color = tex2D(_Decal, projUV);
                float3 normal = UnpackNormal(tex2D(_DecalBumpMap, projUV));
                half rim = 1.0 - saturate(dot (normalize(IN.viewDir), normal));
                float3 emission = (_RimColor.rgb * pow(rim, _RimFalloff)) * _RimColor.a;

                // clip alpha
                clip(color.a - _Cutoff);
                
                o.Albedo = UnderwaterFog(IN.worldPosition, color).rgb;
                o.Alpha = color.a * _Opacity;
                o.Emission = emission;
                o.Normal = DECAL_ORIENT_NORMAL(normal, IN);
            }

            ENDCG
        } 
        
        // shadow casting support
        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
}	