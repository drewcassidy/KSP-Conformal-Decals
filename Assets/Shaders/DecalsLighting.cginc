#ifndef DECALS_LIGHTING_INCLUDED
#define DECALS_LIGHTING_INCLUDED

// modifed version of the KSP BlinnPhong because it does some weird things
inline fixed4 LightingBlinnPhongDecal(SurfaceOutput s, fixed3 lightDir, half3 viewDir, fixed atten)
{
    s.Normal = normalize(s.Normal);
    half3 h = normalize(lightDir + viewDir);

    fixed diff = max(0, dot(s.Normal, lightDir));

    float nh = max(0, dot(s.Normal, h));
    float spec = pow(nh, s.Specular*128.0) * s.Gloss;

    fixed4 c = 0;
    c.rgb = (s.Albedo * _LightColor0.rgb * diff + _LightColor0.rgb * _SpecColor.rgb * spec) * (atten);
    return c;
}

// KSP underwater fog function
float4 UnderwaterFog(float3 worldPos, float3 color)
{
    float3 toPixel = worldPos - _LocalCameraPos.xyz;
    float toPixelLength = length(toPixel);

    float underwaterDetection = _UnderwaterFogFactor * _LocalCameraDir.w;
    float albedoLerpValue = underwaterDetection * (_UnderwaterMaxAlbedoFog * saturate(toPixelLength * _UnderwaterAlbedoDistanceScalar));
    float alphaFactor = 1 - underwaterDetection * (_UnderwaterMaxAlphaFog * saturate((toPixelLength - _UnderwaterMinAlphaFogDistance) * _UnderwaterAlphaDistanceScalar));

    return float4(lerp(color, _UnderwaterFogColor.rgb, albedoLerpValue), alphaFactor);
}

#endif
