// Copyright 2004 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Lub;

public class Tunnel
{
    public const int DEPTH_NUM = 72;
    public const int SHIP_IDX_OFS = 5;
    public const float RAD_RATIO = 1.05f;
    public const float DEPTH_CHANGE_RATIO = 1.15f;
    public const float DEPTH_RATIO_MAX = 80;
    public Slice[] slice;
    public float shipDeg, shipOfs, shipY;
    public int shipIdx;
    public Vector3 shipPos;
    public Vector3 tpos;
    public Torus torus;
    public int torusIdx;
    public float pointFrom;
    public float sightDepth;
    public Slice[] sliceBackward;
    public Tunnel()
    {
        slice = new Slice[DEPTH_NUM];
        for (int i = 0; i < DEPTH_NUM; i++)
            slice[i] = new Slice();
        sliceBackward = new Slice[DEPTH_NUM];
        for (int i = 0; i < DEPTH_NUM; i++)
            sliceBackward[i] = new Slice();
        shipPos = new Vector3();
        tpos = new Vector3();
    }

    public void start(Torus torus)
    {
        this.torus = torus;
        torusIdx = 0;
        pointFrom = 0;
        sightDepth = 0;
    }

    public void setSlices()
    {
        float ti = torusIdx;
        int pti = 0;
        sightDepth = 0;
        float dr = 1;
        Slice ps = slice[0];
        ps.setFirst(pointFrom, torus.getSliceState(torusIdx), -shipIdx - shipOfs);
        for (int i = 1; i < slice.Length; i++)
        {
            pti = GameMath.integer(ti);
            ti = ti + (dr);
            sightDepth = sightDepth + (dr);
            if (ti >= torus.sliceNum)
                ti = ti - (torus.sliceNum);
            slice[i].set_4(ps, torus.getSliceStateWithRing(GameMath.integer(ti), pti), dr, sightDepth - shipIdx - shipOfs);
            if ((i >= GameMath.integer(slice.Length / 2)) && (dr < DEPTH_RATIO_MAX))
                dr = dr * (DEPTH_CHANGE_RATIO);
            ps = slice[i];
        }
    }

    public void setSlicesBackward()
    {
        float ti = torusIdx;
        int pti = 0;
        float sd = 0;
        float dr = -1;
        Slice ps = sliceBackward[0];
        ps.setFirst(pointFrom, torus.getSliceState(torusIdx), -shipIdx - shipOfs);
        for (int i = 1; i < sliceBackward.Length; i++)
        {
            pti = GameMath.integer(ti);
            ti = ti + (dr);
            sd = sd + (dr);
            if (ti < 0)
                ti = ti + (torus.sliceNum);
            sliceBackward[i].set_4(ps, torus.getSliceStateWithRing(pti, GameMath.integer(ti)), dr, sd - shipIdx - shipOfs);
            if ((i >= GameMath.integer(sliceBackward.Length / 2)) && (dr > -DEPTH_RATIO_MAX))
                dr = dr * (DEPTH_CHANGE_RATIO);
            ps = sliceBackward[i];
        }
    }

    public void goToNextSlice(int n)
    {
        if (n <= 0)
            return;
        torusIdx = torusIdx + (n);
        for (int i = 0; i < n; i++)
        {
            pointFrom = pointFrom + (slice[i].state.mp);
            pointFrom = pointFrom % (slice[i].state.pointNum);
            if (pointFrom < 0)
                pointFrom = pointFrom + (slice[i].state.pointNum);
        }

        if (torusIdx >= torus.sliceNum)
        {
            torusIdx = torusIdx - (torus.sliceNum);
            pointFrom = 0;
        }
    }

    public void setShipPos(float d, float o, float y)
    {
        shipDeg = d;
        shipOfs = o;
        shipY = y;
        shipIdx = SHIP_IDX_OFS;
    }

    public Vector3 getPos_4_Single_Single_Int32_Single(float d, float o, int si, float rr)
    {
        int nsi = si + 1;
        float r = slice[si].state.rad * (1 - o) + slice[nsi].state.rad * o;
        float d1 = slice[si].d1 * (1 - o) + slice[nsi].d1 * o;
        float d2 = slice[si].d2 * (1 - o) + slice[nsi].d2 * o;
        tpos.x = 0;
        tpos.y = r * rr;
        tpos.z = 0;
        tpos.rollZ(d);
        tpos.rollY(d1);
        tpos.rollX(d2);
        tpos.x = tpos.x + (slice[si].centerPos.x * (1 - o) + slice[nsi].centerPos.x * o);
        tpos.y = tpos.y + (slice[si].centerPos.y * (1 - o) + slice[nsi].centerPos.y * o);
        tpos.z = tpos.z + (slice[si].centerPos.z * (1 - o) + slice[nsi].centerPos.z * o);
        return tpos;
    }

