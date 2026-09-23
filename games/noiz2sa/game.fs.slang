Texture2D<float4> first : register(t0);
Texture2D<float4> second : register(t1);
Texture2D<float4> third : register(t2);
Texture2D<float4> tables : register(t3);
Texture2D<float4> sprites : register(t4);
struct Input { float4 position : SV_Position; float4 data : COLOR0; };
int indexOf(float value){return (int)(value*255+0.5);}
[shader("fragment")]
float4 fs_main(Input input) : SV_Target {
    int operation=(int)(input.data.z+0.5);
    int2 p=int2(input.data.xy);
    if(operation==0)return float4(input.data.x/255,0,0,1);
    if(operation==1||operation==2){
        if(operation==2){
            p.x+=indexOf(tables.Load(int3(p.x%256,258+p.x/256,0)).r)-2;
            p.y+=indexOf(tables.Load(int3(p.y%256,258+p.y/256,0)).g)-2;
        }
        int index=p.x<0||p.x>=320||p.y<0||p.y>=480?0:indexOf(first.Load(int3(p,0)).r);
        return tables.Load(int3(index,1,0));
    }
    if(operation==3){
        int a=indexOf(first.Load(int3(p,0)).r),b=indexOf(second.Load(int3(p,0)).r);
        return tables.Load(int3(b,a+2,0));
    }
    if(operation==4){
        int index;
        if(p.x<160)index=indexOf(second.Load(int3(p,0)).r);
        else if(p.x>=480)index=indexOf(third.Load(int3(p.x-480,p.y,0)).r);
        else index=indexOf(first.Load(int3(p.x-160,p.y,0)).r);
        return tables.Load(int3(index,0,0));
    }
    float4 sprite=sprites.Load(int3(p,0));
    if(sprite.a<0.5)discard;
    return sprite;
}
