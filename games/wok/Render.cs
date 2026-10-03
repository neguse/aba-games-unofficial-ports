using System.Collections.Generic;
using static Lub;
public static class WkRender {
 public static List<float> vertices=new List<float>();
 static int version;
 public static void Init(){}
 public static void Rect(int x,int y,int w,int h,int u,int v,float r,float g,float b){
  vertices.Add(x/320f-1);vertices.Add(1-y/240f);vertices.Add(w/320f);vertices.Add(-h/240f);
  vertices.Add(u);vertices.Add(v);vertices.Add(w);vertices.Add(h);
  vertices.Add(r);vertices.Add(g);vertices.Add(b);vertices.Add(1);
 }
 public static void Sprite(int n,int x,int y){Rect(x,y,WkData.widths[n],WkData.heights[n],0,WkData.ys[n],1,1,1);}
 public static void Solid(int x,int y,int w,int h){Rect(x,y,w,h,-1,0,WkData.zoneColor[0]/255f,WkData.zoneColor[1]/255f,WkData.zoneColor[2]/255f);}
 public static void Frame(){
  version++;
  var shader=Gfx.UseShader("wok",GameShaders.vertex,GameShaders.fragment,1);
  Png.Load("images/atlas.png",out var pixels,out _,out _,out _,out _,out int pixelsVersion,out _,out _);
  var atlas=pixels==null?null:Gfx.UseTextureBytes("atlas",WkData.atlasWidth,WkData.atlasHeight,Gfx.PixelFormat.Rgba8,pixels,pixelsVersion);
  var rects=Gfx.UseBuffer("rects",Gfx.BufferType.Storage,vertices,version);
  if(shader==null||atlas==null||rects==null)return;
  Gfx.BeginPass(new PassOpts{Target=Gfx.MainTex,ClearColor=new float[]{1,1,1,1}});
  if(vertices.Count>0)Gfx.Draw(vertices.Count/12*6,new Dictionary<string,object>{["rectangles"]=rects,["atlas"]=atlas},new DrawOpts{Shader=shader,Blend=Gfx.Blend.None,Depth=false,DepthWrite=false,Cull=Gfx.Cull.None});
  Gfx.EndPass();
 }
}
