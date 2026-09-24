// Copyright 2005 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Lub;

public class Ship
{
    public const float INITIAL_SCROLL_SPEED = 0.01f;
    public const float SCROLL_SPEED_MAX = 0.1f;
    public const float SCROLL_START_Y = 2.5f;
    public Field field;
    public Boat[] boat = new Boat[2];
    public int gameMode;
    public int boatNum;
    public InGameState gameState;
    public float scrollSpeed, _scrollSpeedBase;
    public Vector _midstPos, _higherPos, _lowerPos, _nearPos, _nearVel;
    public BaseShape bridgeShape;
    public Ship(Pad pad, TwinStick twinStick, Mouse mouse, RecordableMouseAndPad mouseAndPad, Field field, GrScreen screen, SparkPool sparks, SmokePool smokes, FragmentPool fragments, WakePool wakes)
    {
        this.field = field;
        Boat.init();
        int i = 0;
        for (int index0 = 0; index0 < 2; index0++)
        {
            boat[index0] = new Boat(i, this, pad, twinStick, mouse, mouseAndPad, field, screen, sparks, smokes, fragments, wakes);
            i++;
        }

        boatNum = 1;
        {
            _scrollSpeedBase = INITIAL_SCROLL_SPEED;
            scrollSpeed = _scrollSpeedBase;
        }

        _midstPos = new Vector();
        _higherPos = new Vector();
        _lowerPos = new Vector();
        _nearPos = new Vector();
        _nearVel = new Vector();
        bridgeShape = new BaseShape(0.3f, 0.2f, 0.1f, BaseShapeShapeType.BRIDGE, 0.3f, 0.7f, 0.7f);
    }

    public void setRandSeed(int seed)
    {
        Boat.setRandSeed(seed);
    }

    public void close()
    {
        foreach (Boat b in boat)
            b.close();
    }

    public void setShots(ShotPool shots)
    {
        foreach (Boat b in boat)
            b.setShots(shots);
    }

    public void setEnemies(EnemyPool enemies)
    {
        foreach (Boat b in boat)
            b.setEnemies(enemies);
    }

    public void setStageManager(StageManager stageManager)
    {
        foreach (Boat b in boat)
            b.setStageManager(stageManager);
    }

    public void setGameState(InGameState gameState)
    {
        this.gameState = gameState;
        foreach (Boat b in boat)
            b.setGameState(gameState);
    }

    public void start(int gameMode)
    {
        this.gameMode = gameMode;
        if (gameMode == InGameStateGameMode.DOUBLE_PLAY)
            boatNum = 2;
        else
            boatNum = 1;
        _scrollSpeedBase = INITIAL_SCROLL_SPEED;
        for (int i = 0; i < boatNum; i++)
            boat[i].start(gameMode);
        {
            _midstPos.y = 0;
            _midstPos.x = _midstPos.y;
        }

        {
            _higherPos.y = 0;
            _higherPos.x = _higherPos.y;
        }

        {
            _lowerPos.y = 0;
            _lowerPos.x = _lowerPos.y;
        }

        {
            _nearPos.y = 0;
            _nearPos.x = _nearPos.y;
        }

        {
            _nearVel.y = 0;
            _nearVel.x = _nearVel.y;
        }

        restart();
    }

    public void restart()
    {
        scrollSpeed = _scrollSpeedBase;
        for (int i = 0; i < boatNum; i++)
            boat[i].restart();
    }

    public void move()
    {
        field.scroll(scrollSpeed);
        bool sf = false;
        for (int i = 0; i < boatNum; i++)
        {
            boat[i].move();
            if ((boat[i].hasCollision() && (boat[i].pos.x > field.size.x / 3)) && (boat[i].pos.y < -field.size.y / 4 * 3))
                sf = true;
        }

        if (sf)
            gameState.shrinkScoreReel();
        if (higherPos().y >= SCROLL_START_Y)
            scrollSpeed = scrollSpeed + ((SCROLL_SPEED_MAX - scrollSpeed) * 0.1f);
        else
            scrollSpeed = scrollSpeed + ((_scrollSpeedBase - scrollSpeed) * 0.1f);
        _scrollSpeedBase = _scrollSpeedBase + ((SCROLL_SPEED_MAX - _scrollSpeedBase) * 0.00001f);
    }

    public bool checkBulletHit(Vector p, Vector pp)
    {
        for (int i = 0; i < boatNum; i++)
            if (boat[i].checkBulletHit(p, pp))
                return true;
        return false;
    }

    public void clearBullets()
    {
        gameState.clearBullets();
    }

    public void destroyed()
    {
        for (int i = 0; i < boatNum; i++)
            boat[i].destroyedBoat();
    }

