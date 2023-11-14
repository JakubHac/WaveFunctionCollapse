// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/WFCOutputShader"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo (RGB)", 2D) = "white" {}
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
        }

        CGPROGRAM
        //Notice the "vertex:vert" at the end of the next line
        #pragma target 5.0
        #pragma surface surf Standard fullforwardshadows vertex:vert

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        fixed4 _Color;
        #ifdef SHADER_API_D3D11
            uniform StructuredBuffer<float> HeightMap;
        #endif

        struct vertex_input {
            float4 vertex : POSITION;
            float3 normal : NORMAL;
            float4 texcoord : TEXCOORD0;
            uint id : SV_VertexID;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        void vert(inout vertex_input v, out Input o)
        {
            #ifdef SHADER_API_D3D11
                v.vertex.y = v.vertex.y + HeightMap[v.id];
            #endif
            UNITY_INITIALIZE_OUTPUT(Input, o);
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;

            o.Alpha = c.a;
        }
        ENDCG
    }


    FallBack "Diffuse"
}