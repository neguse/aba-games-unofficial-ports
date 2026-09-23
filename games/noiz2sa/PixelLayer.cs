using System.Collections.Generic;
public class PixelLayer {
    public int width;
    public List<float> rectangles=new List<float>();
    public PixelLayer(int width){this.width=width;}
    public void Clear(){rectangles.Clear();}
    public void Rect(int x,int y,int w,int h,float u,float v,float du,float dv,int operation){
        rectangles.Add(x*2f/width-1);rectangles.Add(1-y*2f/480);rectangles.Add(w*2f/width);rectangles.Add(-h*2f/480);
        rectangles.Add(u);rectangles.Add(v);rectangles.Add(du);rectangles.Add(dv);
        rectangles.Add(operation);rectangles.Add(0);rectangles.Add(0);rectangles.Add(0);
    }
    public void Set(int offset,int color){Fill(offset,color,1);}
    public void Fill(int offset,int color,int count){if(count>0)Rect(offset%width,offset/width,count,1,color&255,0,0,0,0);}
}