    public void draw(float[] model)
    {
        float[] color = null;
        for (int i = 0; i < boatNum; i++)
            boat[i].draw(model);
        if ((gameMode == InGameStateGameMode.DOUBLE_PLAY) && boat[0].hasCollision())
        {
            color = new float[] { 0.5f, 0.5f, 0.9f, 0.8f };
            var geometry1 = new Mesh("ship-link");
            int first1 = geometry1.vertexCount;
            geometry1.Vertex(boat[0].pos.x, boat[0].pos.y, 0, color);
            color = new float[] { 0.5f, 0.5f, 0.9f, 0.3f };
            geometry1.Vertex(midstPos().x, midstPos().y, 0, color);
            color = new float[] { 0.5f, 0.5f, 0.9f, 0.8f };
            geometry1.Vertex(boat[1].pos.x, boat[1].pos.y, 0, color);
            geometry1.LineStrip(first1, geometry1.vertexCount - first1);
            Gfx.Draw(geometry1.count, geometry1.Bindings(model, null, 1, true), new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = Gfx.Blend.Additive });
            float[] parent1 = model;
            model = Transform.Translate(model, midstPos().x, midstPos().y, 0);
            model = Transform.Rotate(model, -degAmongBoats() * 180 / PI, 0, 0, 1);
            bridgeShape.draw(model);
            model = parent1;
        }
    }

    public void drawFront(float[] model)
    {
        for (int i = 0; i < boatNum; i++)
            boat[i].drawFront(model);
    }

    public void drawShape(float[] model)
    {
        boat[0].drawShape(model);
    }

    public float scrollSpeedBase
    {
        get
        {
            return _scrollSpeedBase;
        }

        set
        {
            _scrollSpeedBase = value;
        }
    }

    public void setReplayMode(float turnSpeed, bool reverseFire)
    {
        foreach (Boat b in boat)
            b.setReplayMode(turnSpeed, reverseFire);
    }

    public void unsetReplayMode()
    {
        foreach (Boat b in boat)
            b.unsetReplayMode();
    }

    public bool replayMode()
    {
        return boat[0].replayMode;
    }

    public Vector midstPos()
    {
        {
            _midstPos.y = 0;
            _midstPos.x = _midstPos.y;
        }

        for (int i = 0; i < boatNum; i++)
        {
            _midstPos.x = _midstPos.x + (boat[i].pos.x);
            _midstPos.y = _midstPos.y + (boat[i].pos.y);
        }

        _midstPos.opDivAssign(boatNum);
        return _midstPos;
    }

    public Vector higherPos()
    {
        _higherPos.y = -99999;
        for (int i = 0; i < boatNum; i++)
        {
            if (boat[i].pos.y > _higherPos.y)
            {
                _higherPos.x = boat[i].pos.x;
                _higherPos.y = boat[i].pos.y;
            }
        }

        return _higherPos;
    }

    public Vector lowerPos()
    {
        _lowerPos.y = 99999;
        for (int i = 0; i < boatNum; i++)
        {
            if (boat[i].pos.y < _lowerPos.y)
            {
                _lowerPos.x = boat[i].pos.x;
                _lowerPos.y = boat[i].pos.y;
            }
        }

        return _lowerPos;
    }

    public Vector nearPos(Vector p)
    {
        float dist = 99999;
        for (int i = 0; i < boatNum; i++)
        {
            if (boat[i].pos.dist_1(p) < dist)
            {
                dist = boat[i].pos.dist_1(p);
                _nearPos.x = boat[i].pos.x;
                _nearPos.y = boat[i].pos.y;
            }
        }

        return _nearPos;
    }

    public Vector nearVel(Vector p)
    {
        float dist = 99999;
        for (int i = 0; i < boatNum; i++)
        {
            if (boat[i].pos.dist_1(p) < dist)
            {
                dist = boat[i].pos.dist_1(p);
                _nearVel.x = boat[i].vel.x;
                _nearVel.y = boat[i].vel.y;
            }
        }

        return _nearVel;
    }

    public float distAmongBoats()
    {
        return boat[0].pos.dist_1(boat[1].pos);
    }

    public float degAmongBoats()
    {
        if (distAmongBoats() < 0.1f)
            return 0;
        else
            return atan2(boat[0].pos.x - boat[1].pos.x, boat[0].pos.y - boat[1].pos.y);
    }
}

public class Boat
{
    public const int RESTART_CNT = 300;
    public const int INVINCIBLE_CNT = 228;
    public const float HIT_WIDTH = 0.02f;
    public const int INITIAL_FIRE_INTERVAL = 2;
    public const int INITIAL_FIRE_INTERVAL_MAX = 4;
    public const int FIRE_LANCE_INTERVAL = 15;
    public const float SPEED_BASE = 0.15f;
    public const float TURN_RATIO_BASE = 0.2f;
    public const float SLOW_TURN_RATIO = 0;
    public const float TURN_CHANGE_RATIO = 0.5f;
    public static GunroarRand rand;
    public static PadState padInput;
    public static TwinStickState stickInput;
    public static MouseState mouseInput;
    public RecordablePad pad;
    public RecordableTwinStick twinStick;
    public RecordableMouse mouse;
    public RecordableMouseAndPad mouseAndPad;
    public Field field;
    public GrScreen screen;
    public ShotPool shots;
    public SparkPool sparks;
    public SmokePool smokes;
    public FragmentPool fragments;
    public WakePool wakes;
    public EnemyPool enemies;
    public StageManager stageManager;
    public InGameState gameState;
    public Vector _pos;
    public Vector firePos;
    public float deg;
    public float speed;
    public float turnRatio;
    public BaseShape _shape;
    public BaseShape bridgeShape;
    public int fireCnt;
    public int fireSprCnt;
    public float fireInterval;
    public float fireSprDeg;
    public int fireLanceCnt;
    public float fireDeg;
    public bool aPressed, bPressed;
    public int cnt;
    public bool onBlock;
    public Vector _vel;
    public Vector refVel;
    public int shieldCnt;
    public ShieldShape shieldShape;
    public bool _replayMode;
    public float turnSpeed;
    public bool reverseFire;
    public int gameMode;
    public float vx, vy;
    public int idx;
    public Ship ship;
    public static void init()
    {
        rand = new GunroarRand();
    }

