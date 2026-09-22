// Copyright 2006 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Drawing;

public class Frame
{
    public const string LAST_REPLAY_FILE_NAME = "last.rpl";
    public Pad pad;
    public TtnScreen screen;
    public Field field;
    public Player player;
    public PlayerSpec playerSpec;
    public EnemyPool enemies;
    public BulletPool bullets;
    public ParticlePool particles, bonusParticles;
    public PillarPool pillars;
    public Stage stage;
    public Title title;
    public Preference preference;
    public GameState gameState;
    public ReplayData replayData;
    public TitanionRand rand;
    public virtual void init_0()
    {
        Sound.load_0();
        preference = new Preference();
        Letter.init_0();
        pad = new RecordablePad();
        screen = new TtnScreen();
        field = new Field(this, screen);
        enemies = new EnemyPool(128);
        enemies.field(field);
        bullets = new BulletPool(1024);
        pillars = new PillarPool(48);
        TriangleParticleSpec triangleParticleSpec = new TriangleParticleSpec(field);
        LineParticleSpec lineParticleSpec = new LineParticleSpec(field);
        QuadParticleSpec quadParticleSpec = new QuadParticleSpec(field);
        BonusParticleSpec bonusParticleSpec = new BonusParticleSpec(field);
        particles = new ParticlePool(1024, new List<object> { triangleParticleSpec, lineParticleSpec, quadParticleSpec, bonusParticleSpec });
        bonusParticles = new ParticlePool(256, new List<object> { triangleParticleSpec, lineParticleSpec, quadParticleSpec, bonusParticleSpec });
        triangleParticleSpec.setParticles(particles);
        gameState = new GameState(this, preference);
        title = new Title(preference, pad, this);
        title.setMode(preference.lastMode);
        title.init_0();
        playerSpec = new PlayerSpec(pad, gameState, field, enemies, bullets, particles);
        player = new Player(playerSpec);
        triangleParticleSpec.setPlayer(player);
        lineParticleSpec.setPlayer(player);
        quadParticleSpec.setPlayer(player);
        bonusParticleSpec.setPlayer(player);
        stage = new Stage(field, enemies, bullets, player, particles, bonusParticles, pillars, gameState);
        gameState.setStage(stage);
        rand = new TitanionRand();
        loadLastReplay();
    }

    public virtual void quit()
    {
        title.close();
        playerSpec.close();
        gameState.close();
        stage.close();
        Letter.close();
    }

    public virtual void start_0()
    {
        startTitle();
    }

    public virtual void startInGame(int mode)
    {
        gameState.startInGame((int)mode);
        player.replayMode(false);
        RecordablePad rp = (pad is RecordablePad ? (RecordablePad)pad : null);
        rp.startRecord();
        replayData = new ReplayData();
        replayData.inputRecord = rp.inputRecord;
        replayData.seed = rand.nextInt32();
        clearAll();
        field.set_0();
        player.set_0();
        stage.start_1(replayData.seed);
        Sound.clearMarkedSes();
        Sound.playBgm();
    }

    public virtual void startTitle()
    {
        startReplay_0();
        title.start_0();
    }

    public virtual void startReplay_0()
    {
        gameState.startTitle();
        if ((replayData) != null)
        {
            player.replayMode(true);
            RecordablePad rp = (pad is RecordablePad ? (RecordablePad)pad : null);
            rp.startReplay_1(replayData.inputRecord);
        }

        clearAll();
        field.set_0();
        if ((replayData) != null)
        {
            gameState.mode_1((int)replayData.mode);
            gameState.setExtendScore();
            gameState.inReplay(true);
            player.set_0();
            stage.start_1(replayData.seed);
        }
        else
        {
            field.setEyePos(new Vector(0, 0));
        }

        Sound.clearMarkedSes();
        Sound.haltBgm();
    }

    public virtual void clearAll()
    {
        enemies.clear();
        bullets.clear();
        particles.clear();
        bonusParticles.clear();
        pillars.clear();
    }

    public virtual void breakLoop()
    {
    }

    public virtual void move_0()
    {
        gameState.move_0();
        field.move_0();
        if ((((gameState.isInGame())) || (((replayData) != null))))
        {
            if ((!((gameState.paused()))))
            {
                stage.move_0();
                pillars.move_0();
                player.move_0();
                enemies.move_0();
                bullets.move_0();
                particles.move_0();
                bonusParticles.move_0();
            }
        }

        if (gameState.isTitle())
            title.move_0();
    }

    public virtual void handleSound()
    {
        Sound.playMarkedSes();
    }

    public virtual void addSlowdownRatio(float sr)
    {
        slowdownRatio = slowdownRatio + (sr);
    }

