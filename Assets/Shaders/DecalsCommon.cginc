#ifndef DECALS_COMMON_INCLUDED
#define DECALS_COMMON_INCLUDED

struct DecalSurfaceInput
{
    float2 uv_decal;
    
    #ifdef DECAL_NORMAL
        float2 uv_bump;
    #endif //DECAL_NORMAL
    
    #ifdef DECAL_SPECULAR
        float2 uv_spec;
    #endif //DECAL_SPECULAR
    
    #ifdef DECAL_EMISSIVE
        float2 uv_glow;
    #endif //DECAL_EMISSIVE
    
    #ifdef DECAL_BASE_NORMAL
        float3 normal;
    #endif
    
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

// Projection matrix, normal, and tangent vectors
float4x4 _ProjectionMatrix;
float3 _DecalNormal;
float3 _DecalTangent;

#ifdef DECAL_BASE_NORMAL
    sampler2D _BumpMap;
    float4 _BumpMap_ST;
#endif //DECAL_BASE_NORMAL

float _Cutoff;
float _DecalOpacity;
float _Opacity;

inline void decalClipAlpha(float alpha) {
    #ifndef DECAL_PREVIEW 
        clip(alpha - _Cutoff + 0.01);
    #endif
}

// declare surf function, 
// this must be defined in any shader using this cginc
void surf (DecalSurfaceInput IN, inout SurfaceOutput o);

v2f vert_forward(appdata_decal v)
{
    v2f o;
    UNITY_INITIALIZE_OUTPUT(v2f,o);
    
    o.pos = UnityObjectToClipPos(v.vertex);
    o.normal = v.normal;
    
    #ifdef DECAL_PREVIEW
        o.uv_decal = v.texcoord;
    #else
        o.uv_decal = mul (_ProjectionMatrix, v.vertex);
    #endif //DECAL_PREVIEW
    
    #ifdef DECAL_BASE_NORMAL
        o.uv_base = TRANSFORM_TEX(v.texcoord, _BumpMap);
    #endif //DECAL_BASE_NORMAL
    
    float3 worldPosition = mul(unity_ObjectToWorld, v.vertex).xyz;
    float3 worldNormal = UnityObjectToWorldNormal(v.normal);
    
    #if defined(DECAL_BASE_NORMAL) || defined(DECAL_PREVIEW)
        // use tangent of base geometry
        fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
        fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
        fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
    #else
        // use tangent of projector
        fixed3 decalTangent = UnityObjectToWorldDir(_DecalTangent);
        fixed3 worldBinormal = cross(decalTangent, worldNormal);
        fixed3 worldTangent = cross(worldNormal, worldBinormal);
    #endif //defined(DECAL_BASE_NORMAL) || defined(DECAL_PREVIEW)
    
    o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPosition.x);
    o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPosition.y);
    o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPosition.z);
    
    // forward base pass specific lighting code
    #ifdef UNITY_PASS_FORWARDBASE
        // SH/ambient light
        #if UNITY_SHOULD_SAMPLE_SH
            float3 shlight = ShadeSH9 (float4(worldNormal,1.0));
            o.vlight = shlight;
        #else
            o.vlight = 0.0;
        #endif // UNITY_SHOULD_SAMPLE_SH
    
        // vertex light
        #ifdef VERTEXLIGHT_ON
            o.vlight += Shade4PointLights (
            unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
            unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
            unity_4LightAtten0, worldPosition, worldNormal );
        #endif // VERTEXLIGHT_ON
    #endif // UNITY_PASS_FORWARDBASE
    
    // pass shadow and, possibly, light cookie coordinates to pixel shader
    UNITY_TRANSFER_LIGHTING(o, 0.0);
    
    return o;
}