    public Vector3 getPosBackward(float d, float o, int si, float rr)
    {
        int nsi = si + 1;
        float r = sliceBackward[si].state.rad * (1 - o) + sliceBackward[nsi].state.rad * o;
        float d1 = sliceBackward[si].d1 * (1 - o) + sliceBackward[nsi].d1 * o;
        float d2 = sliceBackward[si].d2 * (1 - o) + sliceBackward[nsi].d2 * o;
        tpos.x = 0;
        tpos.y = r * rr;
        tpos.z = 0;
        tpos.rollZ(d);
        tpos.rollY(d1);
        tpos.rollX(d2);
        tpos.x = tpos.x + (sliceBackward[si].centerPos.x * (1 - o) + sliceBackward[nsi].centerPos.x * o);
        tpos.y = tpos.y + (sliceBackward[si].centerPos.y * (1 - o) + sliceBackward[nsi].centerPos.y * o);
        tpos.z = tpos.z + (sliceBackward[si].centerPos.z * (1 - o) + sliceBackward[nsi].centerPos.z * o);
        return tpos;
    }

    public Vector3 getPos_3_Single_Single_Int32(float d, float o, int si)
    {
        return getPos_4_Single_Single_Int32_Single(d, o, si, 1.0f);
    }

    public Vector3 getPos_1_Vector(Vector p)
    {
        int si = 0;
        float o = 0;
        if (p.y >= -shipIdx - shipOfs)
        {
            {
                Vector index = calcIndex(p.y);
                si = GameMath.integer(index.x);
                o = index.y;
            }

            return getPos_4_Single_Single_Int32_Single(p.x, o, si, 1.0f);
        }
        else
        {
            {
                Vector index = calcIndexBackward(p.y);
                si = GameMath.integer(index.x);
                o = index.y;
            }

            return getPosBackward(p.x, o, si, 1.0f);
        }
    }

    public Vector3 getPos_1_Vector3(Vector3 p)
    {
        int si = 0;
        float o = 0;
        {
            Vector index = calcIndex(p.y);
            si = GameMath.integer(index.x);
            o = index.y;
        }

        return getPos_4_Single_Single_Int32_Single(p.x, o, si, RAD_RATIO - p.z / slice[si].state.rad);
    }

    public Vector3 getCenterPos(float y, Vector angles)
    {
        angles.x = 0;
        angles.y = 0;
        int si = 0;
        float o = 0;
        y = y - (shipY);
        if (y < GameMath.integer(-getTorusLength() / 2))
            y = y + (getTorusLength());
        if (y >= -shipIdx - shipOfs)
        {
            {
                Vector index = calcIndex(y);
                si = GameMath.integer(index.x);
                o = index.y;
            }

            int nsi = si + 1;
            angles.x = slice[si].d1 * (1 - o) + slice[nsi].d1 * o;
            angles.y = slice[si].d2 * (1 - o) + slice[nsi].d2 * o;
            tpos.x = slice[si].centerPos.x * (1 - o) + slice[nsi].centerPos.x * o;
            tpos.y = slice[si].centerPos.y * (1 - o) + slice[nsi].centerPos.y * o;
            tpos.z = slice[si].centerPos.z * (1 - o) + slice[nsi].centerPos.z * o;
        }
        else
        {
            {
                Vector index = calcIndexBackward(y);
                si = GameMath.integer(index.x);
                o = index.y;
            }

            int nsi = si + 1;
            angles.x = sliceBackward[si].d1 * (1 - o) + sliceBackward[nsi].d1 * o;
            angles.y = sliceBackward[si].d2 * (1 - o) + sliceBackward[nsi].d2 * o;
            tpos.x = sliceBackward[si].centerPos.x * (1 - o) + sliceBackward[nsi].centerPos.x * o;
            tpos.y = sliceBackward[si].centerPos.y * (1 - o) + sliceBackward[nsi].centerPos.y * o;
            tpos.z = sliceBackward[si].centerPos.z * (1 - o) + sliceBackward[nsi].centerPos.z * o;
        }

        return tpos;
    }

    public Slice getSlice(float y)
    {
        int si = 0;
        float o = 0;
        if (y >= -shipIdx - shipOfs)
        {
            {
                Vector index = calcIndex(y);
                si = GameMath.integer(index.x);
                o = index.y;
            }

            return slice[si];
        }
        else
        {
            {
                Vector index = calcIndexBackward(y);
                si = GameMath.integer(index.x);
                o = index.y;
            }

            return sliceBackward[si];
        }
    }

    public float checkInCourse(Vector p)
    {
        Slice sl = getSlice(p.y);
        if (sl.isNearlyRound())
            return 0;
        float ld = sl.getLeftEdgeDeg();
        float rd = sl.getRightEdgeDeg();
        int rsl = checkDegInside(p.x, ld, rd);
        if (rsl == 0)
        {
            return 0;
        }
        else
        {
            float rad = sl.state.rad;
            float ofs = 0;
            if (rsl == 1)
                ofs = p.x - rd;
            else
                ofs = ld - p.x;
            if (ofs >= PI * 2)
                ofs = ofs - (PI * 2);
            else if (ofs < 0)
                ofs = ofs + (PI * 2);
            return ofs * rad * rsl;
        }
    }

