#ifndef DECALS_COMMON_INCLUDED
#define DECALS_COMMON_INCLUDED

#include "AutoLight.cginc"
#include "Lighting.cginc"

#define CLIP_MARGIN 0.1
#define EDGE_MARGIN 0.01

// UNIFORM VARIABLES
// Projection matrix, normal, and tangent vectors
float4x4 _ProjectionMatrix;
float3 _DecalNormal;
float3 _DecalTangent;

// Common Shading Paramaters
float _Cutoff;
float _DecalOpacity;
float4 _Background;

sampler2D _Decal;
float4 _Decal_ST;

// Variant Shading Parameters
#ifdef DECAL_BASE_NORMAL
    sampler2D _BumpMap;
    float4 _BumpMap_ST;
    float _EdgeWearStrength;
    float _EdgeWearOffset;
#endif //DECAL_BASE_NORMAL

#ifdef DECAL_BUMPMAP 
    sampler2D _BumpMap;
    float4 _BumpMap_ST;
#endif //DECAL_BUMPMAP

#ifdef DECAL_SPECMAP
    sampler2D _SpecMap;
    float4 _SpecMap_ST;
    // specular color is declared in a unity CGINC for some reason??
    fixed _Shininess;
#endif //DECAL_SPECMAP

#ifdef DECAL_EMISSIVE
    sampler2D _Emissive;
    float4 _Emissive_ST;
    fixed4 _Emissive_Color;
#endif //DECAL_EMISSIVE

// KSP EFFECTS
// opacity and color
float _Opacity;
float4 _Color;
float _RimFalloff;
float4 _RimColor;

// fog
float4 _LocalCameraPos;
float4 _LocalCameraDir;
float4 _UnderwaterFogColor;
float _UnderwaterMinAlphaFogDistance;
float _UnderwaterMaxAlbedoFog;
float _UnderwaterMaxAlphaFog;
float _UnderwaterAlbedoDistanceScalar;
float _UnderwaterAlphaDistanceScalar;
float _UnderwaterFogFactor;

// SURFACE INPUT STRUCT
struct DecalSurfaceInput
{
    float3 uv;
    float2 uv_decal;
    
    #ifdef DECAL_BUMPMAP
        float2 uv_bumpmap;
    #endif //DECAL_BUMPMAP
    
    #ifdef DECAL_SPECMAP
        float2 uv_specmap;
    #endif //DECAL_SPECMAP
    
    #ifdef DECAL_EMISSIVE
        float2 uv_emissive;
    #endif //DECAL_EMISSIVE
    
    #ifdef DECAL_BASE_NORMAL
        float3 normal;
    #endif

    float3 vertex_normal;
    float3 viewDir;
    float3 worldPosition;
};

struct appdata_decal
{
    float4 vertex : POSITION;
    float3 normal : NORMAL;
    #if defined(DECAL_BASE_NORMAL) || defined(DECAL_PREVIEW)
        float4 texcoord : TEXCOORD0;
        float4 tangent : TANGENT;
    #endif
};

struct v2f
{
    UNITY_POSITION(pos);
    float3 normal : NORMAL;
    float4 uv_decal : TEXCOORD0;
    
    #ifdef DECAL_BASE_NORMAL
        float2 uv_base : TEXCOORD1;
    #endif //DECAL_BASE_NORMAL
    
    float4 tSpace0 : TEXCOORD2;
    float4 tSpace1 : TEXCOORD3;
    float4 tSpace2 : TEXCOORD4;
    
    #ifdef UNITY_PASS_FORWARDBASE
        fixed3 vlight : TEXCOORD5;
        UNITY_SHADOW_COORDS(6)
    #endif //UNITY_PASS_FORWARDBASE
    
    #ifdef UNITY_PASS_FORWARDADD
        UNITY_LIGHTING_COORDS(5,6)
    #endif //UNITY_PASS_FORWARDADD
};


inline void decalClipAlpha(float alpha) {
    #ifndef DECAL_PREVIEW 
        clip(alpha - 0.001);
    #endif
}

inline float CalcMipLevel(float2 texture_coord) {
    float2 dx = ddx(texture_coord);
    float2 dy = ddy(texture_coord);
    float delta_max_sqr = max(dot(dx, dx), dot(dy, dy));
    
    return 0.5 * log2(delta_max_sqr);
}

inline float BoundsDist(float3 p, float3 normal, float3 projNormal) {
    float3 q = abs(p - 0.5) - 0.5; // 1x1 square/cube centered at (0.5,0.5)
    //float dist = length(max(q,0)) + min(max(q.x,max(q.y,q.z)),0.0); // true SDF
    #ifdef DECAL_PREVIEW
        return 10 * max(q.x, q.y); // 2D pseudo SDF
    #else
        float dist = max(max(q.x, q.y), q.z); // pseudo SDF
        float ndist = EDGE_MARGIN - dot(normal, projNormal); // SDF to normal
        return 10 * max(dist, ndist); // return intersection
    #endif
}

inline float SDFAA(float dist) {
    float ddist = length(float2(ddx(dist), ddy(dist)));
    float pixelDist = dist / ddist;
    return saturate(0.5-pixelDist);
    return saturate(0.5 - dist);
}

#endif