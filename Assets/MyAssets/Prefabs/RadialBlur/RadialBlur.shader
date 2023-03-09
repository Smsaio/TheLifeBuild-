Shader "Hidden/Radial Blur"
{
    SubShader
    {
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            half _SampleCount;
            half _Strength;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 color = 0;
                half2 symmetryUv = IN.uv - 0.5;
                half distance = length(symmetryUv);
                half factor = _Strength / _SampleCount * distance;
                for (int i = 0; i < _SampleCount; i++)
                {
                    half uvOffset = 1 - factor * i;
                    color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, symmetryUv * uvOffset + 0.5);
                }
                color /= _SampleCount;
                return color;
            }
            ENDHLSL
        }
    }
}