    public static void setRandSeed(int seed)
    {
        rand.setSeed(seed);
    }

    public Boat(int idx, Ship ship, Pad pad, TwinStick twinStick, Mouse mouse, RecordableMouseAndPad mouseAndPad, Field field, GrScreen screen, SparkPool sparks, SmokePool smokes, FragmentPool fragments, WakePool wakes)
    {
        this.idx = idx;
        this.ship = ship;
        this.pad = (RecordablePad)pad;
        this.twinStick = (RecordableTwinStick)twinStick;
        this.mouse = (RecordableMouse)mouse;
        this.mouseAndPad = mouseAndPad;
        this.field = field;
        this.screen = screen;
        this.sparks = sparks;
        this.smokes = smokes;
        this.fragments = fragments;
        this.wakes = wakes;
        _pos = new Vector();
        firePos = new Vector();
        _vel = new Vector();
        refVel = new Vector();
        switch (idx)
        {
            case 0:
            {
                _shape = new BaseShape(0.7f, 0.6f, 0.6f, BaseShapeShapeType.SHIP_ROUNDTAIL, 0.5f, 0.7f, 0.5f);
                bridgeShape = new BaseShape(0.3f, 0.6f, 0.6f, BaseShapeShapeType.BRIDGE, 0.3f, 0.7f, 0.3f);
                break;
            }

            case 1:
            {
                _shape = new BaseShape(0.7f, 0.6f, 0.6f, BaseShapeShapeType.SHIP_ROUNDTAIL, 0.4f, 0.3f, 0.8f);
                bridgeShape = new BaseShape(0.3f, 0.6f, 0.6f, BaseShapeShapeType.BRIDGE, 0.2f, 0.3f, 0.6f);
                break;
            }
        }

        deg = 0;
        speed = 0;
        turnRatio = 0;
        turnSpeed = 1;
        fireInterval = INITIAL_FIRE_INTERVAL;
        fireSprDeg = 0;
        cnt = 0;
        shieldCnt = 0;
        shieldShape = new ShieldShape();
    }

    public void close()
    {
        _shape.close();
        bridgeShape.close();
        shieldShape.close();
    }

    public void setShots(ShotPool shots)
    {
        this.shots = shots;
    }

    public void setEnemies(EnemyPool enemies)
    {
        this.enemies = enemies;
    }

    public void setStageManager(StageManager stageManager)
    {
        this.stageManager = stageManager;
    }

    public void setGameState(InGameState gameState)
    {
        this.gameState = gameState;
    }

    public void start(int gameMode)
    {
        this.gameMode = gameMode;
        if (gameMode == InGameStateGameMode.DOUBLE_PLAY)
        {
            switch (idx)
            {
                case 0:
                {
                    _pos.x = -field.size.x * 0.5f;
                    break;
                }

                case 1:
                {
                    _pos.x = field.size.x * 0.5f;
                    break;
                }
            }
        }
        else
        {
            _pos.x = 0;
        }

        _pos.y = -field.size.y * 0.8f;
        {
            firePos.y = 0;
            firePos.x = firePos.y;
        }

        {
            _vel.y = 0;
            _vel.x = _vel.y;
        }

        deg = 0;
        speed = SPEED_BASE;
        turnRatio = TURN_RATIO_BASE;
        cnt = -INVINCIBLE_CNT;
        {
            bPressed = true;
            aPressed = bPressed;
        }

        padInput = pad.getNullState();
        stickInput = twinStick.getNullState();
        mouseInput = mouse.getNullState();
    }

    public void restart()
    {
        switch (gameMode)
        {
            case InGameStateGameMode.NORMAL:
            {
                fireCnt = 99999;
                fireInterval = 99999;
                break;
            }

            case InGameStateGameMode.TWIN_STICK:
            case InGameStateGameMode.DOUBLE_PLAY:
            case InGameStateGameMode.MOUSE:
            {
                fireCnt = 0;
                fireInterval = INITIAL_FIRE_INTERVAL;
                break;
            }
        }

        fireSprCnt = 0;
        fireSprDeg = 0.5f;
        fireLanceCnt = 0;
        if (field.getBlock_1(_pos) >= 0)
            onBlock = true;
        else
            onBlock = false;
        {
            refVel.y = 0;
            refVel.x = refVel.y;
        }

        shieldCnt = 20 * 60;
    }

