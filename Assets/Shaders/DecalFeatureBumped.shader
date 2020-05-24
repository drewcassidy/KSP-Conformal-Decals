Shader "ConformalDecals/Feature/Bumped"
{
    Properties
    {
		_Decal("Decal Texture", 2D) = "gray" {}
		_DecalBumpMap("Decal Normal Map", 2D) = "bump" {}
		
	    _Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
		_Opacity("_Opacity", Range(0,1) ) = 1
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
                clip(color.a - _Cutoff);
                
                o.Normal = normal;
                o.Albedo = color.rgb;
                o.Alpha = color.a * _Opacity;
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
                clip(color.a - _Cutoff);
                
                o.Normal = normal;
                o.Albedo = color.rgb;
                o.Alpha = color.a * _Opacity;
            }

            ENDCG
        } 
        
        // shadow casting support
        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
}	