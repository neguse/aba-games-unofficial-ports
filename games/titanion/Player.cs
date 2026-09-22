// Copyright 2006 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Drawing;

public class Player : Token<PlayerState, PlayerSpec>
{
    public override void init_1(List<object> args)
    {
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

    public virtual bool replayMode(bool v)
    {
        state.replayMode = v;
        return state.replayMode;
    }

    public virtual void set_0()
    {
        state.set_0();
        spec.start_0();
        {
            hitOffset.y = 0;
            hitOffset.x = hitOffset.y;
        }

        spec.field.setEyePos(pos());
    }

    public virtual bool checkBulletHit(Vector p, Vector pp)
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

    public virtual bool checkEnemyHit_3(Vector p, Vector v, Vector size)
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

    public virtual void destroy()
    {
        remove();
        spec.destroyed_1(state);
    }

    public virtual void drawState_0()
    {
        if ((spec.gameState.mode_0() == GameStateMode.CLASSIC))
            spec.drawState_1(state);
    }

    public virtual void destroyCapturedEnemies(int idx)
    {
        state.destroyCapturedEnemies(idx);
    }

    public virtual bool isInTractorBeam(Vector p)
    {
        return spec.tractorBeam.contains_1(p);
    }

    public virtual int addCapturedEnemy(Enemy e)
    {
        return state.addCapturedEnemy(e);
    }

    public virtual float capturedEnemyWidth()
    {
        return state.capturedEnemyWidth;
    }

    public virtual void midEnemyProvacated()
    {
        state.midEnemyProvacated = true;
    }

    public virtual void addScore_1(int sc)
    {
        spec.addScore_1(sc);
    }

    public virtual void addMultiplier(float mp)
    {
        spec.addMultiplier(mp);
    }

    public virtual float multiplier()
    {
        return spec.multiplier();
    }

    public virtual float deg()
    {
        return state.deg;
    }

    public virtual bool isActive()
    {
        return state.isActive();
    }

    public virtual bool hasCollision()
    {
        return state.hasCollision();
    }

    public virtual bool enemiesHasCollision()
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

public class PlayerState : TokenState
{
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
        capturedEnemies = new Enemy[MAX_CAPTURED_ENEMIES_NUM];
        vel = new Vector();
    }

    public virtual void setSpec(PlayerSpec spec)
    {
        this.spec = spec;
    }

    public virtual void set_0()
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

    public override void clear()
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
        base.clear();
    }

    public virtual void reset()
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

    public virtual void move_0()
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

    public virtual bool isActive()
    {
        return !((isInRespawn));
    }

    public virtual bool hasCollision()
    {
        return (((!((isInRespawn)))) && ((!((isInvincible)))));
    }

    public virtual bool hasShape()
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

    public virtual void destroyed_0()
    {
        respawnCnt = RESPAWN_INTERVAL;
        destroyCapturedEnemies(0);
        isInRespawn = true;
    }

    public virtual int addCapturedEnemy(Enemy e)
    {
        if (((isInRespawn)) || ((capturedEnemyNum >= MAX_CAPTURED_ENEMIES_NUM)))
            return -1;
        capturedEnemies[capturedEnemyNum] = e;
        capturedEnemyNum++;
        return capturedEnemyNum - 1;
    }

    public virtual void destroyCapturedEnemies(int idx)
    {
        for (int i = idx; i < capturedEnemyNum; i++)
            if (capturedEnemies[i].exists)
                capturedEnemies[i].destroyed_0();
        capturedEnemyNum = idx;
    }

    public virtual void countShotHit()
    {
        captureBeamEnergy = captureBeamEnergy + (0.02f / (capturedEnemyNum + 1));
        if (captureBeamEnergy > 1)
            captureBeamEnergy = 1;
    }
}

