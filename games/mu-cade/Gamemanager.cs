// Copyright 2006 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Lub;

public class GameManager
{
    public const string LAST_REPLAY_FILE_NAME = "last.rpl";
    public const int RANK_DOWN_INTERVAL = 60 * 1000;
    public const int BGM_CHANGE_INTERVAL = 2 * 60 * 1000;
    public RecordableTwinStickPad pad;
    public World world;
    public Field field;
    public EnemyPool enemies;
    public BulletPool bullets;
    public Ship ship;
    public ParticlePool particles;
    public ConnectedParticlePool connectedParticles;
    public TailParticlePool tailParticles;
    public StarParticlePool starParticles;
    public NumIndicatorPool numIndicators;
    public StageManager stageManager;
    public TitleManager titleManager;
    public ReplayData _replayData;
    public Rand rand;
    public int score;
    public int nextExtendScore;
    public int time;
    public int rankDownTime;
    public int bgmChangeTime;
    public int bgmStartCnt;
    public int left;
    public int state;
    public bool escPressed, pPressed, aPressed;
    public bool paused;
    public int pauseCnt;
    public bool _isGameOver;
    public int gameOverCnt;
    public PrefManager prefManager;
    public virtual void init_0()
    {
        BarrageManager.load();
        SoundManager.loadSounds();
        prefManager = new PrefManager();
        prefManager.load();
        OdeActor.initFirst();
        Letter.init_0();
        Ship.init_0();
        Enemy.init_0();
        SimpleBullet.init_0();
        Particle.init_0();
        ConnectedParticle.init_0();
        TailParticle.init_0();
        Field.init_0();
        pad = new RecordableTwinStickPad();
        world = new World();
        field = new Field(world, this);
        object[] pargs = default(object[]);
        pargs = McdArrays.Append(pargs, field);
        particles = new ParticlePool(256, pargs);
        connectedParticles = new ConnectedParticlePool(540, pargs);
        starParticles = new StarParticlePool(128, pargs);
        numIndicators = new NumIndicatorPool(16, null);
        field.setStarParticles(starParticles);
        ship = new Ship(world, pad, field, particles, connectedParticles, this);
        field.setShip(ship);
        object[] tpargs = default(object[]);
        tpargs = McdArrays.Append(tpargs, field);
        tpargs = McdArrays.Append(tpargs, ship);
        tailParticles = new TailParticlePool(32, tpargs);
        object[] bargs = default(object[]);
        bargs = McdArrays.Append(bargs, field);
        bargs = McdArrays.Append(bargs, ship);
        bargs = McdArrays.Append(bargs, this);
        bargs = McdArrays.Append(bargs, particles);
        bargs = McdArrays.Append(bargs, world);
        bullets = new BulletPool(256, 256, bargs);
        ship.setBullets(bullets);
        object[] eargs = default(object[]);
        eargs = McdArrays.Append(eargs, field);
        eargs = McdArrays.Append(eargs, particles);
        eargs = McdArrays.Append(eargs, connectedParticles);
        eargs = McdArrays.Append(eargs, tailParticles);
        eargs = McdArrays.Append(eargs, numIndicators);
        eargs = McdArrays.Append(eargs, ship);
        eargs = McdArrays.Append(eargs, this);
        enemies = new EnemyPool(64, eargs);
        enemies.init_1_World(world);
        stageManager = new StageManager(field, ship, bullets, world, enemies);
        titleManager = new TitleManager(field, stageManager, prefManager);
        rand = new Rand();
    }

    public virtual void start_0()
    {
        time = 0;
        score = time;
        startTitle();
    }

    public virtual void startInGame()
    {
        state = GameManagerGameState.IN_GAME;
        ship.replayMode_1(false);
        _replayData = new ReplayData();
        RecordableTwinStickPad rtsp = (RecordableTwinStickPad)pad;
        rtsp.startRecord();
        _replayData.twinStickPadInputRecord = rtsp.inputRecord;
        _replayData.seed = rand.nextInt32();
        initGame();
        score = 0;
        time = 0;
        SoundManager.playBgm();
    }

    public virtual void startTitle()
    {
        SoundManager.haltBgm();
        titleManager.start_0();
        restartTitle();
    }

    public virtual void restartTitle()
    {
        state = GameManagerGameState.TITLE;
        _replayData = new ReplayData();
        _replayData.seed = rand.nextInt32();
        initGame();
        aPressed = true;
    }

