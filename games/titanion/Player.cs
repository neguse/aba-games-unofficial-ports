// Copyright 2006 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using Lub;

public class Player
{
    public bool exists;
    public PlayerState state;
    public PlayerSpec spec;
    public void move_0()
    {
        if (!((spec.move_1(state))))
            remove();
    }

    public void remove()
    {
        exists = false;
    }

    public void draw_0(float[] model, float[] color, Gfx.Blend blend)
    {
        spec.draw_1(model, color, blend, state);
    }

    public Vector pos()
    {
        return state.pos;
    }

    public Vector hitOffset;
    public Player(PlayerSpec spec)
    {
        state = new PlayerState();
        this.spec = spec;
        spec.setState(state);
        state.setSpec(spec);
        hitOffset = new Vector();
    }

    public bool replayMode(bool v)
    {
        state.replayMode = v;
        return state.replayMode;
    }

    public void set_0()
    {
        state.set_0();
        spec.start_0();
        {
            hitOffset.y = 0;
            hitOffset.x = hitOffset.y;
        }

        spec.field.setEyePos(pos());
    }

    public bool checkBulletHit(Vector p, Vector pp)
    {
        {
            if ((!((state.hasCollision()))))
                return false;
            if (state.spec.field.checkHitDist_4(state.pos, p, pp, state.spec.bulletHitWidth))
            {
                destroy();
                return true;
            }

            return false;
        }
    }

    public bool checkEnemyHit_3(Vector p, Vector v, Vector size)
    {
        if ((spec.gameState.mode_0() == GameStateMode.MODERN))
            return false;
        {
            if ((!((state.hasCollision()))))
                return false;
            if (((fabs(state.pos.x - p.x) < size.x)) && ((fabs(state.pos.y - p.y) < size.y)))
            {
                switch (state.spec.gameState.mode_0())
                {
                    case GameStateMode.CLASSIC:
                        destroy();
                        break;
                    case GameStateMode.BASIC:
                        hitOffset.x = state.pos.x - p.x;
                        hitOffset.y = state.pos.y - p.y;
                        state.spec.addVelocity(state, v, hitOffset);
                        break;
                }

                return true;
            }

            return false;
        }
    }

    public void destroy()
    {
        remove();
        spec.destroyed_1(state);
    }

    public void drawState_0(float[] model, float[] color, Gfx.Blend blend)
    {
        if ((spec.gameState.mode_0() == GameStateMode.CLASSIC))
            spec.drawState_1(model, color, blend, state);
    }

    public void destroyCapturedEnemies(int idx)
    {
        state.destroyCapturedEnemies(idx);
    }

    public bool isInTractorBeam(Vector p)
    {
        return spec.tractorBeam.contains_1(p);
    }

    public int addCapturedEnemy(Enemy e)
    {
        return state.addCapturedEnemy(e);
    }

    public float capturedEnemyWidth()
    {
        return state.capturedEnemyWidth;
    }

    public void midEnemyProvacated()
    {
        state.midEnemyProvacated = true;
    }

    public void addScore_1(int sc)
    {
        spec.addScore_1(sc);
    }

    public void addMultiplier(float mp)
    {
        spec.addMultiplier(mp);
    }

    public float multiplier()
    {
        return spec.multiplier();
    }

    public float deg()
    {
        return state.deg;
    }

    public bool isActive()
    {
        return state.isActive();
    }

    public bool hasCollision()
    {
        return state.hasCollision();
    }

    public bool enemiesHasCollision()
    {
        switch (spec.gameState.mode_0())
        {
            case GameStateMode.CLASSIC:
                return state.hasCollision();
            case GameStateMode.BASIC:
                return true;
            case GameStateMode.MODERN:
                return false;
        }

        return false;
    }
}

public class PlayerState
{
    public string meshKey;
    public bool isInitialized = false;
    public Vector pos;
    public float deg;
    public float speed;
    public bool replayMode;
    public const int RESPAWN_INTERVAL = 72;
    public const int INVINCIBLE_INTERVAL_RESPAWN = 240;
    public const int MAX_CAPTURED_ENEMIES_NUM = 10;
    public PlayerSpec spec;
    public Enemy[] capturedEnemies;
    public int capturedEnemyNum;
    public int respawnCnt;
    public bool isInRespawn;
    public int invincibleCnt;
    public bool isInvincible;
    public int shotCnt;
    public int capturedEnemyShotCnt;
    public bool aPressed, bPressed;
    public Vector vel;
    public float capturedEnemyWidth;
    public int colorCnt;
    public bool isFirstShot;
    public float captureBeamEnergy;
    public bool captureBeamReleased;
    public int ghostCnt;
    public int ghostShotCnt;
    public bool midEnemyProvacated;
    public PlayerState()
    {
        meshKey = TokenState.nextMeshKey();
        pos = new Vector();
        capturedEnemies = new Enemy[MAX_CAPTURED_ENEMIES_NUM];
        vel = new Vector();
    }

