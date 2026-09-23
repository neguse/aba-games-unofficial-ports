StructuredBuffer<float> spans : register(t1);
struct Output { float4 position : SV_Position; float4 color : COLOR0; };
[shader("vertex")]
Output vs_main(uint id : SV_VertexID) {
    float2 corners[6] = { float2(0,0),float2(1,0),float2(0,1),float2(0,1),float2(1,0),float2(1,1) };
    uint i=id/6*5; float2 xy=float2(spans[i],spans[i+1])+corners[id%6]*float2(spans[i+2],spans[i+3]);
    float c=spans[i+4]; Output o;
    o.position=float4(xy.x/320-1,1-xy.y/240,0,1);
    o.color=float4(c,c,c==0?96:c+floor(96*(255-c)/256),255)/255; return o;
}
