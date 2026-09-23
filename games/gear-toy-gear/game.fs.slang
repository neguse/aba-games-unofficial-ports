Texture2D<float4> surface : register(t2);
SamplerState surfaceSampler : register(s3);
struct Input {float4 position:SV_Position;float4 color:COLOR0;float depth:TEXCOORD0;float2 uv:TEXCOORD1;float3 normal:TEXCOORD2;nointerpolation uint mode:TEXCOORD3;};
[shader("fragment")]
float4 fs_main(Input input):SV_Target {
 uint mode=input.mode;
 if(mode==6)return surface.SampleLevel(surfaceSampler,input.uv,0);
 if(mode==7){
  float2 offsets[12]={float2(-.3,-.4),float2(-.8,-.1),float2(-.7,.5),float2(-.2,.6),float2(1,-.1),float2(.5,-.5),float2(.5,.8),float2(.2,-.9),float2(.5,.1),float2(.9,.4),float2(-.3,-1),float2(-.8,-.6)};
  float4 sum=surface.SampleLevel(surfaceSampler,input.uv,0);for(int i=0;i<12;i++)sum+=surface.SampleLevel(surfaceSampler,input.uv+.01*offsets[i],0);return sum/5;
 }
 if(mode==0)return float4(input.color.rgb*((input.normal.x+2)/3),input.color.a*(.75+sin(input.uv.x+input.uv.y)*.25)*(1-smoothstep(32,640,input.depth)));
 return input.color;
}