public class PlayerSpec : TokenSpec<PlayerState>
{
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
    public GhostEnemySpec ghostEnemySpec;
    public EnemyShape ghostEnemyShape;
    public int shotMaxNum;
    public PlayerSpec(Pad pad, GameState gameState, Field field, EnemyPool enemies, BulletPool bullets, ParticlePool particles)
    {
        this.pad = (pad is RecordablePad ? (RecordablePad)pad : null);
        this.gameState = gameState;
        this.field = field;
        this.enemies = enemies;
        this.bullets = bullets;
        this.particles = particles;
        shots = new ShotPool(16);
        capturedEnemiesShots = new ShotPool(64);
        shotSpec = new ShotSpec(field, enemies, bullets, gameState);
        shape = new PlayerShape();
        lineShape = new PlayerLineShape();
        ghostEnemyShape = new Enemy1TrailShape();
        ghostEnemySpec = new GhostEnemySpec(field, ghostEnemyShape);
    }

    public virtual void setState(PlayerState ps)
    {
        playerState = ps;
        shotSpec.setPlayerState(ps);
        tractorBeam = new TractorBeam(field, ps, gameState);
    }

    public virtual void close()
    {
        ghostEnemyShape.close();
        ((shape is PlayerShape ? (PlayerShape)shape : null)).close();
        shotSpec.close();
    }

    public virtual void start_0()
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

    public virtual void respawn(PlayerState ps)
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

    public virtual void clear()
    {
        tractorBeam.clear();
        shots.clear();
        capturedEnemiesShots.clear();
    }

    public override bool move_1(PlayerState ps)
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

    public virtual void fireShot(PlayerState ps)
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

    public virtual void addShotParticle(Vector p, float d)
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

    public virtual void addVelocity(PlayerState ps, Vector v, Vector o)
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

    public virtual void destroyed_1(PlayerState ps)
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

    public virtual void addScore_1(int sc)
    {
        gameState.addScore_2(sc);
    }

    public virtual void addMultiplier(float mp)
    {
        gameState.addMultiplier(mp);
    }

    public virtual float multiplier()
    {
        return gameState.multiplier();
    }

    public override void draw_1(PlayerState ps)
    {
        {
            shots.draw_0();
            capturedEnemiesShots.draw_0();
            tractorBeam.draw_0();
            if ((!((ps.isActive()))))
                return;
            Vector3 p = field.calcCircularPos_1(ps.pos);
            float cd = field.calcCircularDeg(ps.pos.x);
            if (ps.hasShape())
                shape.draw_3(p, cd, ps.deg);
            int c = ps.colorCnt % 60;
            float a = 0;
            if (c < 30)
                a = (float)c / 30;
            else
                a = 1 - (float)(c - 30) / 30;
            TtnScreen.setColor(a, a, a);
            lineShape.draw_3(p, cd, ps.deg);
        }
    }

    public virtual void drawState_1(PlayerState ps)
    {
        {
            TtnScreen.setColor(1, 1, 1, 0.5f);
            glBegin(GL_TRIANGLE_FAN);
            glVertex3f(15, 400, 0);
            glVertex3f(15 + ps.captureBeamEnergy * 100, 400, 0);
            glVertex3f(25 + ps.captureBeamEnergy * 100, 420, 0);
            glVertex3f(25, 420, 0);
            glEnd();
            glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
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

            TtnScreen.setColor(1, 1, 1, a);
            glBegin(GL_LINE_LOOP);
            glVertex3f(15, 400, 0);
            glVertex3f(115, 400, 0);
            glVertex3f(125, 420, 0);
            glVertex3f(25, 420, 0);
            glEnd();
            glBlendFunc(GL_SRC_ALPHA, GL_ONE);
            if (ps.captureBeamEnergy >= 1)
                Letter.drawString("READY", 50, 390, 4);
        }
    }
}

public class ShotPool : ActorPool<Shot>
{
    public ShotPool(int n) : base(n, null, () => new Shot())
    {
    }

    public virtual void checkParent_0()
    {
        foreach (Shot a in actors)
            if (a.exists)
                if (!((a.spec.checkParent_1(a.state))))
                    a.remove();
    }

    public virtual int num()
    {
        int n = 0;
        foreach (Shot a in actors)
            if (a.exists)
                n++;
        return n;
    }
}