    public virtual void draw_0()
    {
        field.setLookAt();
        if ((((gameState.isInGame())) || (((replayData) != null))))
        {
            pillars.drawOutside();
            field.drawBack();
            enemies.drawPillarBack();
            pillars.drawCenter();
            enemies.drawBack();
            field.drawFront();
            particles.draw_0();
            bonusParticles.draw_0();
            enemies.drawFront();
            player.draw_0();
            bullets.draw_0();
            field.beginDrawingFront();
            gameState.draw_0();
            if (gameState.isTitle())
                title.draw_0();
            player.drawState_0();
            field.resetLookAt();
            gameState.drawLeft();
        }
        else
        {
            pillars.drawOutside();
            field.drawBack();
            field.drawFront();
            field.beginDrawingFront();
            if (gameState.isTitle())
                title.draw_0();
        }
    }

    public string savedReplay = "";
    public float slowdownRatio;
    public virtual void saveLastReplay()
    {
        replayData.score = gameState.score;
        replayData.mode = gameState.mode_0();
        replayData.stageRandomized = stage.randomized;
        savedReplay = replayData.encode();
        if (Lub.Host.Available())
            Lub.Host.Send("replay.save", savedReplay);
    }

    public virtual void loadLastReplay()
    {
        replayData = null;
        if (savedReplay == "")
            return;
        ReplayData data = new ReplayData();
        if (!((data.decode(savedReplay))))
            return;
        replayData = data;
        gameState.lastGameScore(data.score);
        gameState.lastGameMode(data.mode);
        stage.randomized = data.stageRandomized;
    }
}

public class GameState
{
    public const int MODE_NUM = 3;
    public static string[] MODE_NAME = new string[]
    {
        "CLASSIC",
        " BASIC ",
        "MODERN"
    };
    public static bool stageRandomized = false;
    public const int MAX_LEFT = 4;
    public Frame frame;
    public Preference preference;
    public int scene;
    public Stage stage;
    public int score;
    public int _lastGameScore;
    public int _lastGameMode;
    public int nextExtendScore;
    public float _multiplier;
    public int left;
    public bool escPressed, pPressed;
    public bool _paused;
    public int pauseCnt;
    public bool _isGameOver;
    public int gameOverCnt;
    public PlayerShape playerShape;
    public PlayerLineShape playerLineShape;
    public bool _inReplay;
    public int _mode;
    public int extendScore;
    public int proximityMultiplier, pmDispCnt;
    public GameState(Frame frame, Preference preference)
    {
        this.frame = frame;
        this.preference = preference;
        playerShape = new PlayerShape();
        playerLineShape = new PlayerLineShape();
        clear();
        _lastGameScore = -1;
    }

    public virtual void setStage(Stage stage)
    {
        this.stage = stage;
    }

    public virtual void close()
    {
        playerShape.close();
        playerLineShape.close();
    }

    public virtual void startInGame(int m)
    {
        scene = GameStateScene.IN_GAME;
        clear();
        _mode = m;
        left = 2;
        setExtendScore();
        _lastGameScore = -1;
        preference.setMode(_mode);
        stage.randomized = stageRandomized;
    }

    public virtual void setExtendScore()
    {
        switch (_mode)
        {
            case GameStateMode.CLASSIC:
                extendScore = 100000;
                break;
            case GameStateMode.BASIC:
            case GameStateMode.MODERN:
                extendScore = 1000000;
                break;
        }

        nextExtendScore = extendScore;
    }

    public virtual void startTitle()
    {
        scene = GameStateScene.TITLE;
        clear();
        left = 2;
    }

    public virtual void clear()
    {
        score = 0;
        _multiplier = 1.0f;
        left = 0;
        gameOverCnt = 0;
        {
            _paused = false;
            _isGameOver = _paused;
        }

        _inReplay = false;
        pmDispCnt = 0;
    }

    public virtual void startGameOver()
    {
        if ((!((isInGameAndNotGameOver()))))
            return;
        _isGameOver = true;
        gameOverCnt = 0;
        Sound.fadeBgm();
        _lastGameScore = score;
        _lastGameMode = mode_0();
        preference.recordResult(score, _mode);
        preference.save();
    }

    public virtual void startGameOverWithoutRecording()
    {
        if (_isGameOver)
            return;
        _isGameOver = true;
        gameOverCnt = 0;
        Sound.fadeBgm();
    }

    public virtual void backToTitle()
    {
        if (isTitle())
        {
            frame.startReplay_0();
            return;
        }

        if (gameOverCnt > 120)
        {
            frame.saveLastReplay();
            frame.startTitle();
        }
    }

    public virtual void move_0()
    {
        handleEscKey();
        if (isInGameAndNotGameOver())
        {
            handlePauseKey();
            if (_paused)
            {
                pauseCnt++;
                return;
            }
        }

        if (isInGame())
        {
            if (!((_isGameOver)))
            {
                frame.handleSound();
            }
            else
            {
                gameOverCnt++;
                if (gameOverCnt < 60)
                    frame.handleSound();
                if (gameOverCnt > 1000)
                    backToTitle();
            }
        }
        else
        {
            if (_inReplay)
                frame.handleSound();
            if (_isGameOver)
            {
                gameOverCnt++;
                if (((_inReplay)) && ((gameOverCnt < 60)))
                    frame.handleSound();
                if (gameOverCnt > 120)
                    backToTitle();
            }
        }

        if (pmDispCnt > 0)
            pmDispCnt--;
    }

