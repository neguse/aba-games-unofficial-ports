struct Vertex {float4 position;float4 color;};
StructuredBuffer<Vertex> vertices : register(t0);
StructuredBuffer<float4> parameters : register(t1);
StructuredBuffer<float4> eye : register(t4);
struct Output {float4 position:SV_Position;float4 color:COLOR0;float depth:TEXCOORD0;float2 uv:TEXCOORD1;float3 normal:TEXCOORD2;nointerpolation uint mode:TEXCOORD3;};
float4 transform(float4 p,uint start){return p.x*parameters[start]+p.y*parameters[start+1]+p.z*parameters[start+2]+p.w*parameters[start+3];}
float3 rotate(float3 v,float4 q){return (2*q.w*q.w-1)*v+2*dot(v,q.xyz)*q.xyz+2*q.w*cross(q.xyz,v);}
[shader("vertex")]
Output vs_main(uint id:SV_VertexID,uint instance:SV_InstanceID){
 Output o=(Output)0;uint mode=(uint)parameters[8].y;o.mode=mode;
 if(mode>=6){o.uv=float2(id==1?2:0,id==2?2:0);o.position=float4(o.uv.x*2-1,1-o.uv.y*2,0,1);return o;}
 Vertex v=vertices[id];float4 p0=parameters[33+instance*3],p1=parameters[34+instance*3],p2=parameters[35+instance*3];
 float3 rotated=rotate(v.position.xyz*p0.w,p1);float4 p=float4(p0.xyz+rotated,1);float4 view=transform(p,0);
 float4 local=transform(p,4);o.position=float4(dot(eye[0],local),dot(eye[1],local),dot(eye[2],local),dot(eye[3],local));o.depth=-view.z;
 if(mode==0){
  o.color=v.color*v.color.a+p2*(1-v.color.a);o.color.rgb*=(sin((view.z+parameters[8].x)*.05)+1)*.5+1;
  float3 light=0;int li=(int)(2-p0.z/32);float rates[5]={.3,.7,1,.7,.3};
  for(int i=0;i<5;i++)light+=parameters[9+clamp(li+i,0,23)].xyz*rates[i];
  o.color.rgb=o.color.rgb*(.5-smoothstep(0,500,length(light))*.5)+light*.002;
  o.uv=float2(v.position.x*5,(v.position.y+v.position.z)*2);o.normal=normalize(p.xyz*2+rotated);
 }else if(mode==1||mode==2){
  float a=(1-smoothstep(36,256,o.depth))*smoothstep(20,36,o.depth);o.color.rgb=v.color.rgb*a+p2.rgb*(1-a);o.color.a=mode==1?v.color.a*a:1-a*.2;
 }else if(mode==3||mode==4){
  o.color.rgb=p2.rgb*(1+sin(float3(dot(rotated,float3(2,1,3)),dot(rotated,float3(3,2,1)),dot(rotated,float3(1,3,2))))*.5);
  o.color.a=p2.a*(mode==3?smoothstep(20,36,o.depth):(1-smoothstep(50,640,o.depth))*smoothstep(20,50,o.depth));
 }else o.color=p2;
 return o;
}