    public void setSpec(PlayerSpec spec)
    {
        this.spec = spec;
    }

    public void set_0()
    {
        reset();
        pos.x = 0;
        respawnCnt = 0;
        isInRespawn = false;
        {
            bPressed = true;
            aPressed = bPressed;
        }

        shotCnt = 60;
    }

    public void clear()
    {
        capturedEnemyNum = 0;
        {
            invincibleCnt = 0;
            respawnCnt = invincibleCnt;
        }

        {
            isInvincible = false;
            isInRespawn = isInvincible;
        }

        shotCnt = 0;
        capturedEnemyShotCnt = 0;
        {
            vel.y = 0;
            vel.x = vel.y;
        }

        capturedEnemyWidth = 1.0f;
        colorCnt = 0;
        isFirstShot = false;
        captureBeamEnergy = 0;
        captureBeamReleased = false;
        ghostCnt = 0;
        ghostShotCnt = 0;
        midEnemyProvacated = false;
        {
            pos.y = 0;
            pos.x = pos.y;
        }

        deg = 0;
        speed = 0;
        isInitialized = true;
    }

    public void reset()
    {
        float x = pos.x;
        clear();
        pos.x = x;
        pos.y = -10.0f;
        speed = PlayerSpec.BASE_SPEED;
        invincibleCnt = INVINCIBLE_INTERVAL_RESPAWN;
        isInvincible = true;
        isFirstShot = true;
        captureBeamEnergy = 1;
        spec.respawn(this);
    }

    public void move_0()
    {
        colorCnt++;
        ghostCnt++;
        if (isInRespawn)
        {
            respawnCnt--;
            if (respawnCnt <= 0)
            {
                reset();
                isInRespawn = false;
            }
        }
        else if (isInvincible)
        {
            invincibleCnt--;
            if (invincibleCnt <= 0)
                isInvincible = false;
        }

        midEnemyProvacated = false;
    }

    public bool isActive()
    {
        return !((isInRespawn));
    }

    public bool hasCollision()
    {
        return (((!((isInRespawn)))) && ((!((isInvincible)))));
    }

    public bool hasShape()
    {
        if (isInRespawn)
            return false;
        if (!((isInvincible)))
            return true;
        if (invincibleCnt % 60 < 30)
            return false;
        else
            return true;
    }

    public void destroyed_0()
    {
        respawnCnt = RESPAWN_INTERVAL;
        destroyCapturedEnemies(0);
        isInRespawn = true;
    }

    public int addCapturedEnemy(Enemy e)
    {
        if (((isInRespawn)) || ((capturedEnemyNum >= MAX_CAPTURED_ENEMIES_NUM)))
            return -1;
        capturedEnemies[capturedEnemyNum] = e;
        capturedEnemyNum++;
        return capturedEnemyNum - 1;
    }

    public void destroyCapturedEnemies(int idx)
    {
        for (int i = idx; i < capturedEnemyNum; i++)
            if (capturedEnemies[i].exists)
                capturedEnemies[i].destroyed_0();
        capturedEnemyNum = idx;
    }

    public void countShotHit()
    {
        captureBeamEnergy = captureBeamEnergy + (0.02f / (capturedEnemyNum + 1));
        if (captureBeamEnergy > 1)
            captureBeamEnergy = 1;
    }
}

public class PlayerSpec
{
    public Field field;
    public Shape shape;
    public static TitanionRand rand = new TitanionRand();
    public static void setRandSeed(int seed)
    {
        rand.setSeed(seed);
    }

