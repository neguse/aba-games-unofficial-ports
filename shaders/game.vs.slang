struct Vertex { float4 position; float4 color; };
StructuredBuffer<Vertex> verts;
struct Output { float4 position : SV_Position; float4 color : COLOR0; };
[shader("vertex")]
Output vs_main(uint index : LUB_VERTEX_ID) {
    Output output;
    output.position = verts[index].position;
    output.color = verts[index].color;
    return output;
}
