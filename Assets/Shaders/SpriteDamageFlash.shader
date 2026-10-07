Shader "Redline/SpriteDamageFlash"
{
    // Shader de dano em tempo real para sprites 2D (URP).
    //
    // Substitui o placeholder anterior (troca de SpriteRenderer.color via script)
    // por um efeito de verdade calculado por pixel na GPU: o fragment shader
    // interpola (lerp) entre a cor normal do sprite e _FlashColor, de acordo com
    // _FlashAmount (0 = cor normal, 1 = totalmente na cor de flash). O script
    // Redline.Effects.DamageFlash anima _FlashAmount de 1 para 0 a cada dano
    // sofrido, usando um MaterialPropertyBlock (sem criar instâncias de material
    // por objeto).
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)
        _FlashColor ("Flash Color", Color) = (1, 1, 1, 1)
        _FlashAmount ("Flash Amount", Range(0, 1)) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "Universal2D"
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
                float4 color      : COLOR;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float4 color       : COLOR;
                float2 uv          : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float4 _FlashColor;
                float _FlashAmount;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.color = IN.color * _Color;
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                float4 baseColor = texColor * IN.color;

                // Efeito de dano: interpola a cor final em direção a _FlashColor
                // conforme _FlashAmount, mantendo o alpha original do sprite.
                float3 finalRGB = lerp(baseColor.rgb, _FlashColor.rgb, saturate(_FlashAmount));

                return float4(finalRGB, baseColor.a);
            }
            ENDHLSL
        }
    }

    Fallback "Sprites/Default"
}