    public static int checkDegInside(float d, float ld, float rd)
    {
        int rsl = 0;
        if (rd <= ld)
        {
            if ((d > rd) && (d < ld))
            {
                if (d < (rd + ld) / 2)
                    rsl = 1;
                else
                    rsl = -1;
            }
        }
        else
        {
            if ((d < ld) || (d > rd))
            {
                float cd = (ld + rd) / 2 + PI;
                if (cd >= PI * 2)
                    cd = cd - (PI * 2);
                if (cd >= PI)
                {
                    if ((d < cd) && (d > rd))
                        rsl = 1;
                    else
                        rsl = -1;
                }
                else
                {
                    if ((d > cd) && (d < ld))
                        rsl = -1;
                    else
                        rsl = 1;
                }
            }
        }

        return rsl;
    }

    public float getRadius(float z)
    {
        int si = 0;
        float o = 0;
        {
            Vector index = calcIndex(z);
            si = GameMath.integer(index.x);
            o = index.y;
        }

        int nsi = si + 1;
        return slice[si].state.rad * (1.0f - o) + slice[nsi].state.rad * o;
    }

    public Vector calcIndex(float z)
    {
        int idx = 0;
        float ofs = 0;
        idx = slice.Length + 99999;
        for (int i = 1; i < slice.Length; i++)
        {
            if (z < slice[i].depth)
            {
                idx = i - 1;
                ofs = (z - slice[idx].depth) / (slice[idx + 1].depth - slice[idx].depth);
                break;
            }
        }

        if (idx < 0)
        {
            idx = 0;
            ofs = 0;
        }
        else if (idx >= slice.Length - 1)
        {
            idx = slice.Length - 2;
            ofs = 0.99f;
        }

        if (!(ofs >= 0))
            ofs = 0;
        else if (ofs >= 1)
            ofs = 0.99f;
        return new Vector(idx, ofs);
    }

    public Vector calcIndexBackward(float z)
    {
        int idx = 0;
        float ofs = 0;
        idx = sliceBackward.Length + 99999;
        for (int i = 1; i < sliceBackward.Length; i++)
        {
            if (z > sliceBackward[i].depth)
            {
                idx = i - 1;
                ofs = (sliceBackward[idx].depth - z) / (sliceBackward[idx + 1].depth - sliceBackward[idx].depth);
                break;
            }
        }

        if (idx < 0)
        {
            idx = 0;
            ofs = 0;
        }
        else if (idx >= sliceBackward.Length - 1)
        {
            idx = sliceBackward.Length - 2;
            ofs = 0.99f;
        }

        if (!(ofs >= 0))
            ofs = 0;
        else if (ofs >= 1)
            ofs = 0.99f;
        return new Vector(idx, ofs);
    }

    public bool checkInScreen_2(Vector p, Ship ship)
    {
        return checkInScreen_4(p, ship, 0.03f, 28);
    }

    public bool checkInScreen_4(Vector p, Ship ship, float v, float ofs)
    {
        float xr = fabs(p.x - ship.eyePos.x);
        if (xr > PI)
            xr = PI * 2 - xr;
        xr = xr * (getRadius(0) / SliceState.DEFAULT_RAD);
        v = v * ((p.y + ofs));
        if (xr > v)
            return false;
        else
            return true;
    }

    public bool checkInSight(float y)
    {
        float oy = y - torusIdx;
        if (oy < 0)
            oy = oy + (getTorusLength());
        if ((oy > 0) && (oy < sightDepth - 1))
            return true;
        else
            return false;
    }

    public int getTorusLength()
    {
        return torus.sliceNum;
    }

    public void draw(float[] model, float[] tint, Gfx.Blend blend, Gfx.Cull cull, float lineWidth)
    {
        blend = Gfx.Blend.Alpha;
        float lineBn = 0.4f, polyBn = 0, lightBn = 0.5f - Slice.darkLineRatio * 0.2f;
        slice[slice.Length - 1].setPointPos();
        for (int i = slice.Length - 1; i >= 1; i--)
        {
            slice[i - 1].setPointPos();
            slice[i].draw(model, tint, blend, cull, lineWidth, slice[i - 1], lineBn, polyBn, lightBn, this);
            lineBn = lineBn * (1.02f);
            if (lineBn > 1)
                lineBn = 1;
            lightBn = lightBn * (1.02f);
            if (lightBn > 1)
                lightBn = 1;
            if (i < GameMath.integer(slice.Length / 2))
            {
                if (polyBn <= 0)
                    polyBn = 0.2f;
                polyBn = polyBn * (1.03f);
                if (polyBn > 1)
                    polyBn = 1;
            }

            if (i < slice.Length * 0.75f)
            {
                lineBn = lineBn * (1.0f - Slice.darkLineRatio * 0.05f);
                lightBn = lightBn * (1.0f + Slice.darkLineRatio * 0.02f);
            }
        }

        blend = Gfx.Blend.Additive;
    }

