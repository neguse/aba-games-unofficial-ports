Texture2D<float4> titleImage : register(t3);
StructuredBuffer<float4> titleParameters : register(t4);
SamplerState titleImage_smp : register(s5);
Texture2D<float4> glow;
SamplerState glow_smp;
struct Input { float4 position : SV_Position; float4 color : COLOR0; };
[shader("fragment")]
float4 fs_main(Input input) : SV_Target {
    if (input.color.w < -1.5) {
        float4 color = titleImage.SampleLevel(titleImage_smp, input.color.xy, 0) * titleParameters[0];
        if (titleParameters[1].w > 0) color.rgb *= color.a;
        return color;
    }
    if (input.color.w >= 0) return input.color;
    return glow.SampleLevel(glow_smp, input.color.xy, 0) * float4(0.5, 0.4, 0.45, 0);
}
