// Copyright 2004 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;

public class Camera
{
    public const int ZOOM_CNT = 24;
    public Ship ship;
    public Rand rand;
    public Vector3 _cameraPos, cameraTrg, cameraVel;
    public Vector3 _lookAtPos, lookAtOfs;
    public float lookAtCnt, changeCnt, moveCnt;
    public float _deg;
    public float _zoom;
    public float zoomTrg, zoomMin;
    public int type;
    public Camera(Ship ship)
    {
        this.ship = ship;
        _cameraPos = new Vector3();
        cameraTrg = new Vector3();
        cameraVel = new Vector3();
        _lookAtPos = new Vector3();
        lookAtOfs = new Vector3();
        {
            zoomTrg = 1;
            _zoom = zoomTrg;
        }

        zoomMin = 0.5f;
        rand = new Rand();
        type = CameraMoveType.FLOAT;
    }

    public void start()
    {
        changeCnt = 0;
        moveCnt = 0;
    }

    public void move()
    {
        changeCnt -= SimulationTime.Step;
        if (changeCnt < 0)
        {
            type = rand.nextInt(2);
            switch (type)
            {
                case CameraMoveType.FLOAT:
                    changeCnt = 256 + rand.nextInt(150);
                    cameraTrg.x = ship.relPos.x + rand.nextSignedFloat(1);
                    cameraTrg.y = ship.relPos.y - 12 + rand.nextSignedFloat(48);
                    cameraTrg.z = rand.nextInt(32);
                    cameraVel.x = (ship.relPos.x - cameraTrg.x) / changeCnt * (1 + rand.nextFloat(1));
                    cameraVel.y = (ship.relPos.y - 12 - cameraTrg.y) / changeCnt * (1.5f + rand.nextFloat(0.8f));
                    cameraVel.z = (16 - cameraTrg.z) / changeCnt * rand.nextFloat(1);
                {
                    zoomTrg = 1.2f + rand.nextFloat(0.8f);
                    _zoom = zoomTrg;
                }

                    break;
                case CameraMoveType.FIX:
                    changeCnt = 200 + rand.nextInt(100);
                    cameraTrg.x = rand.nextSignedFloat(0.3f);
                    cameraTrg.y = -8 - rand.nextFloat(12);
                    cameraTrg.z = 8 + rand.nextInt(16);
                    cameraVel.x = (ship.relPos.x - cameraTrg.x) / changeCnt * (1 + rand.nextFloat(1));
                    cameraVel.y = rand.nextSignedFloat(0.05f);
                    cameraVel.z = (10 - cameraTrg.z) / changeCnt * rand.nextFloat(0.5f);
                    zoomTrg = 1.0f + rand.nextSignedFloat(0.25f);
                    _zoom = 0.2f + rand.nextFloat(0.8f);
                    break;
            }

            _cameraPos.x = cameraTrg.x;
            _cameraPos.y = cameraTrg.y;
            _cameraPos.z = cameraTrg.z;
            _deg = cameraTrg.x;
            lookAtOfs.x = 0;
            lookAtOfs.y = 0;
            lookAtOfs.z = 0;
            lookAtCnt = 0;
            zoomMin = 1.0f - rand.nextFloat(0.9f);
        }

        lookAtCnt -= SimulationTime.Step;
        if (SimulationTime.Crossed(lookAtCnt, ZOOM_CNT))
        {
            lookAtOfs.x = rand.nextSignedFloat(0.4f);
            lookAtOfs.y = rand.nextSignedFloat(3);
            lookAtOfs.z = rand.nextSignedFloat(10);
        }
        else if (lookAtCnt < 0)
        {
            lookAtCnt = 32 + rand.nextInt(48);
        }

        cameraTrg.x += cameraVel.x * SimulationTime.Step;
        cameraTrg.y += cameraVel.y * SimulationTime.Step;
        cameraTrg.z += cameraVel.z * SimulationTime.Step;
        float cox = 0, coy = 0, coz = 0;
        switch (type)
        {
            case CameraMoveType.FLOAT:
                cox = cameraTrg.x;
                coy = cameraTrg.y;
                coz = cameraTrg.z;
                break;
            case CameraMoveType.FIX:
                cox = cameraTrg.x + ship.relPos.x;
                coy = cameraTrg.y + ship.relPos.y;
                coz = cameraTrg.z;
                float od = ship.relPos.x - _deg;
                while (od >= PI)
                    od = od - (PI * 2);
                while (od < -PI)
                    od = od + (PI * 2);
                _deg = _deg + (od * SimulationTime.Blend(0.2f));
                break;
        }

        cox = cox - (cameraPos.x);
        while (cox >= PI)
            cox = cox - (PI * 2);
        while (cox < -PI)
            cox = cox + (PI * 2);
        coy = coy - (cameraPos.y);
        coz = coz - (cameraPos.z);
        _cameraPos.x = _cameraPos.x + (cox * SimulationTime.Blend(0.12f));
        _cameraPos.y = _cameraPos.y + (coy * SimulationTime.Blend(0.12f));
        _cameraPos.z = _cameraPos.z + (coz * SimulationTime.Blend(0.12f));
        float ofsRatio = 0;
        if (lookAtCnt <= ZOOM_CNT)
            ofsRatio = 1.0f + fabs(zoomTrg - _zoom) * 2.5f;
        else
            ofsRatio = 1.0f;
        float lox = ship.relPos.x + lookAtOfs.x * ofsRatio - _lookAtPos.x;
        while (lox >= PI)
            lox = lox - (PI * 2);
        while (lox < -PI)
            lox = lox + (PI * 2);
        float loy = ship.relPos.y + lookAtOfs.y * ofsRatio - _lookAtPos.y;
        float loz = lookAtOfs.z * ofsRatio - _lookAtPos.z;
        if (lookAtCnt <= ZOOM_CNT)
        {
            _zoom = _zoom + ((zoomTrg - _zoom) * SimulationTime.Blend(0.16f));
            _lookAtPos.x = _lookAtPos.x + (lox * SimulationTime.Blend(0.2f));
            _lookAtPos.y = _lookAtPos.y + (loy * SimulationTime.Blend(0.2f));
            _lookAtPos.z = _lookAtPos.z + (loz * SimulationTime.Blend(0.2f));
        }
        else
        {
            _lookAtPos.x = _lookAtPos.x + (lox * SimulationTime.Blend(0.1f));
            _lookAtPos.y = _lookAtPos.y + (lox * SimulationTime.Blend(0.1f));
            _lookAtPos.z = _lookAtPos.z + (loz * SimulationTime.Blend(0.1f));
        }

        lookAtOfs.opMulAssign(SimulationTime.Decay(0.985f));
        if (fabs(lookAtOfs.x) < 0.04f)
            lookAtOfs.x = 0;
        if (fabs(lookAtOfs.y) < 0.3f)
            lookAtOfs.y = 0;
        if (fabs(lookAtOfs.z) < 1)
            lookAtOfs.z = 0;
        moveCnt -= SimulationTime.Step;
        if (moveCnt < 0)
        {
            moveCnt = 15 + rand.nextInt(15);
            float lookDistance = fabs(_lookAtPos.x - _cameraPos.x);
            if (lookDistance > PI)
                lookDistance = PI * 2 - lookDistance;
            float ofs = lookDistance * 3 + fabs(_lookAtPos.y - _cameraPos.y);
            zoomTrg = 3.0f / ofs;
            if (zoomTrg < zoomMin)
                zoomTrg = zoomMin;
            else if (zoomTrg > 2)
                zoomTrg = 2;
        }

        if (_lookAtPos.x < 0)
            _lookAtPos.x = _lookAtPos.x + (PI * 2);
        else if (_lookAtPos.x >= PI * 2)
            _lookAtPos.x = _lookAtPos.x - (PI * 2);
    }

    public Vector3 cameraPos
    {
        get
        {
            return _cameraPos;
        }
    }

    public Vector3 lookAtPos
    {
        get
        {
            return _lookAtPos;
        }
    }

    public float deg
    {
        get
        {
            return _deg;
        }
    }

    public float zoom
    {
        get
        {
            return _zoom;
        }
    }
}

public static class CameraMoveType
{
    public const int FLOAT = 0, FIX = 1;
}