    public void drawBackward(float[] model, float[] tint, Gfx.Blend blend, Gfx.Cull cull, float lineWidth)
    {
        blend = Gfx.Blend.Alpha;
        float lineBn = 0.4f, polyBn = 0, lightBn = 0.5f - Slice.darkLineRatio * 0.2f;
        sliceBackward[sliceBackward.Length - 1].setPointPos();
        for (int i = sliceBackward.Length - 1; i >= 1; i--)
        {
            sliceBackward[i - 1].setPointPos();
            sliceBackward[i].draw(model, tint, blend, cull, lineWidth, sliceBackward[i - 1], lineBn, polyBn, lightBn, this);
            lineBn = lineBn * (1.02f);
            if (lineBn > 1)
                lineBn = 1;
            lightBn = lightBn * (1.02f);
            if (lightBn > 1)
                lightBn = 1;
            if (i < GameMath.integer(slice.Length / 2))
            {
                if (polyBn <= 0)
                    polyBn = 0.2f;
                polyBn = polyBn * (1.03f);
                if (polyBn > 1)
                    polyBn = 1;
            }

            if (i < slice.Length * 0.75f)
            {
                lineBn = lineBn * (1.0f - Slice.darkLineRatio * 0.05f);
                lightBn = lightBn * (1.0f + Slice.darkLineRatio * 0.02f);
            }
        }

        blend = Gfx.Blend.Additive;
    }
}

public class Slice
{
    static int nextMesh;
    public string meshKey;
    public const float SLICE_DEPTH = 5;
    public static float lineR, lineG, lineB;
    public static float polyR, polyG, polyB;
    public static bool darkLine;
    public static float darkLineRatio;
    public SliceState _state;
    public float _d1, _d2;
    public float _pointFrom;
    public Vector3 _centerPos;
    public float pointRatio;
    public Vector3[] pointPos;
    public Vector3 radOfs;
    public Vector3 polyPoint;
    public float _depth;
    public Slice()
    {
        meshKey = nextMesh.ToString(); nextMesh++;
        _state = new SliceState();
        _centerPos = new Vector3();
        pointPos = new Vector3[SliceState.MAX_POINT_NUM];
        for (int i = 0; i < SliceState.MAX_POINT_NUM; i++)
            pointPos[i] = new Vector3();
        radOfs = new Vector3();
        polyPoint = new Vector3();
    }

    public void setFirst(float pf, SliceState state, float dpt)
    {
        {
            _centerPos.z = 0;
            _centerPos.y = _centerPos.z;
            _centerPos.x = _centerPos.y;
        }

        {
            _d2 = 0;
            _d1 = _d2;
        }

        _pointFrom = pf;
        _state.set_1(state);
        _depth = dpt;
        pointRatio = 1;
    }

    public void set_4(Slice prevSlice, SliceState state, float depthRatio, float dpt)
    {
        _d1 = prevSlice.d1 + state.md1 * depthRatio;
        _d2 = prevSlice.d2 + state.md2 * depthRatio;
        {
            _centerPos.y = 0;
            _centerPos.x = _centerPos.y;
        }

        _centerPos.z = SLICE_DEPTH * depthRatio;
        _centerPos.rollY(_d1);
        _centerPos.rollX(_d2);
        _centerPos.x = _centerPos.x + (prevSlice.centerPos.x);
        _centerPos.y = _centerPos.y + (prevSlice.centerPos.y);
        _centerPos.z = _centerPos.z + (prevSlice.centerPos.z);
        pointRatio = 1 + (fabs(depthRatio) - 1) * 0.02f;
        _pointFrom = prevSlice.pointFrom + state.mp * depthRatio;
        _pointFrom = _pointFrom % (state.pointNum);
        if (_pointFrom < 0)
            _pointFrom = _pointFrom + (state.pointNum);
        _state.set_1(state);
        _depth = dpt;
    }