    public void move()
    {
        float px = _pos.x, py = _pos.y;
        cnt++;
        {
            vy = 0;
            vx = vy;
        }

        switch (gameMode)
        {
            case InGameStateGameMode.NORMAL:
            {
                moveNormal();
                break;
            }

            case InGameStateGameMode.TWIN_STICK:
            {
                moveTwinStick();
                break;
            }

            case InGameStateGameMode.DOUBLE_PLAY:
            {
                moveDoublePlay();
                break;
            }

            case InGameStateGameMode.MOUSE:
            {
                moveMouse();
                break;
            }
        }

        if (gameState.isGameOver)
        {
            clearBullets();
            if (cnt < -INVINCIBLE_CNT)
                cnt = -RESTART_CNT;
        }
        else if (cnt < -INVINCIBLE_CNT)
        {
            clearBullets();
        }

        vx = vx * (speed);
        vy = vy * (speed);
        vx = vx + (refVel.x);
        vy = vy + (refVel.y);
        refVel.opMulAssign(0.9f);
        if (field.checkInField_2(_pos.x, _pos.y - field.lastScrollY))
            _pos.y = _pos.y - (field.lastScrollY);
        if ((onBlock || (field.getBlock_2(_pos.x + vx, _pos.y) < 0)) && field.checkInField_2(_pos.x + vx, _pos.y))
        {
            _pos.x = _pos.x + (vx);
            _vel.x = vx;
        }
        else
        {
            _vel.x = 0;
            refVel.x = 0;
        }

        bool srf = false;
        if ((onBlock || (field.getBlock_2(px, _pos.y + vy) < 0)) && field.checkInField_2(_pos.x, _pos.y + vy))
        {
            _pos.y = _pos.y + (vy);
            _vel.y = vy;
        }
        else
        {
            _vel.y = 0;
            refVel.y = 0;
        }

        if (field.getBlock_2(_pos.x, _pos.y) >= 0)
        {
            if (!onBlock)
                if (cnt <= 0)
                    onBlock = true;
                else
                {
                    if (field.checkInField_2(_pos.x, _pos.y - field.lastScrollY))
                    {
                        _pos.x = px;
                        _pos.y = py;
                    }
                    else
                    {
                        destroyed();
                    }
                }
        }
        else
        {
            onBlock = false;
        }

        switch (gameMode)
        {
            case InGameStateGameMode.NORMAL:
            {
                fireNormal();
                break;
            }

            case InGameStateGameMode.TWIN_STICK:
            {
                fireTwinStick();
                break;
            }

            case InGameStateGameMode.DOUBLE_PLAY:
            {
                fireDobulePlay();
                break;
            }

            case InGameStateGameMode.MOUSE:
            {
                fireMouse();
                break;
            }
        }

        if ((cnt % 3 == 0) && (cnt >= -INVINCIBLE_CNT))
        {
            float sp = 0;
            if ((vx != 0) || (vy != 0))
                sp = 0.4f;
            else
                sp = 0.2f;
            sp = sp * (1 + rand.nextSignedFloat(0.33f));
            sp = sp * (SPEED_BASE);
            _shape.addWake(wakes, _pos, deg, sp);
        }

        Enemy he = enemies.checkHitShip(pos.x, pos.y);
        if (he != null)
        {
            float rd = 0;
            if (pos.dist_1(he.pos()) < 0.1f)
                rd = 0;
            else
                rd = atan2(_pos.x - he.pos().x, _pos.y - he.pos().y);
            float sz = he.size();
            refVel.x = sin(rd) * sz * 0.1f;
            refVel.y = cos(rd) * sz * 0.1f;
            float rs = refVel.vctSize();
            if (rs > 1)
            {
                refVel.x = refVel.x / (rs);
                refVel.y = refVel.y / (rs);
            }
        }

        if (shieldCnt > 0)
            shieldCnt--;
    }

    public void moveNormal()
    {
        if (!_replayMode)
        {
            padInput = pad.getState();
        }
        else
        {
            if (pad.inputRecord.hasNext())
            {
                padInput = pad.replay();
            }
            else
            {
                gameState.isGameOver = true;
                padInput = pad.getNullState();
            }
        }

        if (gameState.isGameOver || (cnt < -INVINCIBLE_CNT))
            padInput.clear();
        if ((padInput.dir & PadStateDir.UP) != 0)
            vy = 1;
        if ((padInput.dir & PadStateDir.DOWN) != 0)
            vy = -1;
        if ((padInput.dir & PadStateDir.RIGHT) != 0)
            vx = 1;
        if ((padInput.dir & PadStateDir.LEFT) != 0)
            vx = -1;
        if ((vx != 0) && (vy != 0))
        {
            vx = vx * (0.7f);
            vy = vy * (0.7f);
        }

        if ((vx != 0) || (vy != 0))
        {
            float ad = atan2(vx, vy);
            ad = normalizeDeg(ad);
            ad = ad - (deg);
            ad = normalizeDeg(ad);
            deg = deg + (ad * turnRatio * turnSpeed);
            deg = normalizeDeg(deg);
        }
    }

    public void moveTwinStick()
    {
        if (!_replayMode)
        {
            stickInput = twinStick.getState();
        }
        else
        {
            if (twinStick.inputRecord.hasNext())
            {
                stickInput = twinStick.replay();
            }
            else
            {
                gameState.isGameOver = true;
                stickInput = twinStick.getNullState();
            }
        }

        if (gameState.isGameOver || (cnt < -INVINCIBLE_CNT))
            stickInput.clear();
        vx = stickInput.left.x;
        vy = stickInput.left.y;
        if ((vx != 0) || (vy != 0))
        {
            float ad = atan2(vx, vy);
            ad = normalizeDeg(ad);
            ad = ad - (deg);
            ad = normalizeDeg(ad);
            deg = deg + (ad * turnRatio * turnSpeed);
            deg = normalizeDeg(deg);
        }
    }