fixed4 frag_forward(v2f IN) : SV_Target
{
    // declare data
    DecalSurfaceInput i;
    SurfaceOutput o;
    fixed4 c = 0;
    
    // setup world-space TBN vectors
    UNITY_EXTRACT_TBN(IN);
    
    float3 worldPosition = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
    float3 worldTangent = float3(IN.tSpace0.x, IN.tSpace1.x, IN.tSpace2.x);
    
    // setup world-space light and view direction vectors
    #ifndef USING_DIRECTIONAL_LIGHT
        fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPosition));
    #else
        fixed3 lightDir = _WorldSpaceLightPos0.xyz;
    #endif
    float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPosition));
    float3 viewDir = _unity_tbn_0 * worldViewDir.x + _unity_tbn_1 * worldViewDir.y  + _unity_tbn_2 * worldViewDir.z;
    
    #ifdef DECAL_PREVIEW
        fixed4 uv_projected = IN.uv_decal;
    #else
        // perform decal projection
        fixed4 uv_projected = UNITY_PROJ_COORD(IN.uv_decal);

        // clip texture outside of xyz bounds
        clip(uv_projected.xyz);
        clip(1-uv_projected.xyz);
                    
        // clip backsides
        clip(dot(_DecalNormal, IN.normal));
    #endif //DECAL_PREVIEW
                
    // initialize surface input
    UNITY_INITIALIZE_OUTPUT(DecalSurfaceInput, i)
    i.uv_decal = TRANSFORM_TEX(uv_projected, _Decal);
    
    #ifdef DECAL_NORMAL
        i.uv_bump = TRANSFORM_TEX(uv_projected, _DecalBumpMap);
    #endif //DECAL_NORMAL
        
    #ifdef DECAL_SPECULAR
        i.uv_spec = TRANSFORM_TEX(uv_projected, _SpecMap);
    #endif //DECAL_SPECULAR
    
    #ifdef DECAL_EMISSIVE
        i.uv_glow = TRANSFORM_TEX(uv_projected, _GlowMap);
    #endif //DECAL_EMISSIVE
    
    #ifdef DECAL_BASE_NORMAL
        #ifdef DECAL_PREVIEW
           i.normal = fixed3(0,0,1);
        #else
           i.normal = UnpackNormal(tex2D(_BumpMap, IN.uv_base));
        #endif //DECAL_PREVIEW
    #endif //DECAL_BASE_NORMAL 
    
    //i.normal = IN.normal;
    i.viewDir = viewDir;
    i.worldPosition = worldPosition;
    
    // initialize surface output
    o.Albedo = 0.0;
    o.Emission = 0.0;
    o.Specular = 0.0;
    o.Alpha = 0.0;
    o.Gloss = 0.0;
    o.Normal = fixed3(0,0,1);
    
    // call surface function
    surf(i, o);
   
    #ifdef DECAL_PREVIEW
        o.Albedo = lerp(_Color.rgb,o.Albedo, o.Alpha);
        o.Normal = lerp(float3(0,0,1), o.Normal, o.Alpha);
        o.Gloss = lerp(_Color.a, o.Gloss, o.Alpha);
        o.Emission = lerp(0, o.Emission, o.Alpha);
        o.Alpha = _Opacity;
    #endif //DECAL_PREVIEW 
    
    // compute lighting & shadowing factor
    UNITY_LIGHT_ATTENUATION(atten, IN, worldPosition)
    
    // compute world normal
    float3 WorldNormal;
    WorldNormal.x = dot(_unity_tbn_0, o.Normal);
    WorldNormal.y = dot(_unity_tbn_1, o.Normal);
    WorldNormal.z = dot(_unity_tbn_2, o.Normal);
    WorldNormal = normalize(WorldNormal);
    o.Normal = WorldNormal;
    
    //KSP lighting function
    c += LightingBlinnPhongSmooth(o, lightDir, worldViewDir, atten);
    
    // Forward base emission and ambient/vertex lighting
    #ifdef UNITY_PASS_FORWARDBASE
        c.rgb += o.Emission;
        c.rgb += o.Albedo * IN.vlight;
        c.a = saturate(c.a);
    #endif //UNITY_PASS_FORWARDBASE
    
    // Forward add multiply by alpha
    #ifdef UNITY_PASS_FORWARDADD
        c.rgb *= c.a;
        c.a = 0.0;
    #endif
    
    return c;
}

#endif
