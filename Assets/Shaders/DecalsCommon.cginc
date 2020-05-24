#ifndef DECALS_COMMON_INCLUDED
#define DECALS_COMMON_INCLUDED

struct DecalSurfaceInput
{
    float4 uv_decal;
    #ifdef DECAL_BASE_NORMAL
        float2 uv_base;
    #endif //DECAL_BASE_NORMAL
    float3 normal;
    float3 viewDir;
};

struct appdata_decal
{
    float4 vertex : POSITION;
    float3 normal : NORMAL;
    #ifdef DECAL_BASE_NORMAL
        float4 texcoord : TEXCOORD0;
    #endif //DECAL_BASE_NORMAL
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
    fixed3 vlight : TEXCOORD5;
    UNITY_SHADOW_COORDS(6)
};

// Projection matrix, normal, and tangent vectors
float4x4 _ProjectionMatrix;
float3 _DecalNormal;
float3 _DecalTangent;

// declare surf function, 
// this must be defined in any shader using this cginc
void surf (DecalSurfaceInput IN, inout SurfaceOutput o);

v2f vert_forward_base(appdata_decal v)
{
    v2f o;
    UNITY_INITIALIZE_OUTPUT(v2f,o);
    
    o.pos = UnityObjectToClipPos(v.vertex);
    o.normal = v.normal;
    o.uv_decal = mul (_ProjectionMatrix, v.vertex);
    
    #ifdef DECAL_BASE_NORMAL
        o.uv_base = TRANSFORM_TEX(v.texcoord, _BumpMap);
    #endif //DECAL_BASE_NORMAL
    
    float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
    float3 worldNormal = UnityObjectToWorldNormal(v.normal);
    //fixed3 worldTangent = fixed3(0,0,0);//UnityObjectToWorldDir(v.tangent.xyz);
    fixed3 worldTangent = UnityObjectToWorldDir(_DecalTangent);
    fixed3 worldBinormal = cross(worldTangent, worldNormal);
    worldTangent = cross(worldNormal, worldBinormal);
    o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
    o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
    o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
    
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
        unity_4LightAtten0, worldPos, worldNormal );
    #endif // VERTEXLIGHT_ON
    
    UNITY_TRANSFER_LIGHTING(o, 0.0); // pass shadow and, possibly, light cookie coordinates to pixel shader
    
    return o;
}

fixed4 frag_forward_base(v2f IN) : SV_Target
{
    DecalSurfaceInput i;
    SurfaceOutput o;
    fixed4 c = 0;
    
    UNITY_EXTRACT_TBN(IN);
    
    float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
    float3 worldTan = float3(IN.tSpace0.x, IN.tSpace1.x, IN.tSpace2.x);

    #ifndef USING_DIRECTIONAL_LIGHT
        fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
    #else
        fixed3 lightDir = _WorldSpaceLightPos0.xyz;
    #endif
    float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
    float3 viewDir = _unity_tbn_0 * worldViewDir.x + _unity_tbn_1 * worldViewDir.y  + _unity_tbn_2 * worldViewDir.z;
    
    UNITY_INITIALIZE_OUTPUT(DecalSurfaceInput, i)
    i.uv_decal = IN.uv_decal;
    #ifdef DECAL_BASE_NORMAL
        i.uv_base = IN.uv_base;
    #endif
    i.normal = IN.normal;
    i.viewDir = viewDir;
    
    o.Albedo = 0.0;
    o.Emission = 0.0;
    o.Specular = 0.0;
    o.Alpha = 0.0;
    o.Gloss = 0.0;
    o.Normal = fixed3(0,0,1);
    fixed3 normalWorldVertex = fixed3(0,0,1);
    
    // call surface function
    surf(i, o);
    
    // compute lighting & shadowing factor
    UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
    float3 worldN;
    worldN.x = dot(_unity_tbn_0, o.Normal);
    worldN.y = dot(_unity_tbn_1, o.Normal);
    worldN.z = dot(_unity_tbn_2, o.Normal);
    worldN = normalize(worldN);
    o.Normal = worldN;
    
    
    //KSP lighting function
    c += LightingBlinnPhongSmooth(o, lightDir, viewDir, atten);
    c.rgb += o.Emission;
    c.rgb += o.Albedo * IN.vlight;
    return c;
}

#endif