public class Shot : Token<ShotState, ShotSpec>
{
    public override void init_1(List<object> args)
    {
        state = new ShotState();
    }

    public virtual void setParent_1(Shot s)
    {
        spec.setParent_2(state, s);
    }
}

public class ShotState : TokenState
{
    public Shot parent;
    public int cnt;
    public override void clear()
    {
        parent = null;
        cnt = 0;
        base.clear();
    }
}

public class ShotSpec : TokenSpec<ShotState>
{
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
        shape = new ShotShape();
    }

    public virtual void setPlayerState(PlayerState ps)
    {
        playerState = ps;
    }

    public virtual void close()
    {
        ((shape is ShotShape ? (ShotShape)shape : null)).close();
    }

    public override void set_1(ShotState ss)
    {
        ss.parent = null;
        ss.cnt = 0;
    }

    public virtual void setParent_2(ShotState ss, Shot s)
    {
        ss.parent = s;
    }

    public override bool move_1(ShotState ss)
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

    public virtual bool checkParent_1(ShotState ss)
    {
        if ((ss.parent) != null)
            if (ss.parent.exists == false)
                return false;
        return true;
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
    public List<TractorBeamShape> shapes = new List<TractorBeamShape>();
    public float length = 0;
    public int cnt;
    public bool isExtending;
    public TractorBeam(Field field, PlayerState playerState, GameState gameState)
    {
        this.field = field;
        this.playerState = playerState;
        this.gameState = gameState;
        shapes.Add(new TractorBeamShapeRed());
        shapes.Add(new TractorBeamShapeBlue());
        shapes.Add(new TractorBeamShapePurple());
        shapes.Add(new TractorBeamShapeDarkRed());
        shapes.Add(new TractorBeamShapeDarkBlue());
        shapes.Add(new TractorBeamShapeDarkPurple());
        clear();
    }

    public virtual void clear()
    {
        length = 0;
        cnt = 0;
        isExtending = false;
    }

    public virtual void move_0()
    {
        if (length <= 0)
            return;
        cnt++;
        if (((cnt % 12 == 0)) && ((isExtending)))
            Sound.playSe("tractor.wav");
    }

    public virtual void extendLength(float ratio = 1)
    {
        length = length + ((MAX_LENGTH - length) * 0.05f * ratio);
        isExtending = true;
    }

    public virtual bool reduceLength(float ratio = 1)
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

    public virtual bool contains_1(Vector p)
    {
        if (length <= 0)
            return false;
        return (((((((p.x > playerState.pos.x - WIDTH / 2)) && ((p.x < playerState.pos.x + WIDTH / 2)))) && ((p.y > playerState.pos.y)))) && ((p.y < playerState.pos.y + length + WIDTH)));
    }

    public virtual void draw_0()
    {
        if (length <= 0)
            return;
        float y = SHAPE_INTERVAL_LENGTH - (cnt % SHAPE_INTERVAL_TIME) * SHAPE_INTERVAL_LENGTH / SHAPE_INTERVAL_TIME;
        int c = GameMath.integer((cnt / SHAPE_INTERVAL_TIME));
        for (;;)
        {
            if (y > length)
                break;
            glPushMatrix();
            Vector3 p = field.calcCircularPos_2(playerState.pos.x, playerState.pos.y + y);
            TtnScreen.glTranslate(p);
            float s = y;
            if (s > 1)
                s = 1;
            glScalef(s, s, s);
            switch (gameState.mode_0())
            {
                case GameStateMode.CLASSIC:
                case GameStateMode.BASIC:
                    shapes[c % 3].draw_0();
                    break;
                case GameStateMode.MODERN:
                    if (playerState.midEnemyProvacated)
                        shapes[c % 3].draw_0();
                    else
                        shapes[c % 3 + 3].draw_0();
                    break;
            }

            c++;
            glPopMatrix();
            y = y + (SHAPE_INTERVAL_LENGTH);
        }
    }
}
