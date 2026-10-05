struct Params { float4 r0; float4 r1; float4 r2; float4 r3; float4 tint; float4 options; };
ConstantBuffer<Params> u;
LUB_TEXTURE2D(screen);
struct Output { float4 position : SV_Position; float2 uv : TEXCOORD0; };
[shader("vertex")]
Output vs_main(uint id : LUB_VERTEX_ID) {
    const float2 uv[6] = {float2(0,0),float2(1,0),float2(0,1),float2(0,1),float2(1,0),float2(1,1)};
    Output o;
    o.uv = uv[id];
    float4 p = float4((o.uv.x-.5)*u.options.x,(.5-o.uv.y)*u.options.y,u.options.z,1);
    o.position = float4(dot(u.r0,p),dot(u.r1,p),dot(u.r2,p),dot(u.r3,p));
    return o;
}
[shader("fragment")]
float4 fs_main(Output i) : SV_Target {
    float4 c = LUB_SAMPLE(screen, i.uv) * u.tint;
    if (u.options.w > .5) c.rgb = select(c.rgb <= .04045, c.rgb / 12.92, pow((c.rgb+.055)/1.055,2.4));
    return c;
}
