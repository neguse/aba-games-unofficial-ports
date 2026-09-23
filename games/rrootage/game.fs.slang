Texture2D<float4> atlas;
SamplerState atlasSampler;
struct Input { float4 position : SV_Position; float4 color : COLOR0; };
[shader("fragment")]
float4 fs_main(Input input) : SV_Target {
    if (input.color.w >= 0) return float4(input.color.rgb * input.color.w, 1);
    uint packed = (uint)input.color.z;
    float3 tint = float3(packed & 255, (packed >> 8) & 255, (packed >> 16) & 255) / 255;
    return float4(atlas.SampleLevel(atlasSampler, input.color.xy, 0).rgb * tint, 1);
}
