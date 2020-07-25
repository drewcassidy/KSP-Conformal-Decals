float4 _DecalColor;
float _Weight;

float4 _OutlineColor;
float _OutlineWidth;

void surf(DecalSurfaceInput IN, inout SurfaceOutput o) {
    float4 color = _DecalColor;

    float bias = _Cutoff - (_Weight / 4);
    #ifdef DECAL_OUTLINE
        bias -= _OutlineWidth * 0.25;
    #endif
    float dist = bias - tex2D(_Decal, IN.uv_decal).r;
    float ddist = SDFdDist(dist);
    
    #ifdef DECAL_OUTLINE
        float outlineDist = (_OutlineWidth * 0.5) + dist;
        float outline = SDFAA(outlineDist, -ddist);
        color = lerp(color, _OutlineColor, outline);
    #endif
    
    o.Albedo = UnderwaterFog(IN.worldPosition, color).rgb;
    o.Alpha =  _DecalOpacity * SDFAA(dist, ddist);

    #ifdef DECAL_BASE_NORMAL
        float3 normal = IN.normal;
        float wearFactor = 1 - normal.z;
        float wearFactorAlpha = saturate(_EdgeWearStrength * wearFactor);
        o.Alpha *= saturate(1 + _EdgeWearOffset - saturate(_EdgeWearStrength * wearFactor));
    #endif

    #ifdef DECAL_SPECMAP
        float4 specular = tex2D(_SpecMap, IN.uv_specmap);
        o.Gloss = specular.r;
        o.Specular = _Shininess;
    #endif

    half rim = 1.0 - saturate(dot(normalize(IN.viewDir), o.Normal));
    o.Emission = (_RimColor.rgb * pow(rim, _RimFalloff)) * _RimColor.a;
}