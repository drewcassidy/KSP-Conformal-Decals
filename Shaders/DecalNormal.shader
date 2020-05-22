Shader "ConformalDecals/Feature/Bumped"
{
	Properties 
	{
		_Decal ("Decal Texture", 2D) = "gray" {}
		_DecalBumpMap("Decal Normal Map", 2D) = "bump" {}
		
	    _Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
		_Opacity("_Opacity", Range(0,1) ) = 1
	}
	
	SubShader 
	{
		Tags { "Queue" = "Transparent" }

		ZWrite Off
		ZTest LEqual
		Blend SrcAlpha OneMinusSrcAlpha  

		CGPROGRAM

		#pragma surface surf Lambert alpha vertex:vert
		#pragma target 4.0

		float4x4 _ProjectionMatrix;
		float3 _DecalNormal;
		float3 _DecalBiNormal;
		
		sampler2D _Decal;
		sampler2D _DecalBumpMap;
		
        float _Cutoff;
		float _Opacity;

		struct Input
		{
			float4 pos_decal : TEXCOORD0;
			float3 normal : NORMAL;
		};

		void vert (inout appdata_full v, out Input o) {
			o.pos_decal = mul (_ProjectionMatrix, v.vertex);
            o.normal = v.normal;
            float3 localTangent = normalize(cross(v.normal, _DecalBiNormal));
		}

		void surf (Input IN, inout SurfaceOutput o)
		{
			fixed4 projUV = UNITY_PROJ_COORD(IN.pos_decal);

			// since I cant easily affect the clamping mode in KSP, do it here
			clip(projUV.xyz);
			clip(1-projUV.xyz);
			
			// clip backsides
			clip(dot(_DecalNormal, IN.normal));

			float4 color = tex2D(_Decal, projUV);
			float3 normal = UnpackNormal(tex2D(_DecalBumpMap, projUV));
			
			clip (color.a - _Cutoff);

			o.Albedo = color.rgb;
			o.Normal = normal;
			o.Alpha = color.a * _Opacity;
		}

		ENDCG
	}
}