    public void moveDoublePlay()
    {
        switch (idx)
        {
            case 0:
            {
                if (!_replayMode)
                {
                    stickInput = twinStick.getState();
                }
                else
                {
                    if (twinStick.inputRecord.hasNext())
                    {
                        stickInput = twinStick.replay();
                    }
                    else
                    {
                        gameState.isGameOver = true;
                        stickInput = twinStick.getNullState();
                    }
                }

                if (gameState.isGameOver || (cnt < -INVINCIBLE_CNT))
                    stickInput.clear();
                vx = stickInput.left.x;
                vy = stickInput.left.y;
                break;
            }

            case 1:
            {
                vx = stickInput.right.x;
                vy = stickInput.right.y;
                break;
            }
        }

        if ((vx != 0) || (vy != 0))
        {
            float ad = atan2(vx, vy);
            ad = normalizeDeg(ad);
            ad = ad - (deg);
            ad = normalizeDeg(ad);
            deg = deg + (ad * turnRatio * turnSpeed);
            deg = normalizeDeg(deg);
        }
    }

    public void moveMouse()
    {
        if (!_replayMode)
        {
            MouseAndPadState mps = mouseAndPad.getState();
            padInput = mps.padState;
            mouseInput = mps.mouseState;
        }
        else
        {
            if (mouseAndPad.inputRecord.hasNext())
            {
                MouseAndPadState mps = mouseAndPad.replay();
                padInput = mps.padState;
                mouseInput = mps.mouseState;
            }
            else
            {
                gameState.isGameOver = true;
                padInput = pad.getNullState();
                mouseInput = mouse.getNullState();
            }
        }

        if (gameState.isGameOver || (cnt < -INVINCIBLE_CNT))
        {
            padInput.clear();
            mouseInput.clear();
        }

        if ((padInput.dir & PadStateDir.UP) != 0)
            vy = 1;
        if ((padInput.dir & PadStateDir.DOWN) != 0)
            vy = -1;
        if ((padInput.dir & PadStateDir.RIGHT) != 0)
            vx = 1;
        if ((padInput.dir & PadStateDir.LEFT) != 0)
            vx = -1;
        if ((vx != 0) && (vy != 0))
        {
            vx = vx * (0.7f);
            vy = vy * (0.7f);
        }

        if ((vx != 0) || (vy != 0))
        {
            float ad = atan2(vx, vy);
            ad = normalizeDeg(ad);
            ad = ad - (deg);
            ad = normalizeDeg(ad);
            deg = deg + (ad * turnRatio * turnSpeed);
            deg = normalizeDeg(deg);
        }
    }

    public void fireNormal()
    {
        if ((padInput.button & PadStateButton.A) != 0)
        {
            turnRatio = turnRatio + ((SLOW_TURN_RATIO - turnRatio) * TURN_CHANGE_RATIO);
            fireInterval = INITIAL_FIRE_INTERVAL;
            if (!aPressed)
            {
                fireCnt = 0;
                aPressed = true;
            }
        }
        else
        {
            turnRatio = turnRatio + ((TURN_RATIO_BASE - turnRatio) * TURN_CHANGE_RATIO);
            aPressed = false;
            fireInterval = fireInterval * (1.033f);
            if (fireInterval > INITIAL_FIRE_INTERVAL_MAX)
                fireInterval = 99999;
        }

        fireDeg = deg;
        if (reverseFire)
            fireDeg = fireDeg + (PI);
        if (fireCnt <= 0)
        {
            SoundManager.playSe("shot.wav");
            Shot s = shots.getInstance();
            int foc = (fireSprCnt % 2) * 2 - 1;
            firePos.x = _pos.x + cos(fireDeg + PI) * 0.2f * foc;
            firePos.y = _pos.y - sin(fireDeg + PI) * 0.2f * foc;
            if (s != null)
                s.set(firePos, fireDeg);
            fireCnt = GameMath.integer(fireInterval);
            float td = 0;
            switch (foc)
            {
                case -1:
                {
                    td = fireSprDeg * (fireSprCnt / 2 % 4 + 1) * 0.2f;
                    break;
                }

                case 1:
                {
                    td = -fireSprDeg * (fireSprCnt / 2 % 4 + 1) * 0.2f;
                    break;
                }
            }

            fireSprCnt++;
            s = shots.getInstance();
            if (s != null)
                s.set(firePos, fireDeg + td);
            Smoke sm = smokes.getInstanceForced();
            float sd = fireDeg + td / 2;
            sm.set_7(firePos, sin(sd) * Shot.SPEED * 0.33f, cos(sd) * Shot.SPEED * 0.33f, 0, SmokeSmokeType.SPARK, 10, 0.33f);
        }

        fireCnt--;
        if ((padInput.button & PadStateButton.B) != 0)
        {
            if (((!bPressed) && (fireLanceCnt <= 0)) && (!shots.existsLance()))
            {
                SoundManager.playSe("lance.wav");
                float fd = deg;
                if (reverseFire)
                    fd = fd + (PI);
                Shot s = shots.getInstance();
                if (s != null)
                    s.set(pos, fd, true);
                for (int i = 0; i < 4; i++)
                {
                    Smoke sm = smokes.getInstanceForced();
                    float sd = fd + rand.nextSignedFloat(1);
                    sm.set_7(pos, sin(sd) * Shot.LANCE_SPEED * i * 0.2f, cos(sd) * Shot.LANCE_SPEED * i * 0.2f, 0, SmokeSmokeType.SPARK, 15, 0.5f);
                }

                fireLanceCnt = FIRE_LANCE_INTERVAL;
            }

            bPressed = true;
        }
        else
        {
            bPressed = false;
        }

        fireLanceCnt--;
    }

