// https://github.com/nuonical/unity-holographic-shader/
Shader "Custom/Holographic"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,0,0,1)
        _Distance ("Distance", Float) = 100
        _Scale ("Scale", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "ColorMask"="RGBA" }
        LOD 100

        Pass
        {
            ZTest Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _Distance;
            float _Scale;
            float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                // Calculate lens origin and direction vectors in view space
                float3 lens_origin = UnityObjectToViewPos(float3(0,0,0));
                float3 p0 = UnityObjectToViewPos(float3(0,0, _Distance));
                float3 n = UnityObjectToViewPos(float3(0,0, 1)) - lens_origin;
                float3 uDir = UnityObjectToViewPos(float3(1,0,0)) - lens_origin;
                float3 vDir = UnityObjectToViewPos(float3(0,1,0)) - lens_origin;
                float3 vert = UnityObjectToViewPos(v.vertex);

                // Project the vertex position onto the lens plane
                float a = dot(p0, n) / dot(vert, n);
                float3 vert_prime = a * vert;

                // Convert the vertex position back to clip space
                o.vertex = UnityObjectToClipPos(v.vertex);

                // Calculate UV coordinates based on projected vertex position
                o.uv = float2(dot(vert_prime - p0, uDir), dot(vert_prime - p0, vDir));
                o.uv = o.uv / (_Scale * _Distance);

                // Center the UVs around the lens center
                o.uv += float2(0.5, 0.5);

                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                // Sample the texture
                fixed4 tex = tex2D(_MainTex, i.uv);
                fixed4 col = _Color;
                col.a = tex.r;

                // Clamp UVs to restrict rendering within the lens
                if(i.uv.x < 0 || i.uv.x > 1 || i.uv.y < 0 || i.uv.y > 1) 
                {
                    col.a = 0;
                }

                // Apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
