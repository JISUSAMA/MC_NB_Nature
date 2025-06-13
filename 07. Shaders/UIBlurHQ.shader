// File: UIBlurURP.shader
// Purpose: Converted version of Built-in UIBlurHQ shader to URP-compatible version without GrabPass

Shader "UI/URP/UIBlurURP"
{
    Properties
    {
        _MainTex ("MainTex", 2D) = "white" {}
        _Size ("Blur Size", Range(0, 20)) = 5
        _TintColor ("Tint Color", Color) = (1,1,1,0.2)
        _Vibrancy ("Vibrancy", Range(0, 2)) = 0.2
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
        LOD 100

        Pass
        {
            Name "HorizontalBlur"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize;
            float _Size;

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

            Varyings vert (Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 frag (Varyings i) : SV_Target
            {
                float2 uv = i.uv;
                half4 sum = half4(0,0,0,0);

                sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(-4, 0) * _MainTex_TexelSize.xy * _Size) * 0.05;
                sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(-3, 0) * _MainTex_TexelSize.xy * _Size) * 0.09;
                sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(-2, 0) * _MainTex_TexelSize.xy * _Size) * 0.12;
                sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(-1, 0) * _MainTex_TexelSize.xy * _Size) * 0.15;
                sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv)                                            * 0.18;
                sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(1, 0) * _MainTex_TexelSize.xy * _Size) * 0.15;
                sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(2, 0) * _MainTex_TexelSize.xy * _Size) * 0.12;
                sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(3, 0) * _MainTex_TexelSize.xy * _Size) * 0.09;
                sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(4, 0) * _MainTex_TexelSize.xy * _Size) * 0.05;

                return sum;
            }
            ENDHLSL
        }

        Pass
        {
            Name "VerticalBlur"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize;
            float _Size;

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

            Varyings vert (Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 frag (Varyings i) : SV_Target
            {
                float2 uv = i.uv;
                half4 sum = half4(0,0,0,0);

                sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(0, -4) * _MainTex_TexelSize.xy * _Size) * 0.05;
                sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(0, -3) * _MainTex_TexelSize.xy * _Size) * 0.09;
                sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(0, -2) * _MainTex_TexelSize.xy * _Size) * 0.12;
                sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(0, -1) * _MainTex_TexelSize.xy * _Size) * 0.15;
                sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv)                                            * 0.18;
                sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(0,  1) * _MainTex_TexelSize.xy * _Size) * 0.15;
                sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(0,  2) * _MainTex_TexelSize.xy * _Size) * 0.12;
                sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(0,  3) * _MainTex_TexelSize.xy * _Size) * 0.09;
                sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(0,  4) * _MainTex_TexelSize.xy * _Size) * 0.05;

                return sum;
            }
            ENDHLSL
        }
    }

    FallBack Off
}