    public void fireTwinStick()
    {
        if (fabs(stickInput.right.x) + fabs(stickInput.right.y) > 0.01f)
        {
            fireDeg = atan2(stickInput.right.x, stickInput.right.y);
            if (fireCnt <= 0)
            {
                SoundManager.playSe("shot.wav");
                int foc = (fireSprCnt % 2) * 2 - 1;
                float rsd = stickInput.right.vctSize();
                if (rsd > 1)
                    rsd = 1;
                fireSprDeg = 1 - rsd + 0.05f;
                firePos.x = _pos.x + cos(fireDeg + PI) * 0.2f * foc;
                firePos.y = _pos.y - sin(fireDeg + PI) * 0.2f * foc;
                fireCnt = GameMath.integer(fireInterval);
                float td = 0;
                switch (foc)
                {
                    case -1:
                    {
                        td = fireSprDeg * (fireSprCnt / 2 % 4 + 1) * 0.2f;
                        break;
                    }

                    case 1:
                    {
                        td = -fireSprDeg * (fireSprCnt / 2 % 4 + 1) * 0.2f;
                        break;
                    }
                }

                fireSprCnt++;
                Shot s = shots.getInstance();
                if (s != null)
                    s.set(firePos, fireDeg + td / 2, false, 2);
                s = shots.getInstance();
                if (s != null)
                    s.set(firePos, fireDeg + td, false, 2);
                Smoke sm = smokes.getInstanceForced();
                float sd = fireDeg + td / 2;
                sm.set_7(firePos, sin(sd) * Shot.SPEED * 0.33f, cos(sd) * Shot.SPEED * 0.33f, 0, SmokeSmokeType.SPARK, 10, 0.33f);
            }
        }
        else
        {
            fireDeg = 99999;
        }

        fireCnt--;
    }

    public void fireDobulePlay()
    {
        if (gameState.isGameOver || (cnt < -INVINCIBLE_CNT))
            return;
        float dist = ship.distAmongBoats();
        fireInterval = INITIAL_FIRE_INTERVAL + 10.0f / (dist + 0.005f);
        if (dist < 2)
            fireInterval = 99999;
        else if (dist < 4)
            fireInterval = fireInterval * (3);
        else if (dist < 6)
            fireInterval = fireInterval * (1.6f);
        if (fireCnt > fireInterval)
            fireCnt = GameMath.integer(fireInterval);
        if (fireCnt <= 0)
        {
            SoundManager.playSe("shot.wav");
            int foc = (fireSprCnt % 2) * 2 - 1;
            fireDeg = 0;
            firePos.x = _pos.x + cos(fireDeg + PI) * 0.2f * foc;
            firePos.y = _pos.y - sin(fireDeg + PI) * 0.2f * foc;
            Shot s = shots.getInstance();
            if (s != null)
                s.set(firePos, fireDeg, false, 2);
            fireCnt = GameMath.integer(fireInterval);
            Smoke sm = smokes.getInstanceForced();
            float sd = fireDeg;
            sm.set_7(firePos, sin(sd) * Shot.SPEED * 0.33f, cos(sd) * Shot.SPEED * 0.33f, 0, SmokeSmokeType.SPARK, 10, 0.33f);
            if (idx == 0)
            {
                float fd = ship.degAmongBoats() + PI / 2;
                float td = 0;
                switch (foc)
                {
                    case -1:
                    {
                        td = fireSprDeg * (fireSprCnt / 2 % 4 + 1) * 0.15f;
                        break;
                    }

                    case 1:
                    {
                        td = -fireSprDeg * (fireSprCnt / 2 % 4 + 1) * 0.15f;
                        break;
                    }
                }

                firePos.x = ship.midstPos().x + cos(fd + PI) * 0.2f * foc;
                firePos.y = ship.midstPos().y - sin(fd + PI) * 0.2f * foc;
                s = shots.getInstance();
                if (s != null)
                    s.set(firePos, fd, false, 2);
                s = shots.getInstance();
                if (s != null)
                    s.set(firePos, fd + td, false, 2);
                sm = smokes.getInstanceForced();
                sm.set_7(firePos, sin(fd + td / 2) * Shot.SPEED * 0.33f, cos(fd + td / 2) * Shot.SPEED * 0.33f, 0, SmokeSmokeType.SPARK, 10, 0.33f);
            }

            fireSprCnt++;
        }

        fireCnt--;
    }

