using static Drawing;
using static GameMath;
public static class RrGl {
    public const int GL_TEXTURE_2D = 99;
    public const int starTexture=1, smokeTexture=2, titleTexture=3;
    public static int input;
    public static bool quitRequested;
    static bool textured, additive=true;
    static int texture;
    static float red=255,green=255,blue=255,alpha=255;
    public static int getPadState() { return input & 15; }
    public static int getButtonState() { return input & 48; }
    public static void quitLast() { RrPreference.savePreference(); quitRequested=true; }
    public static void glClearColor(float r,float g,float b,float a) { Drawing.clearColor=new float[]{r,g,b,1}; }
    public static void glEnable(int kind) {
        if(kind==GL_TEXTURE_2D) textured=true;
        else { Drawing.glEnable(kind); if(kind==GL_BLEND){additive=true; applyColor();} }
    }
    public static void glDisable(int kind) {
        if(kind==GL_TEXTURE_2D){textured=false;applyColor();}
        else { Drawing.glDisable(kind); if(kind==GL_BLEND){additive=false;applyColor();} }
    }
    static void applyColor() { if(!textured) Color(red/255,green/255,blue/255,additive?alpha/255:1); }
    public static void glColor4ub(int r,int g,int b,int a) { red=r&255;green=g&255;blue=b&255;alpha=a&255;applyColor(); }
    public static void glBindTexture(int kind,int id) { texture=id; }
    public static void glTexCoord2f(float u,float v) {
        int rgb=integer(red)+(integer(green)<<8)+(integer(blue)<<16);
        Color((1+u*RrData.textureWidth[texture-1])/RrData.atlasWidth,(RrData.textureY[texture-1]+1+v*RrData.textureHeight[texture-1])/RrData.atlasHeight,rgb,-1);
    }
    public static void gluLookAt(float ex,float ey,float ez,float lx,float ly,float lz,float ux,float uy,float uz) {
        float fx=lx-ex,fy=ly-ey,fz=lz-ez,l=sqrt(fx*fx+fy*fy+fz*fz);fx/=l;fy/=l;fz/=l;
        float sx=fy*uz-fz*uy,sy=fz*ux-fx*uz,sz=fx*uy-fy*ux;l=sqrt(sx*sx+sy*sy+sz*sz);sx/=l;sy/=l;sz/=l;
        float tx=sy*fz-sz*fy,ty=sz*fx-sx*fz,tz=sx*fy-sy*fx;
        glMultMatrix(new float[]{sx,tx,-fx,0,sy,ty,-fy,0,sz,tz,-fz,0,0,0,0,1});glTranslatef(-ex,-ey,-ez);
    }
}