    public const float BASE_SPEED = 0.15f;
    public const float BASE_VELOCITY = 0.03f;
    public const float CAPTURED_ENEMIES_INTERVAL_LENGTH = 1.2f;
    public const float TILT_DEG = 1.0f;
    public const float SHOT_INTERVAL = 3;
    public const float FIRST_SHOT_INTERVAL = 6;
    public const int TWIN_SHOT_MAX_NUM = 2;
    public ShotPool shots, capturedEnemiesShots;
    public ShotSpec shotSpec;
    public EnemyPool enemies;
    public BulletPool bullets;
    public ParticlePool particles;
    public RecordablePad pad;
    public GameState gameState;
    public PlayerState playerState;
    public TractorBeam tractorBeam;
    public Shape lineShape;
    public float bulletHitWidth;
    public EnemySpec ghostEnemySpec;
    public EnemyShape ghostEnemyShape;
    public int shotMaxNum;
    public PlayerSpec(RecordablePad pad, GameState gameState, Field field, EnemyPool enemies, BulletPool bullets, ParticlePool particles)
    {
        this.pad = pad;
        this.gameState = gameState;
        this.field = field;
        this.enemies = enemies;
        this.bullets = bullets;
        this.particles = particles;
        shots = new ShotPool(16);
        capturedEnemiesShots = new ShotPool(64);
        shotSpec = new ShotSpec(field, enemies, bullets, gameState);
        shape = new MeshShape(new PlayerShape());
        lineShape = new MeshShape(new PlayerLineShape());
        ghostEnemyShape = new EnemyShape(new Enemy1TrailShape());
        ghostEnemySpec = new GhostEnemySpec(field, ghostEnemyShape).spec;
    }

    public void setState(PlayerState ps)
    {
        playerState = ps;
        shotSpec.setPlayerState(ps);
        tractorBeam = new TractorBeam(field, ps, gameState);
    }

    public void close()
    {
        ghostEnemyShape.close();
        ((shape is MeshShape ? (MeshShape)shape : null)).close();
        shotSpec.close();
    }

    public void start_0()
    {
        clear();
        switch (gameState.mode_0())
        {
            case GameStateMode.CLASSIC:
                bulletHitWidth = 0.4f;
                shotMaxNum = 3;
                break;
            case GameStateMode.BASIC:
                bulletHitWidth = 0.2f;
                shotMaxNum = 3;
                break;
            case GameStateMode.MODERN:
                bulletHitWidth = 0.1f;
                shotMaxNum = 16;
                break;
        }
    }

    public void respawn(PlayerState ps)
    {
        if ((gameState.mode_0() == GameStateMode.MODERN))
        {
            for (int i = 0; i < 4; i++)
            {
                Enemy e = enemies.getInstance();
                if ((!(((e) != null))))
                    break;
                e.set_5(ghostEnemySpec, ps.pos.x, ps.pos.y, 0, 0);
                playerState.addCapturedEnemy(e);
            }
        }
    }

    public void clear()
    {
        tractorBeam.clear();
        shots.clear();
        capturedEnemiesShots.clear();
    }

