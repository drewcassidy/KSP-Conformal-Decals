Shader "ConformalDecals/Text Blit"
{
    Properties
    {
        _MainTex("_MainTex (RGB spec(A))", 2D) = "white" {}

    }
    SubShader
    {
        Tags { "Queue" = "Transparent" }
        Cull Off
        ZWrite Off 
        
        Pass
        {
            BlendOp Max

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            sampler2D _MainTex;

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(float4 vertex : POSITION, float2 uv : TEXCOORD0) {
                v2f o;
                o.pos = UnityObjectToClipPos(vertex);
                o.uv = uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                fixed4 c = 0;

                c.r = tex2D(_MainTex,(i.uv)).a;

                return c;
            }

            ENDCG
        } 
    }
}    