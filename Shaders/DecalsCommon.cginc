#ifndef DECALS_COMMON_INCLUDED
#define DECALS_COMMON_INCLUDED

struct DecalSurfaceInput
{
    float4 uv_decal;
    #ifdef DECAL_BASE_NORMAL
        float2 uv_base;
    #endif
    float3 normal;
    float3 viewDir;
};

struct appdata_decal
{
    float4 vertex : POSITION;
    float3 normal : NORMAL;
    #ifdef DECAL_BASE_NORMAL
        float4 texcoord : TEXCOORD0;
    #endif
};

struct v2f
{
    UNITY_POSITION(pos);
    float3 normal : NORMAL;
    float4 uv_decal : TEXCOORD0;
    #ifdef DECAL_BASE_NORMAL
        float2 uv_base : TEXCOORD1;
    #endif
    float4 tSpace0 : TEXCOORD2;
    float4 tSpace1 : TEXCOORD3;
    float4 tSpace2 : TEXCOORD4;
    #if UNITY_SHOULD_SAMPLE_SH
        half3 sh : TEXCOORD5; // SH
    #endif
    UNITY_SHADOW_COORDS(6)
};

float4x4 _ProjectionMatrix;
float3 _DecalNormal;
float3 _DecalTangent;

#endif