    public bool move_1(PlayerState ps)
    {
        {
            PadState input = null;
            if (!((ps.replayMode)))
            {
                input = pad.getState();
            }
            else
            {
                input = pad.replay();
                if (input == null)
                {
                    gameState.startGameOverWithoutRecording();
                    input = pad.getNullState();
                }
            }

            shots.move_0();
            capturedEnemiesShots.move_0();
            capturedEnemiesShots.checkParent_0();
            if (gameState.isGameOver())
            {
                if ((input.button & PadStateButton.A) != 0)
                {
                    if (!((ps.aPressed)))
                    {
                        ps.aPressed = true;
                        if (!((ps.replayMode)))
                            gameState.backToTitle();
                    }
                }
                else
                {
                    ps.aPressed = false;
                }

                return true;
            }

            ps.move_0();
            if (!((ps.isActive())))
                return true;
            float vx = 0, vy = 0;
            if ((input.dir & PadStateDir.RIGHT) != 0)
                vx = 1;
            else if ((input.dir & PadStateDir.LEFT) != 0)
                vx = -1;
            if ((input.dir & PadStateDir.UP) != 0)
                vy = 1;
            else if ((input.dir & PadStateDir.DOWN) != 0)
                vy = -1;
            if (((vx != 0)) && ((vy != 0)))
            {
                vx = vx * (0.7f);
                vy = vy * (0.7f);
            }

            float px = ps.pos.x;
            ps.pos.x = ps.pos.x + ((vx * ps.speed));
            if ((gameState.mode_0() == GameStateMode.CLASSIC))
                vy = vy * (0.5f);
            ps.pos.y = ps.pos.y + ((vy * ps.speed));
            if ((!((((input.button & PadStateButton.B)) != 0))))
                ps.deg = ps.deg + ((-TILT_DEG * (vx * ps.speed) - ps.deg) * 0.1f);
            ps.pos.opAddAssign(ps.vel);
            ps.vel.opMulAssign(0.9f);
            if ((gameState.mode_0() == GameStateMode.MODERN))
            {
                float d = ps.ghostCnt * 0.05f;
                for (int i = 0; i < ps.capturedEnemyNum; i++)
                {
                    Enemy e = ps.capturedEnemies[i];
                    e.setGhostEnemyState(ps.pos.x + sin(d) * ps.capturedEnemyWidth * 2, ps.pos.y, ps.deg, GameMath.integer((d * 180 / PI / 3)));
                    d = d + (PI / 2);
                }
            }

            switch (gameState.mode_0())
            {
                case GameStateMode.CLASSIC:
                    if ((((((input.button & PadStateButton.A)) != 0)) && ((!((ps.captureBeamReleased))))))
                    {
                        if (ps.shotCnt <= 0)
                            fireShot(ps);
                    }
                    else
                    {
                        ps.isFirstShot = true;
                    }

                    break;
                case GameStateMode.BASIC:
                    if ((((((input.button & PadStateButton.A)) != 0)) && (((!((((input.button & PadStateButton.B)) != 0)))))))
                    {
                        if (ps.shotCnt <= 0)
                            fireShot(ps);
                    }
                    else
                    {
                        ps.isFirstShot = true;
                    }

                    break;
                case GameStateMode.MODERN:
                    if ((input.button & PadStateButton.A) != 0)
                    {
                        if (ps.shotCnt <= 0)
                            fireShot(ps);
                    }
                    else
                    {
                        ps.isFirstShot = true;
                    }

                    break;
            }

            if ((input.button & PadStateButton.B) != 0)
            {
                ps.speed = ps.speed + ((BASE_SPEED * 1.2f - ps.speed) * 0.33f);
                ps.deg = ps.deg * (0.9f);
                if ((gameState.mode_0() == GameStateMode.MODERN))
                {
                    ps.capturedEnemyWidth = ps.capturedEnemyWidth - (0.05f);
                    if (ps.capturedEnemyWidth < 0.2f)
                        ps.capturedEnemyWidth = 0.2f;
                }
            }
            else
            {
                ps.speed = ps.speed + ((BASE_SPEED * 2.0f - ps.speed) * 0.33f);
                if ((gameState.mode_0() == GameStateMode.MODERN))
                {
                    ps.capturedEnemyWidth = ps.capturedEnemyWidth + (0.05f);
                    if (ps.capturedEnemyWidth > 1)
                        ps.capturedEnemyWidth = 1;
                }
            }

            switch (gameState.mode_0())
            {
                case GameStateMode.CLASSIC:
                    if (((((((((((input.button & PadStateButton.B) != 0)) && ((!((ps.captureBeamReleased))))))) && ((ps.captureBeamEnergy >= 1))))) && ((ps.capturedEnemyNum < PlayerState.MAX_CAPTURED_ENEMIES_NUM))))
                    {
                        ps.captureBeamReleased = true;
                        ps.isInvincible = true;
                        ps.invincibleCnt = 99999;
                    }

                    if (ps.captureBeamReleased)
                    {
                        if (((ps.captureBeamEnergy <= 0)) || ((ps.capturedEnemyNum >= PlayerState.MAX_CAPTURED_ENEMIES_NUM)))
                        {
                            ps.captureBeamEnergy = 0;
                            if (tractorBeam.reduceLength(0.5f))
                            {
                                ps.captureBeamReleased = false;
                                ps.invincibleCnt = 120;
                            }
                        }
                        else
                        {
                            tractorBeam.extendLength(0.5f);
                            ps.captureBeamEnergy = ps.captureBeamEnergy - (0.005f);
                        }
                    }

                    break;
                case GameStateMode.BASIC:
                    if (((((input.button & PadStateButton.B) != 0)) && ((ps.capturedEnemyNum < PlayerState.MAX_CAPTURED_ENEMIES_NUM))))
                        tractorBeam.extendLength();
                    else
                        tractorBeam.reduceLength();
                    break;
                case GameStateMode.MODERN:
                    if ((((((input.button & PadStateButton.B)) != 0)) && (((!((((input.button & PadStateButton.A)) != 0)))))))
                        tractorBeam.extendLength();
                    else
                        tractorBeam.reduceLength();
                    break;
            }

            tractorBeam.move_0();
            if (ps.shotCnt > 0)
                ps.shotCnt--;
            if (ps.capturedEnemyShotCnt > 0)
                ps.capturedEnemyShotCnt--;
            switch (gameState.mode_0())
            {
                case GameStateMode.CLASSIC:
                case GameStateMode.BASIC:
                    if (ps.pos.y > 0)
                        ps.pos.y = 0;
                    break;
                case GameStateMode.MODERN:
                    if (ps.pos.y > field.size().y)
                        ps.pos.y = field.size().y;
                    break;
            }

            if (ps.pos.y < -field.size().y)
                ps.pos.y = -field.size().y;
            if (ps.pos.x > field.size().x)
                ps.pos.x = field.size().x;
            else if (ps.pos.x < -field.size().x)
                ps.pos.x = -field.size().x;
            ps.pos.x = field.normalizeX(ps.pos.x);
            field.setEyePos(ps.pos);
            return true;
        }
    }

