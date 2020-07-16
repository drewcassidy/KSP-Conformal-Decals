#ifndef HSL_INCLUDED
#define HSL_INCLUDED

inline float3 HSL2RGB(float3 hsl) {
    int3 n = int3(0, 8, 4);
    float3 k = (n + hsl.x * 12) % 12;
    float a = hsl.y * min(hsl.z, 1 - hsl.z);
    return hsl.z - a * max(-1, min(k - 3, min(9 - k, 1)));
}

#endif