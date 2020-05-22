Shader "ConformalDecals/Paint/Diffuse"
{
	Properties 
	{
		_Decal ("Cookie", 2D) = "gray" {}
		_BumpMap("_BumpMap", 2D) = "bump" {}
		
	    _Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
		_Opacity("_Opacity", Range(0,1) ) = 1
		_NormalWear("_NormalWear", Range(0,100)) = 50
	}
	
	SubShader 
	{
		Tags { "Queue" = "Geometry" }

		ZWrite On
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
		sampler2D _BumpMap;
		
        float _Cutoff;
		float _Opacity;
		float _NormalWear;

		struct Input
		{
			float4 decal : TEXCOORD0;
			float2 uv_BumpMap : TEXCOORD1;
			float4 position : SV_POSITION;
			float3 normal : NORMAL;
		};

		void vert (inout appdata_full v, out Input o) {
			o.decal = mul (_ProjectionMatrix, v.vertex);
			o.uv_BumpMap = v.texcoord.xy;
            o.position = UnityObjectToClipPos(v.vertex);
            o.normal = v.normal;
		}

		void surf (Input IN, inout SurfaceOutput o)
		{
			fixed4 projUV = UNITY_PROJ_COORD(IN.decal);

			// since I cant easily affect the clamping mode in KSP, do it here
			clip(projUV.xyz);
			clip(1-projUV.xyz);
			
			// clip backsides
			clip(dot(_DecalNormal, IN.normal));

			float4 color = tex2D(_Decal, projUV);
			float3 normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
			
			color.a *= (1 - (_NormalWear * (1 - dot(normal, fixed3(0,0,1)))));
			clip (color.a - _Cutoff);
			
			fixed2 normalGradient = fixed2(ddx(normal.z), ddy(normal.z));

			o.Albedo = color.rgb;
			//o.Albedo = projUV;
			o.Normal = normal;
			o.Alpha = color.a * _Opacity;
		}

		ENDCG
	}
}