    public void draw(float[] model, float[] tint, Gfx.Blend blend, Gfx.Cull cull, float lineWidth, Slice prevSlice, float lineBn, float polyBn, float lightBn, Tunnel tunnel)
    {
        var mesh = new Mesh("Tunnel-draw" + "-" + meshKey);
        float pi = _pointFrom;
        float width = _state.courseWidth;
        float prevPi = 0;
        bool isFirst = true;
        bool polyFirst = true;
        bool roundSlice = false;
        if (_state.courseWidth == _state.pointNum)
            roundSlice = true;
        for (;;)
        {
            if (!(isFirst))
            {
                int psPi = GameMath.integer((pi * prevSlice.state.pointNum / _state.pointNum));
                int psPrevPi = GameMath.integer((prevPi * prevSlice.state.pointNum / _state.pointNum));
                tint = new float[] { lineR * lineBn, lineG * lineBn, lineB * lineBn, 1 };
                int part1 = mesh.vertexCount;
                mesh.Vertex(pointPos[GameMath.integer(pi)].x, pointPos[GameMath.integer(pi)].y, pointPos[GameMath.integer(pi)].z, tint);
                mesh.Vertex(prevSlice.pointPos[psPi].x, prevSlice.pointPos[psPi].y, prevSlice.pointPos[psPi].z, tint);
                mesh.Vertex(prevSlice.pointPos[psPrevPi].x, prevSlice.pointPos[psPrevPi].y, prevSlice.pointPos[psPrevPi].z, tint);
                mesh.LineStrip(part1, mesh.vertexCount - part1);
                if (polyBn > 0)
                {
                    if ((roundSlice) || (((!(polyFirst)) && (width > 0))))
                    {
                        tint = new float[] { polyR, polyG, polyB, polyBn };
                        int part2 = mesh.vertexCount;
                        polyPoint.blend(pointPos[GameMath.integer(prevPi)], prevSlice.pointPos[psPi], 0.9f);
                        mesh.Vertex(polyPoint.x, polyPoint.y, polyPoint.z, tint);
                        polyPoint.blend(pointPos[GameMath.integer(pi)], prevSlice.pointPos[psPrevPi], 0.9f);
                        mesh.Vertex(polyPoint.x, polyPoint.y, polyPoint.z, tint);
                        tint = new float[] { polyR, polyG, polyB, polyBn / 2 };
                        polyPoint.blend(pointPos[GameMath.integer(prevPi)], prevSlice.pointPos[psPi], 0.1f);
                        mesh.Vertex(polyPoint.x, polyPoint.y, polyPoint.z, tint);
                        polyPoint.blend(pointPos[GameMath.integer(pi)], prevSlice.pointPos[psPrevPi], 0.1f);
                        mesh.Vertex(polyPoint.x, polyPoint.y, polyPoint.z, tint);
                        mesh.Fan(part2, mesh.vertexCount - part2);
                    }
                    else
                    {
                        polyFirst = false;
                    }
                }
            }
            else
            {
                isFirst = false;
            }

            prevPi = pi;
            pi = pi + (pointRatio);
            while (pi >= _state.pointNum)
                pi = pi - (_state.pointNum);
            if (width <= 0)
                break;
            width = width - (pointRatio);
        }

        if (_state.courseWidth < _state.pointNum)
        {
            pi = _pointFrom;
            int psPi = GameMath.integer((pi * prevSlice.state.pointNum / _state.pointNum));
            tint = new float[] { lineBn / 3 * 2, lineBn / 3 * 2, lineBn, 1 };
            int part3 = mesh.vertexCount;
            mesh.Vertex(pointPos[GameMath.integer(pi)].x, pointPos[GameMath.integer(pi)].y, pointPos[GameMath.integer(pi)].z, tint);
            mesh.Vertex(prevSlice.pointPos[psPi].x, prevSlice.pointPos[psPi].y, prevSlice.pointPos[psPi].z, tint);
            mesh.LineStrip(part3, mesh.vertexCount - part3);
        }

        if ((!(roundSlice)) && (lightBn > 0.2f))
        {
            appendSideLight(mesh, model, tint, getLeftEdgeDeg() - 0.07f, lightBn);
            appendSideLight(mesh, model, tint, getRightEdgeDeg() + 0.07f, lightBn);
        }

        if (mesh.count > 0) Gfx.Draw(mesh.count, mesh.Bindings(model, tint, lineWidth, blend == Gfx.Blend.Additive),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = cull, Blend = blend });
        if ((_state.ring != null))
            if (lightBn > 0.2f)
                _state.ring.draw(model, tint, blend, cull, lineWidth, lightBn * 0.7f, tunnel);
    }

    public void setPointPos()
    {
        float d = 0, md = PI * 2 / (_state.pointNum - 1);
        foreach (Vector3 pp in pointPos)
        {
            radOfs.x = 0;
            radOfs.y = _state.rad * Tunnel.RAD_RATIO;
            radOfs.z = 0;
            radOfs.rollZ(d);
            radOfs.rollY(_d1);
            radOfs.rollX(_d2);
            pp.x = radOfs.x + _centerPos.x;
            pp.y = radOfs.y + _centerPos.y;
            pp.z = radOfs.z + _centerPos.z;
            d = d + (md);
        }
    }

    public void appendSideLight(Mesh mesh, float[] model, float[] tint, float deg, float lightBn)
    {
        radOfs.x = 0;
        radOfs.y = _state.rad;
        radOfs.z = 0;
        radOfs.rollZ(deg);
        radOfs.rollY(_d1);
        radOfs.rollX(_d2);
        radOfs.opAddAssign(_centerPos);
        tint = new float[] { 1 * lightBn, 1 * lightBn, 0.6f * lightBn, 1 };
        int part1 = mesh.vertexCount;
        mesh.Vertex(radOfs.x - 0.5f, radOfs.y - 0.5f, radOfs.z, tint);
        mesh.Vertex(radOfs.x + 0.5f, radOfs.y - 0.5f, radOfs.z, tint);
        mesh.Vertex(radOfs.x + 0.5f, radOfs.y + 0.5f, radOfs.z, tint);
        mesh.Vertex(radOfs.x - 0.5f, radOfs.y + 0.5f, radOfs.z, tint);
        mesh.LineStrip(part1, mesh.vertexCount - part1, true);
        int part2 = mesh.vertexCount;
        tint = new float[] { 0.5f * lightBn, 0.5f * lightBn, 0.3f * lightBn, 1 };
        mesh.Vertex(radOfs.x, radOfs.y, radOfs.z, tint);
        tint = new float[] { 0.9f * lightBn, 0.9f * lightBn, 0.6f * lightBn, 1 };
        mesh.Vertex(radOfs.x - 0.5f, radOfs.y - 0.5f, radOfs.z, tint);
        mesh.Vertex(radOfs.x - 0.5f, radOfs.y + 0.5f, radOfs.z, tint);
        mesh.Vertex(radOfs.x + 0.5f, radOfs.y + 0.5f, radOfs.z, tint);
        mesh.Vertex(radOfs.x + 0.5f, radOfs.y - 0.5f, radOfs.z, tint);
        mesh.Vertex(radOfs.x - 0.5f, radOfs.y - 0.5f, radOfs.z, tint);
        mesh.Fan(part2, mesh.vertexCount - part2);
    }

    public bool isNearlyRound()
    {
        if (_state.courseWidth >= _state.pointNum - 1)
            return true;
        else
            return false;
    }

    public float getLeftEdgeDeg()
    {
        return _pointFrom * PI * 2 / _state.pointNum;
    }

    public float getRightEdgeDeg()
    {
        float rd = (_pointFrom + _state.courseWidth) * PI * 2 / _state.pointNum;
        if (rd >= PI * 2)
            rd = rd - (PI * 2);
        return rd;
    }

    public SliceState state
    {
        get
        {
            return _state;
        }
    }

    public float d1
    {
        get
        {
            return _d1;
        }
    }

    public float d2
    {
        get
        {
            return _d2;
        }
    }

    public Vector3 centerPos
    {
        get
        {
            return _centerPos;
        }
    }

    public float pointFrom
    {
        get
        {
            return _pointFrom;
        }
    }

    public float depth
    {
        get
        {
            return _depth;
        }
    }
}

