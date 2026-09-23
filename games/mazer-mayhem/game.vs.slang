struct Vertex { float4 position; float4 normal; float4 color; float4 uv; };
StructuredBuffer<Vertex> vertices : register(t0);
StructuredBuffer<float4> parameters : register(t1);
struct Output { float4 position : SV_Position; float4 color : COLOR0; };
float4 apply(float4 v, uint start) { return v.x*parameters[start]+v.y*parameters[start+1]+v.z*parameters[start+2]+v.w*parameters[start+3]; }
[shader("vertex")]
Output vs_main(uint id : SV_VertexID) {
    Vertex v=vertices[id]; Output o;
    float mode=parameters[21].y, thickness=parameters[21].x;
    if(mode==2){o.position=apply(apply(float4(parameters[3].xyz,1),4),8)+v.position;o.color=v.color;}
    else if(mode<2){
        o.position=apply(apply(v.position,4),8);o.color=v.color;
        if(mode==1){float fog=1-smoothstep(0,150,length(o.position.xyz));o.color.rgb=lerp(parameters[19].rgb,o.color.rgb,fog);}
    } else {
        float4 world=apply(v.position+v.normal*(v.uv.x-v.uv.y*thickness),0)-parameters[20]*v.uv.x;
        float4 view=apply(world,4);
        o.color.rgb=lerp(parameters[17].rgb,parameters[18].rgb,v.uv.y);
        o.color.a=(1-v.uv.x)*(1-v.uv.y*parameters[18].a)*parameters[17].a;
        if(mode==3||mode==4){
            float3 normal=normalize(apply(-v.normal,12).xyz);
            float df=dot(normal,parameters[16].xyz);
            if(mode==4)o.color.a*=df;
            else {df+=(1-df)*v.uv.y;float fog=1-smoothstep(0,150,length(view.xyz));o.color.rgb=o.color.rgb*df*fog+parameters[19].rgb*(1-fog);}
        }
        o.position=apply(view,8);
    }
    return o;
}