    public void fireMouse()
    {
        float fox = mouseInput.x - _pos.x;
        float foy = mouseInput.y - _pos.y;
        if (fabs(fox) < 0.01f)
            fox = 0.01f;
        if (fabs(foy) < 0.01f)
            foy = 0.01f;
        fireDeg = atan2(fox, foy);
        if ((mouseInput.button & (MouseStateButton.LEFT | MouseStateButton.RIGHT)) != 0)
        {
            if (fireCnt <= 0)
            {
                SoundManager.playSe("shot.wav");
                int foc = (fireSprCnt % 2) * 2 - 1;
                float rsd = stickInput.right.vctSize();
                float fstd = 0.05f;
                if ((mouseInput.button & MouseStateButton.RIGHT) != 0)
                    fstd = fstd + (0.5f);
                fireSprDeg = fireSprDeg + ((fstd - fireSprDeg) * 0.16f);
                firePos.x = _pos.x + cos(fireDeg + PI) * 0.2f * foc;
                firePos.y = _pos.y - sin(fireDeg + PI) * 0.2f * foc;
                fireCnt = GameMath.integer(fireInterval);
                float td = 0;
                switch (foc)
                {
                    case -1:
                    {
                        td = fireSprDeg * (fireSprCnt / 2 % 4 + 1) * 0.2f;
                        break;
                    }

                    case 1:
                    {
                        td = -fireSprDeg * (fireSprCnt / 2 % 4 + 1) * 0.2f;
                        break;
                    }
                }

                fireSprCnt++;
                Shot s = shots.getInstance();
                if (s != null)
                    s.set(firePos, fireDeg + td / 2, false, 2);
                s = shots.getInstance();
                if (s != null)
                    s.set(firePos, fireDeg + td, false, 2);
                Smoke sm = smokes.getInstanceForced();
                float sd = fireDeg + td / 2;
                sm.set_7(firePos, sin(sd) * Shot.SPEED * 0.33f, cos(sd) * Shot.SPEED * 0.33f, 0, SmokeSmokeType.SPARK, 10, 0.33f);
            }
        }

        fireCnt--;
    }

    public bool checkBulletHit(Vector p, Vector pp)
    {
        if (cnt <= 0)
            return false;
        float bmvx = 0, bmvy = 0, inaa = 0;
        bmvx = pp.x;
        bmvy = pp.y;
        bmvx = bmvx - (p.x);
        bmvy = bmvy - (p.y);
        inaa = bmvx * bmvx + bmvy * bmvy;
        if (inaa > 0.00001f)
        {
            float sofsx = 0, sofsy = 0, inab = 0, hd = 0;
            sofsx = _pos.x;
            sofsy = _pos.y;
            sofsx = sofsx - (p.x);
            sofsy = sofsy - (p.y);
            inab = bmvx * sofsx + bmvy * sofsy;
            if ((inab >= 0) && (inab <= inaa))
            {
                hd = sofsx * sofsx + sofsy * sofsy - inab * inab / inaa;
                if ((hd >= 0) && (hd <= HIT_WIDTH))
                {
                    destroyed();
                    return true;
                }
            }
        }

        return false;
    }

    public void destroyed()
    {
        if (cnt <= 0)
            return;
        if (shieldCnt > 0)
        {
            destroyedBoatShield();
            return;
        }

        ship.destroyed();
        gameState.shipDestroyed();
    }

    public void destroyedBoatShield()
    {
        for (int i = 0; i < 100; i++)
        {
            Spark sp = sparks.getInstanceForced();
            sp.set(pos, rand.nextSignedFloat(1), rand.nextSignedFloat(1), 0.5f + rand.nextFloat(0.5f), 0.5f + rand.nextFloat(0.5f), 0, 40 + rand.nextInt(40));
        }

        SoundManager.playSe("ship_shield_lost.wav");
        screen.setScreenShake(30, 0.02f);
        shieldCnt = 0;
        cnt = -INVINCIBLE_CNT / 2;
    }

    public void destroyedBoat()
    {
        for (int i = 0; i < 128; i++)
        {
            Spark sp = sparks.getInstanceForced();
            sp.set(pos, rand.nextSignedFloat(1), rand.nextSignedFloat(1), 0.5f + rand.nextFloat(0.5f), 0.5f + rand.nextFloat(0.5f), 0, 40 + rand.nextInt(40));
        }

        SoundManager.playSe("ship_destroyed.wav");
        for (int i = 0; i < 64; i++)
        {
            Smoke s = smokes.getInstanceForced();
            s.set_7(pos, rand.nextSignedFloat(0.2f), rand.nextSignedFloat(0.2f), rand.nextFloat(0.1f), SmokeSmokeType.EXPLOSION, 50 + rand.nextInt(30), 1);
        }

        screen.setScreenShake(60, 0.05f);
        restart();
        cnt = -RESTART_CNT;
    }

    public bool hasCollision()
    {
        if (cnt < -INVINCIBLE_CNT)
            return false;
        else
            return true;
    }