public class Torus
{
    public const int LENGTH = 5000;
    public int _sliceNum;
    public List<TorusPart> torusPart = new List<TorusPart>();
    public Rand rand;
    public int tpIdx;
    public List<Ring> ring = new List<Ring>();
    public Torus()
    {
        rand = new Rand();
        ring.Clear();
    }

    public void create(int seed)
    {
        rand.setSeed(seed);
        tpIdx = 0;
        torusPart.Clear();
        _sliceNum = 0;
        int tl = LENGTH;
        SliceState prev = new SliceState();
        while (tl > 0)
        {
            TorusPart tp = new TorusPart();
            int lgt = 64 + rand.nextInt(30);
            tp.create(prev, _sliceNum, lgt, rand);
            prev = tp.sliceState;
            torusPart.Add(tp);
            tl = tl - (tp.sliceNum);
            _sliceNum = _sliceNum + (tp.sliceNum);
        }

        torusPart[0].sliceState.init_0();
        torusPart[torusPart.Count - 1].sliceState.init_0();
        ring.Clear();
        int ri = 5;
        while (ri < _sliceNum - 100)
        {
            SliceState ss = getSliceState(ri);
            if (ri == 5)
                ring.Add(new Ring(ri, ss, 1));
            else
                ring.Add(new Ring(ri, ss));
            ri = ri + (100 + rand.nextInt(200));
        }
    }

    public void close()
    {
        if ((ring != null && ring.Count > 0))
            foreach (Ring r in ring)
                r.close();
    }

    public TorusPart getTorusPart(int idx)
    {
        for (int i = 0; i < torusPart.Count; i++)
        {
            if (torusPart[tpIdx].contains(idx))
                break;
            tpIdx++;
            if (tpIdx >= torusPart.Count)
                tpIdx = 0;
        }

        return torusPart[tpIdx];
    }

    public SliceState getSliceState(int idx)
    {
        TorusPart tp = getTorusPart(idx);
        int prvTpIdx = tpIdx - 1;
        if (prvTpIdx < 0)
            prvTpIdx = torusPart.Count - 1;
        SliceState ss = tp.createBlendedSliceState(torusPart[prvTpIdx].sliceState, idx);
        return ss;
    }

    public SliceState getSliceStateWithRing(int idx, int pidx)
    {
        SliceState ss = getSliceState(idx);
        ss.ring = null;
        foreach (Ring r in ring)
        {
            if (idx > pidx)
            {
                if ((r.idx <= idx) && (r.idx > pidx))
                {
                    ss.ring = r;
                    break;
                }
            }
            else
            {
                if ((r.idx <= idx) || (r.idx > pidx))
                {
                    ss.ring = r;
                    break;
                }
            }
        }

        if ((ss.ring != null))
            ss.ring.move();
        return ss;
    }

    public int sliceNum
    {
        get
        {
            return _sliceNum;
        }
    }
}

public class TorusPart
{
    public int _sliceNum;
    public SliceState _sliceState;
    public int sliceIdxFrom;
    public int sliceIdxTo;
    public SliceState blendedSliceState;
    public const float BLEND_DISTANCE = 64;
    public TorusPart()
    {
        _sliceState = new SliceState();
        blendedSliceState = new SliceState();
    }

    public void create(SliceState prev, int sliceIdx, int sn, Rand rand)
    {
        _sliceState.set_1(prev);
        _sliceState.changeDeg(rand);
        if (fabs(prev._mp) >= 1)
        {
            if (rand.nextInt(2) == 0)
            {
                if (prev._mp >= 1)
                {
                    _sliceState.changeToTightCurve_2(rand, -1);
                }
                else
                {
                    _sliceState.changeToTightCurve_2(rand, 1);
                }
            }
            else
            {
                _sliceState.changeToStraight();
            }
        }
        else if ((prev.courseWidth == prev.pointNum) || (rand.nextInt(2) == 0))
        {
            switch (rand.nextInt(3))
            {
                case 0:
                    _sliceState.changeRad(rand);
                    break;
                case 1:
                    _sliceState.changeWidth(rand);
                    break;
                case 2:
                    _sliceState.changeWidthToFull();
                    break;
            }
        }
        else
        {
            switch (rand.nextInt(4))
            {
                case 0:
                    _sliceState.changeToTightCurve_1(rand);
                    break;
                case 2:
                    _sliceState.changeToEasyCurve(rand);
                    break;
                default:
                    _sliceState.changeToStraight();
                    break;
            }
        }

        _sliceNum = sn;
        sliceIdxFrom = sliceIdx;
        sliceIdxTo = sliceIdx + _sliceNum;
    }

