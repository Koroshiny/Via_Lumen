Shader "Hidden/CinematicLitFog"
{
    Properties
    {
        _FogColor ("Fog Color", Color) = (0.65, 0.75, 0.85, 1)

        _FogStart ("Fog Start", Float) = 40
        _FogEnd ("Fog End", Float) = 220

        _FogIntensity ("Fog Intensity", Range(0,1)) = 0.35

        // Размер шума
        _NoiseScale ("Noise Scale", Float) = 0.05

        // Насколько noise влияет на форму тумана
        _NoiseStrength ("Noise Strength", Range(0,1)) = 0.2

        // Скорость движения
        _FogSpeed ("Fog Speed", Float) = 0.01

        [HDR]_LightColor ("Light Color", Color) = (1,1,1,1)
        _LightIntensity ("Light Intensity", Range(0,5)) = 1
        _LightScattering ("Light Scattering", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            Name "CinematicLitFog"

            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            TEXTURE2D(_BlitTexture);
            SAMPLER(sampler_BlitTexture);

            struct Attributes
            {
                uint vertexID : SV_VertexID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float4 _FogColor;

            float _FogStart;
            float _FogEnd;

            float _FogIntensity;

            float _NoiseScale;
            float _NoiseStrength;

            float _FogSpeed;

            float4 _LightColor;
            float _LightIntensity;
            float _LightScattering;

            Varyings vert(Attributes input)
            {
                Varyings output;

                output.uv = float2(
                    (input.vertexID << 1) & 2,
                    input.vertexID & 2
                );

                output.positionCS = float4(
                    output.uv * 2.0 - 1.0,
                    0.0,
                    1.0
                );

                output.uv.y = 1.0 - output.uv.y;

                return output;
            }

            float hash(float2 p)
            {
                return frac(
                    sin(dot(p, float2(127.1, 311.7)))
                    * 43758.5453123
                );
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);

                float a = hash(i);
                float b = hash(i + float2(1,0));
                float c = hash(i + float2(0,1));
                float d = hash(i + float2(1,1));

                float2 u = f * f * (3.0 - 2.0 * f);

                return lerp(a, b, u.x)
                    + (c - a) * u.y * (1.0 - u.x)
                    + (d - b) * u.x * u.y;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float4 sceneColor = SAMPLE_TEXTURE2D(
                    _BlitTexture,
                    sampler_BlitTexture,
                    input.uv
                );

                float rawDepth = SampleSceneDepth(input.uv);

                // Не применяем туман к небу
                if (rawDepth >= 0.9999)
                {
                    return sceneColor;
                }

                float linearDepth = LinearEyeDepth(
                    rawDepth,
                    _ZBufferParams
                );

                float3 worldPos = ComputeWorldSpacePosition(
                    input.uv,
                    rawDepth,
                    UNITY_MATRIX_I_VP
                );

                // Distance fog
                float fogFactor = smoothstep(
                    _FogStart,
                    _FogEnd,
                    linearDepth
                );

                // Pseudo-3D noise
                // Y добавляет variation по высоте,
                // чтобы noise не превращался в вертикальные полосы
                float2 noiseUV =
                    (
                        worldPos.xz +
                        worldPos.y * 0.25
                    )
                    * _NoiseScale
                    + _Time.y * _FogSpeed;

                float fogNoise = noise(noiseUV);

                // Более мягкий noise
                fogFactor *= lerp(
                    1.0,
                    saturate(0.5 + fogNoise),
                    _NoiseStrength
                );

                fogFactor *= _FogIntensity;

                // Main directional light
                Light mainLight = GetMainLight();

                // От пикселя к камере
                float3 viewDir =
                    normalize(
                        _WorldSpaceCameraPos - worldPos
                    );

                // Насколько камера смотрит в сторону света
                float lightDot = saturate(
                    dot(viewDir, -mainLight.direction)
                );

                // Cinematic scattering
                float scattering = pow(
                    lightDot,
                    lerp(1, 16, _LightScattering)
                );

                // Подсвеченный туман
                float3 litFog =
                    _FogColor.rgb +
                    scattering *
                    _LightColor.rgb *
                    _LightIntensity *
                    mainLight.color;

                // Финальный blend
                sceneColor.rgb = lerp(
                    sceneColor.rgb,
                    litFog,
                    fogFactor
                );

                return sceneColor;
            }

            ENDHLSL
        }
    }
}