    public void fireShot(PlayerState ps)
    {
        {
            if ((shots.num() >= shotMaxNum))
                return;
            Shot s = shots.getInstance();
            if ((s) != null)
            {
                s.setAt(shotSpec, ps.pos, ps.deg, 0.66f);
                if (ps.isFirstShot)
                {
                    ps.isFirstShot = false;
                    ps.shotCnt = GameMath.integer(ps.shotCnt + (FIRST_SHOT_INTERVAL));
                }
                else
                {
                    ps.shotCnt = GameMath.integer(ps.shotCnt + (SHOT_INTERVAL));
                }

                gameState.countShotFired();
                addShotParticle(ps.pos, ps.deg);
                Sound.playSe("shot.wav");
                for (int i = 0; i < ps.capturedEnemyNum; i++)
                {
                    if (((((gameState.mode_0() == GameStateMode.MODERN))) && ((((i + ps.ghostShotCnt) % 4 == 0)))))
                        continue;
                    if (ps.capturedEnemies[i].isCaptured_0())
                    {
                        Shot ces = capturedEnemiesShots.getInstance();
                        if ((!(((ces) != null))))
                            break;
                        float d = ps.deg;
                        if ((gameState.mode_0() == GameStateMode.MODERN))
                            d = d - ((ps.capturedEnemies[i].pos().x - ps.pos.x) * 0.3f);
                        ces.setAt(shotSpec, ps.capturedEnemies[i].pos(), d, 0.66f);
                        if ((gameState.mode_0() != GameStateMode.MODERN))
                            ces.setParent_1(s);
                        else
                            gameState.countShotFired();
                        addShotParticle(ps.capturedEnemies[i].pos(), ps.deg);
                    }
                }

                if ((gameState.mode_0() == GameStateMode.MODERN))
                    ps.ghostShotCnt++;
            }
        }
    }

    public void addShotParticle(Vector p, float d)
    {
        for (int i = 0; i < 5; i++)
        {
            Particle pt = null;
            pt = particles.getInstanceForced();
            pt.set_13(ParticleShape.LINE, p.x - 0.5f, p.y, -d + rand.nextSignedFloat(0.5f), 0.25f + rand.nextFloat(0.75f), 1, 1.0f, 0.25f, 0.5f, 10);
            pt = particles.getInstanceForced();
            pt.set_13(ParticleShape.LINE, p.x + 0.5f, p.y, -d + rand.nextSignedFloat(0.5f), 0.25f + rand.nextFloat(0.75f), 1, 1.0f, 0.25f, 0.5f, 10);
        }
    }

    public void addVelocity(PlayerState ps, Vector v, Vector o)
    {
        Vector rv = v.getElement(o, 0.05f, 0.25f);
        rv.opMulAssign(5);
        ps.vel.opAddAssign(rv);
        float d = atan2(rv.x, -rv.y);
        float sp = rv.vctSize();
        for (int i = 0; i < 36; i++)
        {
            Particle pt = null;
            pt = particles.getInstanceForced();
            float r = 0, g = 0, b = 0;
            r = 0.5f + rand.nextFloat(0.5f);
            g = 0.3f + rand.nextFloat(0.3f);
            b = 0.8f + rand.nextFloat(0.2f);
            pt.set_13(ParticleShape.LINE, ps.pos.x, ps.pos.y, d + rand.nextSignedFloat(0.3f), sp * (1 + rand.nextFloat(2)), 1, r, g, b, 30 + rand.nextInt(30));
        }

        Sound.playSe("flick.wav");
    }