    public bool contains(int idx)
    {
        if ((idx >= sliceIdxFrom) && (idx < sliceIdxTo))
            return true;
        else
            return false;
    }

    public SliceState createBlendedSliceState(SliceState blendee, int idx)
    {
        int dst = idx - sliceIdxFrom;
        float blendRatio = (float)dst / BLEND_DISTANCE;
        if (blendRatio >= 1)
            return _sliceState;
        blendedSliceState.blend(_sliceState, blendee, blendRatio);
        return blendedSliceState;
    }

    public int sliceNum
    {
        get
        {
            return _sliceNum;
        }
    }

    public SliceState sliceState
    {
        get
        {
            return _sliceState;
        }
    }
}

public class SliceState
{
    public const int MAX_POINT_NUM = 36;
    public const int DEFAULT_POINT_NUM = 24;
    public const float DEFAULT_RAD = 21;
    public float _md1, _md2;
    public float _rad;
    public int _pointNum;
    public float _courseWidth;
    public float _mp;
    public Ring _ring;
    public SliceState()
    {
        init_0();
    }

    public void init_0()
    {
        {
            _md2 = 0;
            _md1 = _md2;
        }

        _rad = DEFAULT_RAD;
        _pointNum = DEFAULT_POINT_NUM;
        _courseWidth = _pointNum;
        _mp = 0;
        _ring = null;
    }

    public void changeDeg(Rand rand)
    {
        _md1 = rand.nextSignedFloat(0.005f);
        _md2 = rand.nextSignedFloat(0.005f);
    }

    public void changeRad(Rand rand)
    {
        _rad = DEFAULT_RAD + rand.nextSignedFloat(DEFAULT_RAD * 0.3f);
        int ppn = _pointNum;
        _pointNum = GameMath.integer((_rad * DEFAULT_POINT_NUM / DEFAULT_RAD));
        if (ppn == _courseWidth)
            changeWidthToFull();
        else
            _courseWidth = _courseWidth * _pointNum / ppn;
    }

    public void changeWidth(Rand rand)
    {
        _courseWidth = rand.nextInt(GameMath.integer(_pointNum / 4)) + _pointNum * 0.36f;
    }

    public void changeWidthToFull()
    {
        _courseWidth = _pointNum;
    }

    public void changeToStraight()
    {
        _mp = 0;
    }

    public void changeToEasyCurve(Rand rand)
    {
        _mp = rand.nextFloat(0.05f) + 0.04f;
        if (rand.nextInt(2) == 0)
            _mp = -_mp;
    }

    public void changeToTightCurve_1(Rand rand)
    {
        changeToTightCurve_2(rand, rand.nextInt(2) * 2 - 1);
    }

    public void changeToTightCurve_2(Rand rand, int dir)
    {
        _mp = (rand.nextFloat(0.04f) + 0.1f) * dir;
    }

    public void blend(SliceState s1, SliceState s2, float ratio)
    {
        _md1 = s1.md1 * ratio + s2.md1 * (1 - ratio);
        _md2 = s1.md2 * ratio + s2.md2 * (1 - ratio);
        _rad = s1.rad * ratio + s2.rad * (1 - ratio);
        _pointNum = GameMath.integer((s1.pointNum * ratio + s2.pointNum * (1 - ratio)));
        if ((s1.courseWidth == s1._pointNum) && (s2.courseWidth == s2._pointNum))
            _courseWidth = _pointNum;
        else
            _courseWidth = s1.courseWidth * ratio + s2.courseWidth * (1 - ratio);
        _mp = s1.mp;
    }

    public void set_1(SliceState s)
    {
        _md1 = s.md1;
        _md2 = s.md2;
        _rad = s.rad;
        _pointNum = s.pointNum;
        _courseWidth = s.courseWidth;
        _mp = s.mp;
        _ring = s.ring;
    }

    public float md1
    {
        get
        {
            return _md1;
        }
    }

    public float md2
    {
        get
        {
            return _md2;
        }
    }

    public float rad
    {
        get
        {
            return _rad;
        }
    }

    public int pointNum
    {
        get
        {
            return _pointNum;
        }
    }

    public float courseWidth
    {
        get
        {
            return _courseWidth;
        }
    }

    public float mp
    {
        get
        {
            return _mp;
        }
    }

    public Ring ring
    {
        get
        {
            return _ring;
        }

        set
        {
            _ring = value;
        }
    }
}

