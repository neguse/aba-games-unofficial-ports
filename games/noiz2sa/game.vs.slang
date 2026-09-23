struct Rect { float4 bounds; float4 uv; float4 operation; };
StructuredBuffer<Rect> rectangles : register(t5);
struct Output { float4 position : SV_Position; float4 data : COLOR0; };
[shader("vertex")]
Output vs_main(uint index : LUB_VERTEX_ID) {
    Rect rect=rectangles[index/6];
    uint corner=index%6;
    float2 p=float2(corner==1||corner==2||corner==4?1:0,corner>=2&&corner!=3?1:0);
    Output output;
    output.position=float4(rect.bounds.xy+p*rect.bounds.zw,0,1);
    output.data=float4(rect.uv.xy+p*rect.uv.zw,rect.operation.x,0);
    return output;
}
