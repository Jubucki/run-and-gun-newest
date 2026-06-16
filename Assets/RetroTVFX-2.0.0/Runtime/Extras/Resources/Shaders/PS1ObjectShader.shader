Shader "Retro/RetroDiffuse"
{
    Properties
    {
        _Color("Color", COLOR) = (1,1,1,1)
        _AmbientColor("Ambient", COLOR) = (1,1,1,1)
        _MainTex("Texture", 2D) = "white" {}
        _ResolutionX("Resolution X", Int) = 320
        _ResolutionY("Resolution Y", Int) = 240
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 100

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                half4 col : TEXCOORD3;
                float4 pos : TEXCOORD2;
                float fogFactor : TEXCOORD4;
                float3 worldPos : TEXCOORD5;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float4 _AmbientColor;
                int _ResolutionX;
                int _ResolutionY;
            CBUFFER_END

            v2f vert(appdata v)
            {
                v2f o;

                float4 clipPos = TransformObjectToHClip(v.vertex.xyz);

                float originalW = clipPos.w;

                float2 res = float2(_ResolutionX, _ResolutionY) * 0.5;
                res /= clipPos.w;
                clipPos.xy *= res;
                clipPos.xy = floor(clipPos.xy);
                clipPos.xy /= res;

                o.vertex = clipPos;
                o.pos = clipPos;
                o.pos.w = originalW;

                o.uv = TRANSFORM_TEX(v.uv, _MainTex) * originalW;

                o.fogFactor = ComputeFogFactor(clipPos.z);

                float3 worldNormal = TransformObjectToWorldNormal(v.normal);
                o.col = (dot(worldNormal, float3(0, 1, 0)) + _AmbientColor) * _Color;

                o.worldPos = TransformObjectToWorld(v.vertex.xyz);

                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv / i.pos.w;
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);

                // Schatten
                float4 shadowCoord = TransformWorldToShadowCoord(i.worldPos);
                Light mainLight = GetMainLight(shadowCoord);
                float shadow = mainLight.shadowAttenuation;

                col *= i.col * shadow;

                col.rgb = MixFog(col.rgb, i.fogFactor);

                return col;
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                float3 worldPos = TransformObjectToWorld(v.vertex.xyz);
                float3 worldNormal = TransformObjectToWorldNormal(v.normal);
                o.vertex = TransformWorldToHClip(ApplyShadowBias(worldPos, worldNormal, _LightDirection));
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }
}