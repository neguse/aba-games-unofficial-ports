Texture2D<float4> titleImage : register(t3);
StructuredBuffer<float4> titleParameters : register(t4);
float4 titlePixel(int2 p, int2 size, int offset) {
    p = (p % size + size) % size;
    return titleImage.Load(int3(p.x, p.y + offset, 0));
}
float4 sampleTitle(float2 uv, float2 dx, float2 dy) {
    int2 size = int2(titleParameters[1].xy);
    float rho = max(length(dx * size), length(dy * size));
    int level = int(clamp(floor(log2(max(rho, 1.0)) + 0.5), 0.0, titleParameters[1].z));
    int offset = 0;
    for (int i = 0; i < level; i++) { offset += size.y; size = max(size / 2, int2(1, 1)); }
    float2 position = uv * size - 0.5;
    int2 p = int2(floor(position));
    float2 f = frac(position);
    float4 upper = lerp(titlePixel(p, size, offset), titlePixel(p + int2(1, 0), size, offset), f.x);
    float4 lower = lerp(titlePixel(p + int2(0, 1), size, offset), titlePixel(p + int2(1, 1), size, offset), f.x);
    float4 color = lerp(upper, lower, f.y) * titleParameters[0];
    if (titleParameters[1].w > 0) color.rgb *= color.a;
    return color;
}
struct Input { float4 position : SV_Position; float4 color : COLOR0; };
[shader("fragment")]
float4 fs_main(Input input) : SV_Target {
    float2 dx = ddx(input.color.xy), dy = ddy(input.color.xy);
    if (input.color.w < -1.5) return sampleTitle(input.color.xy, dx, dy);
    return input.color;
}
