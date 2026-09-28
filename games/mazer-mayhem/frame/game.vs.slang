struct Vertex { float4 position; float4 normal; float4 color; float4 uv; };
StructuredBuffer<Vertex> vertices : register(t0);
StructuredBuffer<float4> parameters : register(t1);
StructuredBuffer<float4> eye : register(t2);
struct Output { float4 position : SV_Position; float4 color : COLOR0; };
float4 apply(float4 v, uint start) { return v.x*parameters[start]+v.y*parameters[start+1]+v.z*parameters[start+2]+v.w*parameters[start+3]; }
[shader("vertex")]
Output vs_main(uint id : LUB_VERTEX_ID, uint instance : LUB_INSTANCE_ID) {
    uint b=instance*22;
    Vertex v=vertices[id]; Output o;
    float mode=parameters[b+21].y, thickness=parameters[b+21].x;
    bool hud=parameters[b+21].z>0;
    float4 local;
    if(mode==2){
        float4 view=apply(float4(parameters[b+3].xyz,1),b+4);
        float4 clip=apply(view,b+8)+v.position;
        float depth=hud?2:-view.z*.06;
        local=float4(clip.xy/clip.w*float2(4.0/3,1)*depth,-depth,1);
        o.color=v.color;
    } else {
        float4 view;
        if(mode<2){
            view=apply(v.position,b+4);o.color=v.color;
            if(mode==1){float fog=1-smoothstep(0,150,length(apply(view,b+8).xyz));o.color.rgb=lerp(parameters[b+19].rgb,o.color.rgb,fog);}
        } else {
            float4 world=apply(v.position+v.normal*(v.uv.x-v.uv.y*thickness),b)-parameters[b+20]*v.uv.x;
            view=apply(world,b+4);
            o.color.rgb=lerp(parameters[b+17].rgb,parameters[b+18].rgb,v.uv.y);
            o.color.a=(1-v.uv.x)*(1-v.uv.y*parameters[b+18].a)*parameters[b+17].a;
            if(mode==3||mode==4){
                float3 normal=normalize(apply(-v.normal,b+12).xyz);
                float df=dot(normal,parameters[b+16].xyz);
                if(mode==4)o.color.a*=df;
                else {df+=(1-df)*v.uv.y;float fog=1-smoothstep(0,150,length(view.xyz));o.color.rgb=o.color.rgb*df*fog+parameters[b+19].rgb*(1-fog);}
            }
        }
        local=float4(view.xyz*(hud?2:.06),1);
    }
    o.position=float4(dot(eye[0],local),dot(eye[1],local),dot(eye[2],local),dot(eye[3],local));
    return o;
}