    public void draw(float[] model)
    {
        float[] color = null;
        if (cnt < -INVINCIBLE_CNT)
            return;
        if (fireDeg < 99999)
        {
            color = new float[] { 0.5f, 0.9f, 0.7f, 0.4f };
            var geometry1 = new Mesh("boat-aim-" + idx.ToString());
            int first1 = geometry1.vertexCount;
            geometry1.Vertex(_pos.x, _pos.y, 0, color);
            color = new float[] { 0.5f, 0.9f, 0.7f, 0.8f };
            geometry1.Vertex(_pos.x + sin(fireDeg) * 20, _pos.y + cos(fireDeg) * 20, 0, color);
            geometry1.LineStrip(first1, geometry1.vertexCount - first1);
            Gfx.Draw(geometry1.count, geometry1.Bindings(model, null, 1, true), new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = Gfx.Blend.Additive });
        }

        if ((cnt < 0) && ((-cnt % 32) < 16))
            return;
        model = Transform.Translate(model, pos.x, pos.y, 0);
        model = Transform.Rotate(model, -deg * 180 / PI, 0, 0, 1);
        _shape.draw(model);
        bridgeShape.draw(model);
        if (shieldCnt > 0)
        {
            float ss = 0.66f;
            if (shieldCnt < 120)
                ss = ss * ((float)shieldCnt / 120);
            model = Transform.Scale(model, ss, ss, ss);
            model = Transform.Rotate(model, shieldCnt * 5, 0, 0, 1);
            shieldShape.draw(model);
        }

    }

    public void drawFront(float[] model)
    {
        float[] color = null;
        if (cnt < -INVINCIBLE_CNT)
            return;
        if (gameMode == InGameStateGameMode.MOUSE)
        {
            color = new float[] { 0.7f, 0.9f, 0.8f, 1.0f };
            float width = 2;
            drawSight(model, mouseInput.x, mouseInput.y, 0.3f, color, width, "mouse-sight-inner");
            float ss = 0.9f - 0.8f * ((cnt + 1024) % 32) / 32;
            color = new float[] { 0.5f, 0.9f, 0.7f, 0.8f };
            drawSight(model, mouseInput.x, mouseInput.y, ss, color, width, "mouse-sight-outer");
        }
    }

    public void drawSight(float[] model, float x, float y, float size, float[] color, float width, string key)
    {
        var geometry1 = new Mesh(key + "-1");
        int first1 = geometry1.vertexCount;
        geometry1.Vertex(x - size, y - size * 0.5f, 0, color);
        geometry1.Vertex(x - size, y - size, 0, color);
        geometry1.Vertex(x - size * 0.5f, y - size, 0, color);
        geometry1.LineStrip(first1, geometry1.vertexCount - first1);
        Gfx.Draw(geometry1.count, geometry1.Bindings(model, null, width, true), new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = Gfx.Blend.Additive });
        var geometry2 = new Mesh(key + "-2");
        int first2 = geometry2.vertexCount;
        geometry2.Vertex(x + size, y - size * 0.5f, 0, color);
        geometry2.Vertex(x + size, y - size, 0, color);
        geometry2.Vertex(x + size * 0.5f, y - size, 0, color);
        geometry2.LineStrip(first2, geometry2.vertexCount - first2);
        Gfx.Draw(geometry2.count, geometry2.Bindings(model, null, width, true), new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = Gfx.Blend.Additive });
        var geometry3 = new Mesh(key + "-3");
        int first3 = geometry3.vertexCount;
        geometry3.Vertex(x + size, y + size * 0.5f, 0, color);
        geometry3.Vertex(x + size, y + size, 0, color);
        geometry3.Vertex(x + size * 0.5f, y + size, 0, color);
        geometry3.LineStrip(first3, geometry3.vertexCount - first3);
        Gfx.Draw(geometry3.count, geometry3.Bindings(model, null, width, true), new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = Gfx.Blend.Additive });
        var geometry4 = new Mesh(key + "-4");
        int first4 = geometry4.vertexCount;
        geometry4.Vertex(x - size, y + size * 0.5f, 0, color);
        geometry4.Vertex(x - size, y + size, 0, color);
        geometry4.Vertex(x - size * 0.5f, y + size, 0, color);
        geometry4.LineStrip(first4, geometry4.vertexCount - first4);
        Gfx.Draw(geometry4.count, geometry4.Bindings(model, null, width, true), new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = Gfx.Blend.Additive });
    }

    public void drawShape(float[] model)
    {
        _shape.draw(model);
        bridgeShape.draw(model);
    }

    public void clearBullets()
    {
        gameState.clearBullets();
    }

    public Vector pos
    {
        get
        {
            return _pos;
        }

        set
        {
            _pos = value;
        }
    }

    public Vector vel
    {
        get
        {
            return _vel;
        }

        set
        {
            _vel = value;
        }
    }

    public void setReplayMode(float turnSpeed, bool reverseFire)
    {
        _replayMode = true;
        this.turnSpeed = turnSpeed;
        this.reverseFire = reverseFire;
    }

    public void unsetReplayMode()
    {
        _replayMode = false;
        turnSpeed = GameManager.shipTurnSpeed;
        reverseFire = GameManager.shipReverseFire;
    }

    public bool replayMode
    {
        get
        {
            return _replayMode;
        }

        set
        {
            _replayMode = value;
        }
    }
}
