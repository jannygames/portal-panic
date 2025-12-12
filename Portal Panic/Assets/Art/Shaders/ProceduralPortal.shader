Shader "Universal Render Pipeline/PortalUnlit"
{
    Properties
    {
        _MainTex("Base Texture", 2D) = "white" {}
        _Color("Tint Color", Color) = (0.2, 0.6, 1, 1)
        _ScrollSpeed("Scroll Speed", Float) = 0.5
        _DistortionTex("Distortion Noise", 2D) = "gray" {}
        _DistortionStrength("Distortion Strength", Range(0,1)) = 0.2
        _FresnelColor("Fresnel Glow Color", Color) = (0.5, 0.8, 1, 1)
        _FresnelPower("Fresnel Power", Range(1,8)) = 3
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        LOD 200
        Blend SrcAlpha One
        ZWrite Off
        Cull Off

        Pass
        {
            Name "PortalPass"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 viewDirWS : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_DistortionTex);
            SAMPLER(sampler_DistortionTex);

            float4 _Color;
            float _ScrollSpeed;
            float _DistortionStrength;
            float4 _FresnelColor;
            float _FresnelPower;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.viewDirWS = normalize(GetWorldSpaceViewDir(IN.positionOS.xyz));
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Scroll base UVs
                float2 uv = IN.uv;
                uv.y += _Time.y * _ScrollSpeed;

                // Distortion
                float2 distortionUV = IN.uv * 2.0;
                float2 noise = SAMPLE_TEXTURE2D(_DistortionTex, sampler_DistortionTex, distortionUV).rg;
                uv += (noise - 0.5) * _DistortionStrength;

                // Base texture
                half4 baseCol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv) * _Color;

                // Fresnel glow
                float fresnel = pow(1.0 - saturate(dot(IN.viewDirWS, float3(0,0,1))), _FresnelPower);
                half4 fresnelCol = _FresnelColor * fresnel;

                // Combine
                half4 finalCol = baseCol + fresnelCol;
                finalCol.a = saturate(baseCol.a + fresnel); // alpha for transparency

                return finalCol;
            }
            ENDHLSL
        }
    }
}