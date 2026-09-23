Texture2D<float4> atlas : register(t0);
struct Input { float4 position : SV_Position; float3 uv : TEXCOORD0; float4 color : COLOR0; };
[shader("fragment")]
float4 fs_main(Input input) : SV_Target {
    if (input.uv.z < 0) return input.color;
    float4 color = atlas.Load(int3(int2(input.uv.xy),0));
    if (color.a < 0.5) discard;
    return color;
}
