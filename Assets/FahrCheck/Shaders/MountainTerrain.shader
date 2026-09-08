Shader "FahrCheck/Mountain Terrain"
{
    Properties
    {
        _GrassTex ("Meadow", 2D) = "white" {}
        _RockTex ("Rock", 2D) = "white" {}
        _SnowTex ("Snow", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 200
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows addshadow vertex:vert
        #pragma target 3.0

        sampler2D _GrassTex;
        sampler2D _RockTex;
        sampler2D _SnowTex;

        struct Input
        {
            float2 uv_GrassTex;
            float3 weights;
        };

        void vert(inout appdata_full vertex, out Input output)
        {
            UNITY_INITIALIZE_OUTPUT(Input, output);
            output.weights = vertex.color.rgb;
        }

        void surf(Input input, inout SurfaceOutputStandard output)
        {
            float3 weights = max(input.weights, 0.001);
            weights /= weights.x + weights.y + weights.z;
            float3 meadow = tex2D(_GrassTex, input.uv_GrassTex).rgb;
            float3 rock = tex2D(_RockTex, input.uv_GrassTex * 0.76).rgb;
            float3 snow = tex2D(_SnowTex, input.uv_GrassTex * 0.52).rgb;
            output.Albedo = meadow * weights.x + rock * weights.y + snow * weights.z;
            output.Smoothness = weights.z * 0.12;
            output.Metallic = 0;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
