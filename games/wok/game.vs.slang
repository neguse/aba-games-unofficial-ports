struct Rect { float4 bounds; float4 uv; float4 color; };
StructuredBuffer<Rect> rectangles : register(t1);
struct Output { float4 position : SV_Position; float3 uv : TEXCOORD0; float4 color : COLOR0; };
[shader("vertex")]
Output vs_main(uint id : SV_VertexID) {
    float2 corners[6] = { float2(0,0),float2(1,0),float2(0,1),float2(0,1),float2(1,0),float2(1,1) };
    Rect r = rectangles[id/6]; float2 p = corners[id%6];
    Output o; o.position = float4(r.bounds.xy+p*r.bounds.zw,0,1);
    o.uv = float3(r.uv.xy+p*r.uv.zw,r.uv.x); o.color = r.color; return o;
}
