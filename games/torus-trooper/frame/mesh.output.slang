Texture2D<float4> scene;
struct Output { float4 position : SV_Position; };
[shader("vertex")]
Output vs_main(uint index : LUB_VERTEX_ID) {
    Output output;
    output.position = float4(index == 2 ? 3 : -1, index == 1 ? 3 : -1, 0, 1);
    return output;
}
[shader("fragment")]
float4 fs_main(Output input) : SV_Target {
    float3 color = scene.Load(int3(int2(input.position.xy), 0)).rgb;
    // The original blends display-encoded colors; decode only the finished image for the XR sRGB target.
    float3 linear = select(color <= 0.04045, color / 12.92, pow((color + 0.055) / 1.055, 2.4));
    return float4(linear, 1);
}