    public void destroyed_1(PlayerState ps)
    {
        {
            if ((!((ps.isActive()))))
                return;
            ps.destroyed_0();
            tractorBeam.clear();
            gameState.destroyedPlayer();
            float r = 0, g = 0, b = 0;
            r = 0.5f + rand.nextFloat(0.5f);
            g = 0.3f + rand.nextFloat(0.3f);
            b = 0.8f + rand.nextFloat(0.2f);
            for (int i = 0; i < 100; i++)
            {
                Particle p = particles.getInstanceForced();
                p.set_13(ParticleShape.QUAD, ps.pos.x, ps.pos.y, rand.nextFloat(PI * 2), 0.01f + rand.nextFloat(1.0f), 1 + rand.nextFloat(4), r, g, b, 10 + rand.nextInt(200));
            }

            r = 0.5f + rand.nextFloat(0.5f);
            g = 0.3f + rand.nextFloat(0.3f);
            b = 0.8f + rand.nextFloat(0.2f);
            for (int i = 0; i < 30; i++)
            {
                Particle p = particles.getInstanceForced();
                p.set_13(ParticleShape.TRIANGLE, ps.pos.x, ps.pos.y, rand.nextFloat(PI * 2), 0.03f + rand.nextFloat(0.3f), 3, r, g, b, 50 + rand.nextInt(150));
            }

            r = 0.5f + rand.nextFloat(0.5f);
            g = 0.3f + rand.nextFloat(0.3f);
            b = 0.8f + rand.nextFloat(0.2f);
            for (int i = 0; i < 300; i++)
            {
                Particle p = particles.getInstanceForced();
                p.set_13(ParticleShape.LINE, ps.pos.x, ps.pos.y, rand.nextFloat(PI * 2), 0.07f + rand.nextFloat(0.7f), 1, r, g, b, 100 + rand.nextInt(100));
            }

            Sound.playSe("player_explosion.wav");
        }
    }

    public void addScore_1(int sc)
    {
        gameState.addScore_2(sc);
    }

    public void addMultiplier(float mp)
    {
        gameState.addMultiplier(mp);
    }

    public float multiplier()
    {
        return gameState.multiplier();
    }

    public void draw_1(float[] model, float[] color, Gfx.Blend blend, PlayerState ps)
    {
        {
            shots.draw_0(model, color, blend);
            capturedEnemiesShots.draw_0(model, color, blend);
            tractorBeam.draw_0(model, color, blend);
            if ((!((ps.isActive()))))
                return;
            Vector3 p = field.calcCircularPos_1(ps.pos);
            float cd = field.calcCircularDeg(ps.pos.x);
            if (ps.hasShape())
                shape.draw_3(model, color, blend, p, cd, ps.deg);
            int c = ps.colorCnt % 60;
            float a = 0;
            if (c < 30)
                a = (float)c / 30;
            else
                a = 1 - (float)(c - 30) / 30;
            color = new float[] { a, a, a, 1 };
            lineShape.draw_3(model, color, blend, p, cd, ps.deg);
        }
    }

