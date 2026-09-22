// Copyright 2003 Kenta Cho. All rights reserved.
using System.Collections.Generic;
using static Drawing;
using static Lub;

public class A7xScreen : Screen
{
    public void startRenderToTexture()
    {
        BeginFrame();
        viewportWidth = 128; viewportHeight = 128; glLineWidth(1);
    }

    public void endRenderToTexture()
    {
        Game.render(Game.glow, Game.blank, false);
        viewportWidth = 640; viewportHeight = 480;
    }

    public void drawLuminous()
    {
        Game.render(Gfx.MainTex, Game.blank, false);
        var vertices = new List<float>();
        int[] dx = new int[] { 0, 5, -5, 0, 0 }, dy = new int[] { 0, 0, 0, 5, -5 };
        int[] corners = new int[] { 0, 1, 2, 0, 2, 3 };
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                int c = corners[j];
                float u = c >= 2 ? 1 : 0, v = c == 1 || c == 2 ? 1 : 0;
                float x = u * 640 + dx[i], y = v * 480 + (c >= 2 ? dx[i] : dy[i]);
                vertices.Add(x / 320 - 1);
                vertices.Add(1 - y / 240);
                vertices.Add(0);
                vertices.Add(1);
                vertices.Add(u);
                vertices.Add(v);
                vertices.Add(0);
                vertices.Add(-1);
            }
        }

        Gfx.BeginPass(new PassOpts { Target = Gfx.MainTex, Load = Gfx.LoadAction.Load });
        Game.drawVertices(vertices, Game.glow, false);
        Gfx.EndPass();
        batches.Clear();
    }

    public static void setColor(float r, float g, float b, float a)
    {
        Color(r, g, b, a);
    }

    public static void drawBoxSolid(float x, float y, float width, float height)
    {
        glBegin(GL_QUADS);
        glVertex2f(x, y);
        glVertex2f(x + width, y);
        glVertex2f(x + width, y + height);
        glVertex2f(x, y + height);
        glEnd();
    }

    public static void drawBoxLine(float x, float y, float width, float height)
    {
        glBegin(GL_LINE_LOOP);
        glVertex2f(x, y);
        glVertex2f(x + width, y);
        glVertex2f(x + width, y + height);
        glVertex2f(x, y + height);
        glEnd();
    }
}
