//
// GPGPU kernels for Streamline.
//
// The texture buffer contains the position (x,y,z) and the life (w).
//
Shader "Hidden/Kvant/Streamline/Kernels"
{
    Properties
    {
        _MainTex        ("-", 2D)       = ""{}
        _EmitterPos     ("-", Vector)   = (0, 0, 20, 0)
        _EmitterSize    ("-", Vector)   = (40, 40, 40, 0)
        _Direction      ("-", Vector)   = (0, 0, -1, 0.2)
        _SpeedParams    ("-", Vector)   = (5, 10, 0, 0)
        _NoiseParams    ("-", Vector)   = (0.2, 0.1, 1)
        _Config         ("-", Vector)   = (1, 2, 0, 0)
    }

    CGINCLUDE

    #pragma multi_compile NOISE_OFF NOISE_ON

    #include "UnityCG.cginc"
    #include "ClassicNoise3D.cginc"

    sampler2D _MainTex;

    float3 _EmitterPos;
    float3 _EmitterSize;
    float4 _Direction;
    float2 _SpeedParams;
    float4 _NoiseParams;    // (frequency, speed, animation)
    float4 _Config;         // (throttle, life, random seed, dT)

    // PRNG function.
    float nrand(float2 uv, float salt)
    {
        uv += float2(salt, _Config.z);
        return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
    }

    // Get a new particle.
    float4 new_particle(float2 uv)
    {
        float t = _Time.x;

        // Random position.
        float3 p = float3(nrand(uv, t + 1), nrand(uv, t + 2), nrand(uv, t + 3));
        p = (p - float3(0.5)) * _EmitterSize + _EmitterPos;

        // Life.
        float l = _Config.y * (0.5 + nrand(uv, t + 0));

        // Throttling: discard the particle emission by adding offset.
        float4 offs = float4(1e10, 1e10, 1e10, -1e10) * (uv.x > _Config.x);

        return float4(p, l) + offs;
    }

    // Position dependant velocity field.
    float3 get_velocity(float3 p, float2 uv)
    {
        // Random vector.
        float3 v = float3(nrand(uv, 4), nrand(uv, 5), nrand(uv, 6));
        v = (v - float3(0.5)) * 2;

        // Apply the spread parameter.
        v = lerp(_Direction.xyz, v, _Direction.w);

        // Apply the speed parameter.
        v = normalize(v) * lerp(_SpeedParams.x, _SpeedParams.y, nrand(uv, 7));

#ifdef NOISE_ON
        // Add noise vector.
        p = (p + _Time.y * _NoiseParams.z) * _NoiseParams.x;
        float nx = cnoise(p + float3(138.2, 0, 0));
        float ny = cnoise(p + float3(0, 138.2, 0));
        float nz = cnoise(p + float3(0, 0, 138.2));
        v += float3(nx, ny, nz) * _NoiseParams.y;
#endif
        return v;
    }

    // Pass0: initialization
    float4 frag_init(v2f_img i) : SV_Target 
    {
        return new_particle(i.uv);
    }

    // Pass1: update
    float4 frag_update(v2f_img i) : SV_Target 
    {
        float4 p = tex2D(_MainTex, i.uv);

        if (p.w > 0)
        {
            float dt = _Config.w;

            // Move along the velocity field.
            p.xyz += get_velocity(p.xyz, i.uv) * dt;

            // Decrement the life.
            p.w -= dt;

            return p;
        }
        else
        {
            return new_particle(i.uv);
        }
    }

    ENDCG

    SubShader
    {
        // Pass0: initialization
        Pass
        {
            Fog { Mode off }    
            CGPROGRAM
            #pragma target 3.0
            #pragma glsl
            #pragma vertex vert_img
            #pragma fragment frag_init
            ENDCG
        }
        // Pass1: update
        Pass
        {
            Fog { Mode off }    
            CGPROGRAM
            #pragma target 3.0
            #pragma glsl
            #pragma vertex vert_img
            #pragma fragment frag_update
            ENDCG
        }
    }
}
