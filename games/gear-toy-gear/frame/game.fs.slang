LUB_TEXTURE2D(surface);
LUB_TEXTURE2D(surfaceAlpha);
float4 SampleSurface(float2 uv){return float4(LUB_SAMPLE_LOD(surface,uv).rgb,LUB_SAMPLE_LOD(surfaceAlpha,uv).r);}
struct Input {float4 position:SV_Position;float4 color:COLOR0;float depth:TEXCOORD0;float2 uv:TEXCOORD1;float3 normal:TEXCOORD2;nointerpolation uint mode:TEXCOORD3;};
float4 Shade(Input input) {
 uint mode=input.mode;
 if(mode<6 && input.color.a<=0)discard;
 if(mode==6)return SampleSurface(input.uv);
 if(mode==7){
  float2 offsets[12]={float2(-.3,-.4),float2(-.8,-.1),float2(-.7,.5),float2(-.2,.6),float2(1,-.1),float2(.5,-.5),float2(.5,.8),float2(.2,-.9),float2(.5,.1),float2(.9,.4),float2(-.3,-1),float2(-.8,-.6)};
  float4 sum=SampleSurface(input.uv);for(int i=0;i<12;i++)sum+=SampleSurface(input.uv+.01*offsets[i]);return sum/5;
 }
 if(mode==0){float4 color=float4(input.color.rgb*((input.normal.x+2)/3),input.color.a*(.75+sin(input.uv.x+input.uv.y)*.25)*(1-smoothstep(32,640,input.depth)));if(color.a<=0)discard;return color;}
 return input.color;
}
struct Output {
 float4 color:SV_Target0;
#ifdef GTG_ALPHA_TARGET
 float4 alpha:SV_Target1;
#endif
};
[shader("fragment")]
Output fs_main(Input input) {
 Output output;
 output.color=Shade(input);
#ifdef GTG_ALPHA_TARGET
 output.alpha=output.color.aaaa;
#endif
 return output;
}
