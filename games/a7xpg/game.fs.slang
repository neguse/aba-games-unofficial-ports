Texture2D<float4> glow;
SamplerState glowSampler;
struct Input { float4 position : SV_Position; float4 color : COLOR0; };
[shader("fragment")]
float4 fs_main(Input input) : SV_Target {
    if (input.color.w >= 0) return input.color;
    return glow.SampleLevel(glowSampler, input.color.xy, 0) * float4(0.5, 0.4, 0.45, 0);
}
