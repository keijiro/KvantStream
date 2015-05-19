//
// Line shader for Streamline particles.
//
// POSITION : [buffer switch (0 or 1), 0, 0, 0]
// TEXCOORD0: [u, v, 0, 0]
//
Shader "Hidden/Kvant/Streamline/Line"
{
    Properties
    {
        _PositionTex1   ("-", 2D)       = ""{}
        _PositionTex2   ("-", 2D)       = ""{}
        _Color          ("-", Color)    = (1, 1, 1, 1)
        _Options        ("-", Vector)   = (1, 1, 0, 0)
    }

    CGINCLUDE

    #include "UnityCG.cginc"

    struct appdata
    {
        float4 position : POSITION;
        float2 texcoord : TEXCOORD0;
    };

    struct v2f
    {
        float4 position : SV_POSITION;
        float4 color : COLOR;
    };

    sampler2D _PositionTex1;
    float4 _PositionTex1_TexelSize;

    sampler2D _PositionTex2;
    float4 _PositionTex2_TexelSize;

    half4 _Color;
    float2 _Options; // (tail, color amp)

    v2f vert(appdata v)
    {
        v2f o;

        float2 uv = v.texcoord.xy + _PositionTex1_TexelSize.xy / 2;

        float4 p1 = tex2D(_PositionTex1, uv);
        float4 p2 = tex2D(_PositionTex2, uv);
        float sw = v.position.x;

        if (p1.w < 0)
        {
            o.position = mul(UNITY_MATRIX_MVP, float4(p2.xyz, 1));
        }
        else
        {
            float3 p = lerp(p2.xyz, p1.xyz, (1.0 - sw) * _Options.x);
            o.position = mul(UNITY_MATRIX_MVP, float4(p, 1));
        }

        o.color = _Color * _Options.y;
        o.color.a *= sw;

        return o;
    }

    half4 frag(v2f i) : COLOR
    {
        return i.color;
    }

    ENDCG

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma target 3.0
            #pragma glsl
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
    } 
}
