Shader "Custom/EmissiveGlow" {
    Properties {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _EmissionColor ("Emission Color", Color) = (1,1,1,1)
        [Range(0,10)]
        _EmissionIntensity ("Emission Intensity", Float) = 1
    }

    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Indica que es un Surface Shader con modelo de iluminación Standard
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
        float4 _Color;
        float4 _EmissionColor;
        float _EmissionIntensity;

        struct Input {
            float2 uv_MainTex;
        };

        void surf (Input IN, inout SurfaceOutputStandard o) {
            // Tomamos el color base (Albedo) multiplicado por _Color
            half4 albedo = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = albedo.rgb;
            o.Metallic = 0.0;
            o.Smoothness = 0.0;

            // Emisión : se suma al color base sin necesitar post-processing
            o.Emission = _EmissionColor.rgb * _EmissionIntensity;
        }
        ENDCG
    }

    FallBack "Diffuse"
}