    public virtual void handleEscKey()
    {
        if (frame.pad.escape)
        {
            if (!((escPressed)))
            {
                escPressed = true;
                if (scene == GameStateScene.IN_GAME)
                {
                    frame.loadLastReplay();
                    frame.startTitle();
                }
                else
                {
                    frame.breakLoop();
                }
            }
        }
        else
        {
            escPressed = false;
        }
    }

    public virtual void handlePauseKey()
    {
        if (frame.pad.pause)
        {
            if (!((pPressed)))
            {
                pPressed = true;
                _paused = !((_paused));
                pauseCnt = 0;
            }
        }
        else
        {
            pPressed = false;
        }
    }

    public virtual void addScore_2(int sc, bool noMultiplier = false)
    {
        if (!((_isGameOver)))
        {
            if (noMultiplier)
                score = score + (sc);
            else
                score = GameMath.integer(score + (sc * _multiplier));
            if (score >= nextExtendScore)
            {
                if (left < MAX_LEFT)
                {
                    left++;
                    Sound.playSe("extend.wav");
                }

                nextExtendScore = nextExtendScore + (extendScore);
                if (_mode == GameStateMode.MODERN)
                    extendScore = extendScore + (1000000);
            }
        }
    }

    public virtual void addMultiplier(float mp)
    {
        if (!((_isGameOver)))
            _multiplier = _multiplier + (mp);
    }

    public virtual void mulMultiplier(float mp)
    {
        if (!((_isGameOver)))
        {
            _multiplier = _multiplier * (mp);
            if (_multiplier < 1)
                _multiplier = 1;
        }
    }

    public virtual void setProximityMultiplier(int pm)
    {
        proximityMultiplier = pm;
        pmDispCnt = 60;
    }

    public virtual void destroyedPlayer()
    {
        left--;
        if (left < 0)
        {
            if (isInGame())
                startGameOver();
            else
                startGameOverWithoutRecording();
        }
    }

    public virtual void countShotFired()
    {
        stage.countShotFired();
    }

    public virtual void countShotHit()
    {
        stage.countShotHit();
    }

    public virtual void draw_0()
    {
        Letter.drawNum(score, 132, 5, 7);
        Letter.drawNum(nextExtendScore, 134, 25, 5);
        if (_lastGameScore >= 0)
        {
            Letter.drawNum(_lastGameScore, 360, 5, 7);
        }

        Letter.drawNum(GameMath.integer((_multiplier * 100)), 626, 4, 9, 3, 33, 2);
        if (pmDispCnt > 0)
            Letter.drawNum(proximityMultiplier, 626, 30, 7, 0, 33);
        stage.drawPhaseNum();
        if (isInGame())
        {
            if (!((_isGameOver)))
                stage.draw_0();
            if (_isGameOver)
            {
                if (gameOverCnt > 60)
                {
                    Letter.drawString("GAME OVER", 214, 200, 12);
                    stage.drawGameover();
                }
            }
            else if (_paused)
            {
                if (pauseCnt % 120 < 60)
                    Letter.drawString("PAUSE", 290, 420, 7);
            }

            Letter.drawString(GameState.MODE_NAME[mode_0()], 540, 400, 5);
        }
    }

    public virtual void drawLeft()
    {
        for (int i = 0; i < left; i++)
        {
            glPushMatrix();
            glTranslatef(-10.2f + i, -7.5f, -10);
            glScalef(0.6f, 0.6f, 0.6f);
            playerShape.draw_0();
            TtnScreen.setColor(0, 0, 0);
            playerLineShape.draw_0();
            glPopMatrix();
        }
    }

    public virtual bool isInGame()
    {
        return (scene == GameStateScene.IN_GAME);
    }

    public virtual bool isInGameAndNotGameOver()
    {
        return (((scene == GameStateScene.IN_GAME)) && ((!((_isGameOver)))));
    }

    public virtual bool isTitle()
    {
        return (scene == GameStateScene.TITLE);
    }

    public virtual bool isGameOver()
    {
        return _isGameOver;
    }

    public virtual bool paused()
    {
        return _paused;
    }

    public virtual float multiplier()
    {
        return _multiplier;
    }

    public virtual bool inReplay(bool v)
    {
        _inReplay = v;
        return _inReplay;
    }

    public virtual int lastGameScore(int v)
    {
        _lastGameScore = v;
        return _lastGameScore;
    }

    public virtual int lastGameMode(int v)
    {
        _lastGameMode = v;
        return _lastGameMode;
    }

    public virtual int mode_0()
    {
        return _mode;
    }

    public virtual int mode_1(int v)
    {
        _mode = v;
        return _mode;
    }
}

public static class GameStateMode
{
    public const int CLASSIC = 0;
    public const int BASIC = 1;
    public const int MODERN = 2;
}

public static class GameStateScene
{
    public const int TITLE = 0;
    public const int IN_GAME = 1;
}