    public virtual void initGame()
    {
        clearAll();
        world.init_0();
        ship.start_0();
        field.start_0();
        stageManager.start_1(_replayData.seed);
        SoundManager.clearMarkedSes();
        _isGameOver = false;
        paused = false;
        rankDownTime = RANK_DOWN_INTERVAL;
        bgmChangeTime = BGM_CHANGE_INTERVAL;
        bgmStartCnt = -1;
        nextExtendScore = 50000;
        left = 0;
    }

    public virtual void clearAll()
    {
        enemies.clear();
        bullets.clear();
        ship.clear();
        particles.clear();
        connectedParticles.clear();
        tailParticles.clear();
        starParticles.clear();
        numIndicators.clear();
        field.clear();
        world.close();
    }

    public virtual void close()
    {
        world.close();
        SoundManager.haltBgm();
        Letter.close();
        BarrageManager.unload();
    }

    public virtual void move_0()
    {
        handleEscKey();
        if (state == GameManagerGameState.IN_GAME && !(_isGameOver))
        {
            handlePauseKey();
            if (paused)
            {
                pauseCnt++;
                return;
            }
        }

        field.move_0();
        stageManager.move_0();
        world.resetJointFeedback();
        enemies.clearContactJoint();
        ship.clearContactJoint();
        enemies.move_0();
        bullets.move_0();
        switch (state)
        {
            case GameManagerGameState.IN_GAME:
            case GameManagerGameState.REPLAY:
                ship.move_0();
                break;
            case GameManagerGameState.TITLE:
                ship.moveInTitle();
                break;
        }

        particles.move_0();
        connectedParticles.move_0();
        connectedParticles.recordLinePoints_0();
        tailParticles.move_0();
        starParticles.move_0();
        numIndicators.move_0();
        world.move_1(0.05f);
        enemies.checkFeedbackForce();
        ship.checkFeedbackForce();
        world.removeAllContactJoints();
        if (state != GameManagerGameState.TITLE)
        {
            if (!(_isGameOver))
            {
                handleRank();
                handleSound();
                time += 16;
            }
            else
            {
                gameOverCnt++;
                if (gameOverCnt < 60)
                    handleSound();
                if (gameOverCnt > 1000)
                    startTitle();
            }
        }

        if (state != GameManagerGameState.IN_GAME)
        {
            titleManager.move_0();
            checkGameStart();
        }
    }

    public virtual void handleEscKey()
    {
        if (pad.escape)
        {
            if (!(escPressed))
            {
                escPressed = true;
                if (state == GameManagerGameState.IN_GAME)
                    startTitle();
                else if (Lub.Host.Available())
                    Lub.Host.Send("quit", "");
            }
        }
        else
        {
            escPressed = false;
        }
    }

    public virtual void handlePauseKey()
    {
        if (pad.pause)
        {
            if (!(pPressed))
            {
                pPressed = true;
                paused = !(paused);
                pauseCnt = 0;
            }
        }
        else
        {
            pPressed = false;
        }
    }

    public virtual void checkGameStart()
    {
        TwinStickPadState input = default(TwinStickPadState);
        input = pad.getState_1(false);
        if ((input.button & TwinStickPadStateButton.A) != 0)
        {
            if (!(aPressed))
                startInGame();
            aPressed = true;
        }
        else
        {
            aPressed = false;
        }
    }

    public virtual void handleRank()
    {
        if (time >= rankDownTime)
        {
            stageManager.downRank();
            rankDownTime += RANK_DOWN_INTERVAL;
        }
    }

    public virtual void handleSound()
    {
        if (state != GameManagerGameState.IN_GAME)
            return;
        SoundManager.playMarkedSes();
        if (time >= bgmChangeTime)
        {
            SoundManager.fadeBgm();
            bgmChangeTime += BGM_CHANGE_INTERVAL;
            bgmStartCnt = 180;
        }

        if (bgmStartCnt > 0)
        {
            bgmStartCnt--;
            if (bgmStartCnt == 0)
                SoundManager.nextBgm();
        }
    }

    public virtual void addScore_1(int sc)
    {
        if (state == GameManagerGameState.TITLE || _isGameOver)
            return;
        score += sc;
        if (score >= nextExtendScore)
        {
            if (left < 2)
            {
                left++;
                SoundManager.playSe("extend.wav");
            }

            if (nextExtendScore < 200000)
                nextExtendScore = 0;
            nextExtendScore += 200000;
        }
    }

