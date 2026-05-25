Shader "Hidden/LightShafts"
{
    Properties
    {
        _Intensity ("Intensity", Range(0,3)) = 1
        _Density ("Density", Range(0,5)) = 1
        _Decay ("Decay", Range(0,1)) = 0.96
        _Weight ("Weight", Range(0,1)) = 0.8
        _Samples ("Samples", Range(4,32)) = 16
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            float4 _LightPos;

            float _Intensity;
            float _Density;
            float _Decay;
            float _Weight;
            float _Samples;

            half4 frag(Varyings i) : SV_Target
            {
                float2 uv = i.texcoord;

                float3 scene = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv).rgb;

                float2 dir = (uv - _LightPos.xy) * _Density / max(_Samples, 1);

                float3 col = 0;
                float decay = 1;

                float2 p = uv;

                [loop]
                for (int s = 0; s < 32; s++)
                {
                    if (s >= _Samples) break;

                    p -= dir;

                    float depth = SampleSceneDepth(p);

                    // occlusion: если есть геометрия ближе чем sky
                    float occlusion = step(depth, 0.9999);

                    float3 sample = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, p).rgb;

                    col += sample * decay * _Weight * occlusion;

                    decay *= _Decay;
                }

                float3 rays = col * _Intensity;

                return float4(scene + rays, 1);
            }

            ENDHLSL
        }
    }
}