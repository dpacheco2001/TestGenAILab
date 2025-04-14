Shader "Custom/UIBlurredBackground"
{
    Properties
    {
        // Referencia a nuestra Render Texture borrosa
        _BlurredTex ("Blurred Background (Render Texture)", 2D) = "black" {}
        // Tinte opcional para el efecto vidrio
        _Color ("Tint Color", Color) = (1,1,1,0.5) // Blanco semi-transparente por defecto
        // Textura opcional de overlay (como tu WindowGlass)
        _OverlayTex ("Overlay Texture (Optional)", 2D) = "white" {}

        // Propiedades necesarias para UI (Stencil para máscaras, etc.)
        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent" // Renderizar con los objetos transparentes
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "PreviewType"="Plane" // Para que funcione bien con UI
            "CanUseSpriteAtlas"="True" // Para que funcione bien con UI
        }

        // Necesario para máscaras de UI
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off // Generalmente no se necesita Culling para UI 2D
        Lighting Off // No necesita iluminación
        ZWrite Off // No escribe en el Z-Buffer (importante para transparencia)
        ZTest [unity_GUIZTestMode] // Usa el modo de ZTest estándar para UI
        Blend SrcAlpha OneMinusSrcAlpha // Mezcla alfa estándar
        ColorMask [_ColorMask] // Para máscaras de UI

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0 // Compatible con muchas plataformas

            #include "UnityCG.cginc"
            #include "UnityUI.cginc" // Importante para UI y máscaras

            sampler2D _BlurredTex;
            float4 _BlurredTex_TexelSize; // Necesario si haces offsets, pero no aquí
            sampler2D _OverlayTex;
            fixed4 _Color; // Tinte

            // Estructura del vértice para UI
            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0; // UVs estándar de la UI
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            // Estructura para pasar datos del vértice al fragmento
            struct v2f
            {
                float4 vertex   : SV_POSITION; // Posición en pantalla (Clip Space)
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0; // UVs estándar
                float4 worldPosition : TEXCOORD1; // Posición en el mundo
                float4 screenPos : TEXCOORD2; // Posición proyectada en pantalla
                UNITY_VERTEX_OUTPUT_STEREO // Para VR/Stereo
            };

            v2f vert(appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); // Para VR/Stereo

                o.worldPosition = v.vertex; // Guardamos la posición original
                o.vertex = UnityObjectToClipPos(v.vertex); // Proyectamos a pantalla
                o.texcoord = v.texcoord; // Pasamos las UVs estándar
                o.color = v.color * _Color; // Multiplicamos color del vértice por el tinte

                // Calculamos la posición en pantalla para muestrear la RT
                // ComputeScreenPos necesita hacerse después de la proyección
                o.screenPos = ComputeScreenPos(o.vertex);

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Calculamos las UVs correctas para la Render Texture
                // Dividimos xy por w para corregir la perspectiva
                float2 screenUV = i.screenPos.xy / i.screenPos.w;

                // Muestreamos la textura de fondo borrosa usando las UVs de pantalla
                fixed4 blurredBackground = tex2D(_BlurredTex, screenUV);

                // (Opcional) Muestreamos la textura overlay usando las UVs normales de UI
                fixed4 overlay = tex2D(_OverlayTex, i.texcoord);

                // Combinamos el fondo borroso con el tinte de la UI
                // Puedes experimentar con diferentes formas de combinar
                // 1. Solo fondo borroso tintado:
                // fixed4 finalColor = blurredBackground * i.color;

                // 2. Fondo borroso tintado, mezclado con overlay usando el alfa del overlay:
                fixed4 tintedBlur = blurredBackground * i.color;
                fixed4 finalColor = lerp(tintedBlur, overlay * i.color, overlay.a); // Mezcla basada en alfa de overlay

                // 3. Solo fondo tintado, usando el alfa del tinte para transparencia general
                // fixed4 finalColor = blurredBackground * i.color;
                // finalColor.a = i.color.a; // Usa el alfa del tinte (_Color)

                // Aseguramos que el alfa final respete el alfa del tinte (_Color)
                finalColor.a = i.color.a;

                // Aplicar recorte de máscara de UI si es necesario
                // finalColor.a *= UnityGetUIDrawOperationMask(); // Puede no ser necesario si el Stencil funciona

                return finalColor;
            }
            ENDCG
        }
    }
}