    public void drawState_1(float[] model, float[] color, Gfx.Blend blend, PlayerState ps)
    {
        {
            color = new float[] { 1, 1, 1, 0.5f };
            var part1 = new Mesh("Player-drawState_1-1");
            part1.Vertex(15, 400, 0, color);
            part1.Vertex(15 + ps.captureBeamEnergy * 100, 400, 0, color);
            part1.Vertex(25 + ps.captureBeamEnergy * 100, 420, 0, color);
            part1.Vertex(25, 420, 0, color);
            part1.Fan(0, part1.vertexCount);
            Gfx.Draw(part1.count, part1.Bindings(model, null, 1, blend == Gfx.Blend.Additive), new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
            blend = Gfx.Blend.Alpha;
            float a = 0;
            if (ps.captureBeamEnergy < 1)
            {
                a = ps.captureBeamEnergy;
            }
            else
            {
                int c = ps.colorCnt % 60;
                if (c < 30)
                    a = (float)c / 30;
                else
                    a = 1 - (float)(c - 30) / 30;
            }

            color = new float[] { 1, 1, 1, a };
            var part2 = new Mesh("Player-drawState_1-2");
            part2.Vertex(15, 400, 0, color);
            part2.Vertex(115, 400, 0, color);
            part2.Vertex(125, 420, 0, color);
            part2.Vertex(25, 420, 0, color);
            part2.LineStrip(0, part2.vertexCount, true);
            Gfx.Draw(part2.count, part2.Bindings(model, null, 1, blend == Gfx.Blend.Additive), new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
            blend = Gfx.Blend.Additive;
            if (ps.captureBeamEnergy >= 1)
                Letter.drawString(model, color, blend, "READY", 50, 390, 4);
        }
    }
}

public class ShotPool
{
    public ActorPool<Shot> pool;
    public Shot[] actors;
    public ShotPool(int n)
    {
        pool = new ActorPool<Shot>(n, null, () => new Shot());
        actors = pool.actors;
    }

    public Shot getInstance()
    {
        return pool.getInstance();
    }

    public void move_0()
    {
        pool.move_0();
    }

    public void draw_0(float[] model, float[] color, Gfx.Blend blend)
    {
        pool.draw_0(model, color, blend);
    }

    public void clear()
    {
        pool.clear();
    }

    public void checkParent_0()
    {
        foreach (Shot a in actors)
            if (a.exists)
                if (!((a.spec.checkParent_1(a.state))))
                    a.remove();
    }

    public int num()
    {
        int n = 0;
        foreach (Shot a in actors)
            if (a.exists)
                n++;
        return n;
    }
}

public class Shot : Actor
{
    public bool exists;
    public ShotState state;
    public ShotSpec spec;
    public bool getExists()
    {
        return exists;
    }

    public void setExists(bool v)
    {
        exists = v;
    }

    public void init_1(List<object> args)
    {
        state = new ShotState();
    }

    public void setAt(ShotSpec spec, Vector pos, float deg, float speed)
    {
        set_5(spec, pos.x, pos.y, deg, speed);
    }

    public void set_5(ShotSpec spec, float x, float y, float deg, float speed)
    {
        this.spec = spec;
        set_4(x, y, deg, speed);
    }

    public void set_4(float x, float y, float deg, float speed)
    {
        state.clear();
        state.pos.x = x;
        state.pos.y = y;
        state.deg = deg;
        state.speed = speed;
        spec.set_1(state);
        exists = true;
    }

    public void move_0()
    {
        if (!((spec.move_1(state))))
            remove();
    }

    public void remove()
    {
        exists = false;
    }

    public void draw_0(float[] model, float[] color, Gfx.Blend blend)
    {
        spec.draw_1(model, color, blend, state);
    }

    public Vector pos()
    {
        return state.pos;
    }

    public void setParent_1(Shot s)
    {
        spec.setParent_2(state, s);
    }
}

public class ShotState
{
    public string meshKey;
    public bool isInitialized = false;
    public Vector pos;
    public float deg;
    public float speed;
    public Shot parent;
    public int cnt;
    public ShotState()
    {
        meshKey = TokenState.nextMeshKey();
        pos = new Vector();
    }

    public void stepForward()
    {
        TokenState.stepForward(pos, deg, speed);
    }

    public void clear()
    {
        parent = null;
        cnt = 0;
        {
            pos.y = 0;
            pos.x = pos.y;
        }

        deg = 0;
        speed = 0;
        isInitialized = true;
    }
}

public class ShotSpec
{
    public Field field;
    public Shape shape;
    public EnemyPool enemies;
    public BulletPool bullets;
    public PlayerState playerState;
    public GameState gameState;
    public ShotSpec(Field field, EnemyPool enemies, BulletPool bullets, GameState gameState)
    {
        this.field = field;
        this.enemies = enemies;
        this.bullets = bullets;
        this.gameState = gameState;
        shape = new MeshShape(new ShotShape());
    }

    public void setPlayerState(PlayerState ps)
    {
        playerState = ps;
    }

    public void close()
    {
        ((shape is MeshShape ? (MeshShape)shape : null)).close();
    }

    public void set_1(ShotState ss)
    {
        ss.parent = null;
        ss.cnt = 0;
    }

    public void setParent_2(ShotState ss, Shot s)
    {
        ss.parent = s;
    }

    public bool move_1(ShotState ss)
    {
        {
            if ((ss.parent) != null)
                if (ss.parent.exists == false)
                    return false;
            ss.stepForward();
            ss.pos.x = field.normalizeX(ss.pos.x);
            if (!((field.containsOuterY(ss.pos.y))))
            {
                return false;
            }

            if (enemies.checkShotHit(ss.pos, ss.deg, 2))
            {
                if ((ss.parent) != null)
                    ss.parent.remove();
                gameState.countShotHit();
                playerState.countShotHit();
                return false;
            }

            ss.cnt++;
            return true;
        }
    }

    public bool checkParent_1(ShotState ss)
    {
        if ((ss.parent) != null)
            if (ss.parent.exists == false)
                return false;
        return true;
    }

    public void draw_1(float[] model, float[] color, Gfx.Blend blend, ShotState state)
    {
        {
            Vector3 p = field.calcCircularPos_1(state.pos);
            float cd = field.calcCircularDeg(state.pos.x);
            shape.draw_3(model, color, blend, p, cd, state.deg);
        }
    }
}


public class TractorBeam
{
    public const float MAX_LENGTH = 10;
    public const float WIDTH = 3.0f;
    public const float SHAPE_INTERVAL_TIME = 10;
    public const float SHAPE_INTERVAL_LENGTH = 0.5f;
    public Field field;
    public PlayerState playerState;
    public GameState gameState;
    public List<MeshShape> shapes = new List<MeshShape>();
    public float length = 0;
    public int cnt;
    public bool isExtending;
    public TractorBeam(Field field, PlayerState playerState, GameState gameState)
    {
        this.field = field;
        this.playerState = playerState;
        this.gameState = gameState;
        shapes.Add(new MeshShape(new TractorBeamShapeRed()));
        shapes.Add(new MeshShape(new TractorBeamShapeBlue()));
        shapes.Add(new MeshShape(new TractorBeamShapePurple()));
        shapes.Add(new MeshShape(new TractorBeamShapeDarkRed()));
        shapes.Add(new MeshShape(new TractorBeamShapeDarkBlue()));
        shapes.Add(new MeshShape(new TractorBeamShapeDarkPurple()));
        clear();
    }

    public void clear()
    {
        length = 0;
        cnt = 0;
        isExtending = false;
    }

    public void move_0()
    {
        if (length <= 0)
            return;
        cnt++;
        if (((cnt % 12 == 0)) && ((isExtending)))
            Sound.playSe("tractor.wav");
    }

    public void extendLength(float ratio = 1)
    {
        length = length + ((MAX_LENGTH - length) * 0.05f * ratio);
        isExtending = true;
    }

    public bool reduceLength(float ratio = 1)
    {
        length = length + ((0 - length) * 0.1f * ratio);
        if (length < 0.33f)
        {
            length = 0;
            return true;
        }

        isExtending = false;
        return false;
    }

    public bool contains_1(Vector p)
    {
        if (length <= 0)
            return false;
        return (((((((p.x > playerState.pos.x - WIDTH / 2)) && ((p.x < playerState.pos.x + WIDTH / 2)))) && ((p.y > playerState.pos.y)))) && ((p.y < playerState.pos.y + length + WIDTH)));
    }

    public void draw_0(float[] model, float[] color, Gfx.Blend blend)
    {
        if (length <= 0)
            return;
        float y = SHAPE_INTERVAL_LENGTH - (cnt % SHAPE_INTERVAL_TIME) * SHAPE_INTERVAL_LENGTH / SHAPE_INTERVAL_TIME;
        int c = GameMath.integer((cnt / SHAPE_INTERVAL_TIME));
        for (;;)
        {
            if (y > length)
                break;
            float[] parent1 = model;
            Vector3 p = field.calcCircularPos_2(playerState.pos.x, playerState.pos.y + y);
            model = Transform.Translate(model, p.x, p.y, p.z);
            float s = y;
            if (s > 1)
                s = 1;
            model = Transform.Scale(model, s, s, s);
            switch (gameState.mode_0())
            {
                case GameStateMode.CLASSIC:
                case GameStateMode.BASIC:
                    shapes[c % 3].draw_0(model, color, blend);
                    break;
                case GameStateMode.MODERN:
                    if (playerState.midEnemyProvacated)
                        shapes[c % 3].draw_0(model, color, blend);
                    else
                        shapes[c % 3 + 3].draw_0(model, color, blend);
                    break;
            }

            c++;
            model = parent1;
            y = y + (SHAPE_INTERVAL_LENGTH);
        }
    }
}