    public virtual void shipDestroyed()
    {
        left--;
        if (left < 0)
            startGameOver();
    }

    public virtual void startGameOver()
    {
        if (_isGameOver || state == GameManagerGameState.TITLE)
            return;
        _isGameOver = true;
        gameOverCnt = 0;
        SoundManager.fadeBgm();
        if (state == GameManagerGameState.REPLAY)
            return;
        prefManager.prefData.recordResult(score, time);
        prefManager.save();
    }

    public virtual void backToTitle()
    {
        if (gameOverCnt > 60)
            startTitle();
    }

    public virtual void draw()
    {
        float[] model = state == GameManagerGameState.IN_GAME || state == GameManagerGameState.REPLAY ? field.setLookAt() : field.setLookAtTitle();
        float[] tint = new float[] { 1, 1, 1, 1 }; Gfx.Blend blend = Gfx.Blend.Additive; string key = "scene";
        var stars = new Mesh("stars");
        starParticles.draw(model, tint, blend, "stars", stars);
        for (int i = 0; i + 1 < stars.vertexCount; i += 2) stars.Line(i, i + 1);
        if (stars.count > 0) Gfx.Draw(stars.count, stars.Bindings(model, tint, 1, blend == Gfx.Blend.Additive),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
        if (state == GameManagerGameState.IN_GAME || state == GameManagerGameState.REPLAY)
            field.draw(model, tint, blend, key + "-draw-1");
        enemies.drawSpectrum(model, tint, blend, key + "-drawSpectrum-1");
        if (state == GameManagerGameState.IN_GAME || state == GameManagerGameState.REPLAY)
        {
            enemies.drawShadow_0(model, tint, blend, key + "-drawShadow_0-1");
            enemies.draw(model, tint, blend, key + "-draw-2");
        }

        particles.draw(model, tint, blend, key + "-draw-3");
        connectedParticles.draw(model, tint, blend, key + "-draw-4");
        if (state == GameManagerGameState.IN_GAME || state == GameManagerGameState.REPLAY)
            tailParticles.draw(model, tint, blend, key + "-draw-5");
        bullets.drawSpectrum(model, tint, blend, key + "-drawSpectrum-2");
        if (state == GameManagerGameState.IN_GAME || state == GameManagerGameState.REPLAY)
        {
            bullets.drawShadow_0(model, tint, blend, key + "-drawShadow_0-2");
            bullets.draw(model, tint, blend, key + "-draw-6");
            ship.draw(model, tint, blend, key + "-draw-7");
            numIndicators.draw(model, tint, blend, key + "-draw-8");
        }

        field.drawOverlay(model, tint, blend, key + "-drawOverlay-1");
    }

    public virtual void drawState(float[] model, float[] tint, Gfx.Blend blend, string key)
    {
        Letter.drawNum(model, tint, blend, key + "-drawNum-1", score, 120, 21, 6);
        Letter.drawTime(model, tint, blend, key + "-drawTime-1", time, 610, 30, 6);
        switch (state)
        {
            case GameManagerGameState.IN_GAME:
            case GameManagerGameState.REPLAY:
                if (left > 0)
                {
                    float x = 320 - (left - 1) * 12;
                    for (int i = 0; i < left; i++)
                    {
                        ship.drawLeft(model, tint, blend, key + "-drawLeft-1" + "-" + i.ToString(), x, 35);
                        x += 24;
                    }
                }

                if (_isGameOver)
                {
                    if (gameOverCnt > 60)
                        Letter.drawString(model, tint, blend, key + "-drawString-1", "GAME OVER", 214, 200, 12);
                }
                else if (paused)
                {
                    if (pauseCnt % 120 < 60)
                        Letter.drawString(model, tint, blend, key + "-drawString-2", "PAUSE", 290, 420, 7);
                }

                if (state == GameManagerGameState.IN_GAME)
                    break;
                titleManager.draw(model, tint, blend, key + "-draw-1");
                break;
            case GameManagerGameState.TITLE:
                titleManager.draw(model, tint, blend, key + "-draw-2");
                break;
        }
    }

    public virtual bool isGameOver_0()
    {
        return _isGameOver;
    }

    public virtual bool isGameOver_1(bool v)
    {
        _isGameOver = v;
        return _isGameOver;
    }
}

public static class GameManagerGameState
{
    public const int TITLE = 0;
    public const int REPLAY = 1;
    public const int IN_GAME = 2;
}
