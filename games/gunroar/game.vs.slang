struct Vertex { float4 position; float4 color; };
StructuredBuffer<Vertex> verts;
StructuredBuffer<float4> faces;
cbuffer Params : register(b0) { column_major float4x4 model; float4 tint; float4 options; float4 imageInfo; };
struct Output { float4 color : COLOR0; nointerpolation uint premultiply : TEXCOORD1; float4 position : SV_Position; };
[shader("vertex")]
Output vs_main(uint index : LUB_VERTEX_ID) {
    uint4 face = uint4(faces[index / 3 + uint(options.z)]);
    uint corner = index % 3;
    Output output;
    output.premultiply = uint(options.y);
    if (face.w == 1) {
        corner += face.z;
        Vertex a = verts[face.x], b = verts[face.y];
        float4 pa = mul(model, float4(a.position.xyz, 1)), pb = mul(model, float4(b.position.xyz, 1));
        float2 delta = (pb.xy / pb.w - pa.xy / pa.w) * float2(320, 240);
        float length = sqrt(dot(delta, delta));
        if (pa.w <= 0 || pb.w <= 0 || length == 0) {
            output.position = float4(0, 0, 0, 1); output.color = 0;
            return output;
        }
        float2 offset = float2(-delta.y, delta.x) / length * options.x / float2(640, 480);
        bool end = corner == 2 || corner == 3 || corner == 5;
        float sign = corner == 1 || corner == 4 || corner == 5 ? -1 : 1;
        output.position = end ? pb : pa;
        output.position.xy += offset * sign * output.position.w;
        output.color = saturate((end ? b.color : a.color) * tint);
    } else {
        Vertex v = verts[face[corner]];
        output.position = mul(model, float4(v.position.xyz, 1));
        output.color = options.w > 0 ? v.color : saturate(v.color * tint);
    }
    return output;
}
