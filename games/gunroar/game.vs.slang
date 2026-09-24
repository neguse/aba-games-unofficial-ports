struct Vertex { float4 position; float4 color; };
struct Draw {
    float4 range; // Output start, mesh start, mesh vertex count, topology.
    float4 options; // Retained mesh, line width, additive alpha, image.
    float4 model0, model1, model2, model3;
    float4 projection;
    float4 viewport;
    float4 color;
};
StructuredBuffer<Vertex> staticVertices : register(t2);
StructuredBuffer<Vertex> dynamicVertices : register(t3);
StructuredBuffer<Draw> draws : register(t4);
struct Output { float4 position : SV_Position; float4 color : COLOR0; nointerpolation uint premultiply : TEXCOORD1; };
Vertex vertexAt(Draw draw, uint index) {
    uint address = uint(draw.range.y) + index;
    return draw.options.x > 0 ? staticVertices[address] : dynamicVertices[address];
}
float4 project(Draw draw, float3 p) {
    float3 v = draw.model0.xyz * p.x + draw.model1.xyz * p.y + draw.model2.xyz * p.z + draw.model3.xyz;
    float4 result;
    if (draw.projection.z > 0) result = float4(v.x / 320 - 1, 1 - v.y / 240, (1 - v.z) / 2, 1);
    else {
        float far = draw.projection.y;
        result = float4(v.x * draw.projection.x, v.y * (4.0 / 3.0) * draw.projection.x,
            -v.z * (far / (far - 0.1)) - far * 0.1 / (far - 0.1), -v.z);
    }
    result.x = result.x * draw.projection.w + (draw.projection.w - 1) * result.w;
    return result;
}
float4 colorAt(Draw draw, Vertex v) {
    return saturate(v.position.w > 0 ? draw.color : v.color);
}
[shader("vertex")]
Output vs_main(uint index : LUB_VERTEX_ID) {
    uint count, stride;
    draws.GetDimensions(count, stride);
    uint low = 0, high = count;
    // Commands remain in submission order, including overlapping alpha geometry.
    while (low + 1 < high) {
        uint middle = (low + high) / 2;
        if (draws[middle].range.x <= index) low = middle;
        else high = middle;
    }
    Draw draw = draws[low];
    uint local = index - uint(draw.range.x);
    uint mode = uint(draw.range.w), n = uint(draw.range.z);
    Output output;
    output.premultiply = uint(draw.options.z);
    if (mode == 3 || mode == 4 || mode == 10) {
        uint segment = local / 6, corner = local % 6;
        uint a = mode == 3 ? segment * 2 : segment;
        uint b = (a + 1) % n;
        Vertex va = vertexAt(draw, a), vb = vertexAt(draw, b);
        float4 pa = project(draw, va.position.xyz), pb = project(draw, vb.position.xyz);
        float2 delta = (pb.xy / pb.w - pa.xy / pa.w) * draw.viewport.xy / 2;
        float length = sqrt(dot(delta, delta));
        if (pa.w <= 0 || pb.w <= 0 || length == 0) {
            output.position = float4(0, 0, 0, 1); output.color = 0;
            return output;
        }
        float2 offset = float2(-delta.y, delta.x) / length * draw.options.y / draw.viewport.xy;
        bool end = corner == 2 || corner == 3 || corner == 5;
        float sign = corner == 1 || corner == 4 || corner == 5 ? -1 : 1;
        output.position = end ? pb : pa;
        output.position.xy += offset * sign * output.position.w;
        output.color = colorAt(draw, end ? vb : va);
        return output;
    }
    uint vertex = local;
    if (mode == 0) {
        uint corner = local % 6;
        vertex = local / 6 * 4 + (corner < 3 ? corner : corner == 3 ? 0 : corner - 2);
    } else if (mode == 2) {
        uint triangle = local / 3, corner = local % 3;
        vertex = triangle + (corner == 2 ? 2 : corner == 0 ? triangle % 2 : 1 - triangle % 2);
    } else if (mode == 9) {
        uint triangle = local / 3, corner = local % 3;
        vertex = corner == 0 ? 0 : triangle + corner;
    }
    Vertex v = vertexAt(draw, vertex);
    output.position = project(draw, v.position.xyz);
    if (draw.options.w > 0) {
        float2 uv = float2(vertex == 1 || vertex == 2 ? 1 : 0, vertex >= 2 ? 1 : 0);
        output.color = float4(uv, 0, -2);
    } else output.color = colorAt(draw, v);
    return output;
}