public class Ring
{
    public static float[][] COLOR_RGB = new float[][]
    {
        new float[] { 0.5f, 1, 0.9f },
        new float[] { 1, 0.9f, 0.5f }
    };
    public int _idx;
    static int nextMesh;
    public Mesh[] meshes;
    public int cnt;
    public int clr;
    public int type;
    public Ring(int idx, SliceState ss, int type = 0)
    {
        _idx = idx;
        cnt = 0;
        this.type = type;
        float r = ss.rad;
        switch (type)
        {
            case 0:
                createNormalRing(r);
                break;
            case 1:
                createFinalRing(r);
                break;
        }
    }

    public void createNormalRing(float r)
    {
        float[] model = Transform.Identity(); float[] tint = null; Mesh mesh = null; int meshIndex = 0;
        meshes = new Mesh[1];
        mesh = new Mesh("Ring-" + nextMesh.ToString()); nextMesh++; meshes[meshIndex] = mesh; model = Transform.Identity(); tint = null;
        appendRing(mesh, model, tint, r, 1.2f, 1.4f, 16);

    }

    public void createFinalRing(float r)
    {
        float[] model = Transform.Identity(); float[] tint = null; Mesh mesh = null; int meshIndex = 0;
        meshes = new Mesh[2];
        mesh = new Mesh("Ring-" + nextMesh.ToString()); nextMesh++; meshes[meshIndex] = mesh; model = Transform.Identity(); tint = null;
        appendRing(mesh, model, tint, r, 1.2f, 1.5f, 14);
        meshIndex++; mesh = new Mesh("Ring-" + nextMesh.ToString()); nextMesh++; meshes[meshIndex] = mesh; model = Transform.Identity(); tint = null;
        appendRing(mesh, model, tint, r, 1.6f, 1.9f, 14);

    }

    public void appendRing(Mesh mesh, float[] model, float[] tint, float r, float rr1, float rr2, int num)
    {
        float d = 0, md = 0.2f;
        for (int i = 0; i < num; i++)
        {
            int part1 = mesh.vertexCount;
            Vector3 p1 = new Vector3(sin(d) * r * rr1, cos(d) * r * rr1, 0);
            Vector3 p2 = new Vector3(sin(d) * r * rr2, cos(d) * r * rr2, 0);
            Vector3 p3 = new Vector3(sin(d + md) * r * rr2, cos(d + md) * r * rr2, 0);
            Vector3 p4 = new Vector3(sin(d + md) * r * rr1, cos(d + md) * r * rr1, 0);
            Vector3 cp = new Vector3();
            cp.opAddAssign(p1);
            cp.opAddAssign(p2);
            cp.opAddAssign(p3);
            cp.opAddAssign(p4);
            cp.opDivAssign(4);
            Vector3 np1 = new Vector3();
            Vector3 np2 = new Vector3();
            Vector3 np3 = new Vector3();
            Vector3 np4 = new Vector3();
            np1.blend(p1, cp, 0.7f);
            np2.blend(p2, cp, 0.7f);
            np3.blend(p3, cp, 0.7f);
            np4.blend(p4, cp, 0.7f);
            mesh.Vertex(np1.x, np1.y, np1.z, tint, model);
            mesh.Vertex(np2.x, np2.y, np2.z, tint, model);
            mesh.Vertex(np3.x, np3.y, np3.z, tint, model);
            mesh.Vertex(np4.x, np4.y, np4.z, tint, model);
            mesh.LineStrip(part1, mesh.vertexCount - part1, true);
            d = d + (md);
        }
    }

    public void close()
    {
        meshes = null;
    }

    public void move()
    {
        cnt++;
    }

    public void draw(float[] model, float[] tint, Gfx.Blend blend, Gfx.Cull cull, float lineWidth, float a, Tunnel tunnel)
    {
        blend = Gfx.Blend.Additive;
        float d1 = 0, d2 = 0;
        Vector angles = new Vector();
        Vector3 p = tunnel.getCenterPos(_idx, angles);
        d1 = angles.x;
        d2 = angles.y;
        tint = new float[] { COLOR_RGB[type][0] * a, COLOR_RGB[type][1] * a, COLOR_RGB[type][2] * a, 1 };
        float[] parent1 = model;
        model = Transform.Translate(model, p.x, p.y, p.z);
        model = Transform.Rotate(model, cnt * 1.0f, 0, 0, 1);
        model = Transform.Rotate(model, d1, 0, 1, 0);
        model = Transform.Rotate(model, d2, 1, 0, 0);
        { Mesh shape2 = meshes[0]; if (shape2.count > 0) Gfx.Draw(shape2.count, shape2.Bindings(model, tint, lineWidth, blend == Gfx.Blend.Additive),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = cull, Blend = blend }); }
        model = parent1;
        if (type == 1)
        {
            float[] parent3 = model;
            model = Transform.Translate(model, p.x, p.y, p.z);
            model = Transform.Rotate(model, cnt * -1.0f, 0, 0, 1);
            model = Transform.Rotate(model, d1, 0, 1, 0);
            model = Transform.Rotate(model, d2, 1, 0, 0);
            { Mesh shape4 = meshes[1]; if (shape4.count > 0) Gfx.Draw(shape4.count, shape4.Bindings(model, tint, lineWidth, blend == Gfx.Blend.Additive),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = cull, Blend = blend }); }
            model = parent3;
        }

        blend = Gfx.Blend.Alpha;
    }

    public int idx
    {
        get
        {
            return _idx;
        }
    }
}
