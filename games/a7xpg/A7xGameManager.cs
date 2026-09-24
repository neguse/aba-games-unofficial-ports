// Copyright 2003 Kenta Cho. All rights reserved.
using System;
using static GameMath;
using static Lub;

public class A7xGameManager
{
    Mesh logo, glowMesh;
    public const int ENEMY_MAX = 32;
    public A7xPrefManager prefManager;
    public Rand rand;
    public Field field;
    public Ship ship;
    public LuminousActorPool golds;
    public LuminousActorPool enemies;
    public LuminousActorPool particles;
    public ActorPool bonuses;
    public const int FIRST_EXTEND = 20000;
    public const int EVERY_EXTEND = 50000;
    public const int LEFT_MAX = 9;
    public int stage, lap;
    public int left;
    public int score, extendScore;
    public int leftGold, appGold;
    public int enemyAppInterval;
    public int enemyTimer;
    public int stageTimer;
    public const int TITLE = 0, IN_GAME = 1, STAGE_CLEAR = 2, GAMEOVER = 3, PAUSE = 4;
    public int state;
    public int cnt;
    public int timeBonus;
    public const int CONTINUE_ENABLE_SCORE = 100000;
    public bool continueEnable;
    public int contCy;
    public int pauseCnt;
    public Sound[] bgm = new Sound[3];
    public Sound[] se = new Sound[12];
    public const int STAGE_NUM = 30;
    public float[][] stgData = new float[][]
    {
        new float[] { 24, 18, 40, 10, 1, 300, 4, 0, 1, 0.2f },
        new float[] { 21, 21, 30, 12, 2, 280, 4, 1, 1, 0.25f },
        new float[] { 25, 20, 35, 15, 2, 320, 5, 2, 1, 0.3f },
        new float[] { 20, 22, 30, 12, 1, 300, 1, 1, 1, 0.2f, 2, 0, 1, 0.25f, 3, 1, 1, 0.3f },
        new float[] { 24, 18, 45, 20, 2, 150, 1, 3, 2.5f, 0.2f, 5, 2, 1, 0.35f },
        new float[] { 24, 18, 30, 10, 1, 200, 2, 0, 1, 0.4f, 2, 1, 1, 0.3f, 2, 2, 1, 0.3f },
        new float[] { 25, 16, 40, 18, 3, 250, 6, 4, 5, 0.3f },
        new float[] { 30, 12, 35, 12, 1, 50, 4, 1, 1.5f, 0.28f },
        new float[] { 18, 20, 45, 18, 2, 200, 3, 3, 1, 0.5f, 1, 0, 1.5f, 0.4f, 3, 3, 1, 0.2f },
        new float[] { 30, 28, 50, 22, 2, 100, 1, 4, 12, 0.2f, 2, 0, 1.5f, 0.2f, 3, 1, 1.5f, 0.3f, 2, 3, 1.5f, 0.4f },
        new float[] { 24, 18, 40, 16, 2, 30, 6, 1, 1, 0.32f },
        new float[] { 13, 13, 30, 12, 1, 100, 1, 0, 1, 0.3f, 1, 1, 1, 0.3f, 1, 2, 1, 0.3f, 1, 3, 1, 0.3f },
        new float[] { 25, 16, 45, 18, 2, 200, 2, 1, 3, 0.2f, 2, 0, 3, 0.3f },
        new float[] { 8, 22, 30, 15, 3, 180, 1, 4, 8, 0.2f, 3, 2, 0.8f, 0.3f },
        new float[] { 22, 20, 60, 25, 2, 100, 1, 5, 2, 0.15f, 2, 4, 5, 0.2f, 4, 0, 1, 0.4f },
        new float[] { 24, 18, 50, 22, 3, 150, 3, 0, 1, 0.35f, 3, 4, 5, 0.4f, 3, 0, 1, 0.35f },
        new float[] { 16, 26, 40, 15, 2, 100, 7, 2, 1, 0.5f },
        new float[] { 15, 15, 45, 20, 2, 160, 1, 4, 15, 0.1f, 3, 5, 1, 0.2f },
        new float[] { 28, 22, 20, 20, 10, 10, 1, 1, 0.5f, 0.5f, 4, 4, 10, 0.1f },
        new float[] { 26, 26, 60, 25, 2, 100, 3, 0, 4, 0.15f, 3, 3, 1, 0.4f, 3, 4, 1.5f, 0.6f },
        new float[] { 24, 18, 45, 15, 2, 120, 2, 5, 1, 0.4f, 3, 0, 1, 0.3f, 3, 2, 1.5f, 0.4f },
        new float[] { 12, 12, 30, 10, 1, 50, 4, 0, 1, 0.27f },
        new float[] { 20, 25, 50, 25, 4, 100, 8, 5, 0.6f, 0.37f },
        new float[] { 27, 18, 55, 20, 2, 150, 3, 4, 6, 0.5f, 4, 3, 1, 0.5f, 1, 1, 1, 0.5f },
        new float[] { 20, 20, 60, 25, 3, 120, 1, 3, 7, 0.3f, 2, 2, 1, 0.4f, 3, 1, 1, 0.3f, 4, 0, 1, 0.35f },
        new float[] { 24, 18, 50, 20, 2, 75, 1, 0, 1, 0.4f, 1, 0, 1.25f, 0.35f, 1, 0, 1.5f, 0.3f, 1, 0, 1.75f, 0.25f, 1, 0, 2, 0.2f, 1, 0, 2.25f, 0.15f, 1, 0, 2.5f, 0.1f },
        new float[] { 29, 25, 60, 25, 3, 100, 1, 4, 15, 0.25f, 1, 4, 10, 0.3f, 1, 3, 2, 0.35f, 1, 5, 1.5f, 0.35f, 1, 1, 2, 0.35f, 4, 0, 1, 0.3f },
        new float[] { 20, 24, 40, 15, 1, 120, 7, 4, 2, 0.6f },
        new float[] { 22, 22, 60, 30, 5, 100, 4, 4, 8, 0.3f, 4, 3, 2, 0.4f },
        new float[] { 24, 18, 40, 10, 1, 20, 4, 0, 1, 0.2f, 3, 1, 1, 0.25f, 2, 2, 1, 0.3f, 1, 3, 1, 0.3f, 1, 4, 3, 0.3f, 1, 5, 1, 0.2f },
    };
    public float[][] enemyTable = new float[ENEMY_MAX][];
    public Input input = new Input();
    public int enemyTableIdx, enemyNum;
    public void init()
    {
        prefManager = new A7xPrefManager();
        for (int i = 0; i < ENEMY_MAX; i++)
            enemyTable[i] = new float[3];
        rand = new Rand();
        field = new Field();
        field.init();
        Ship.createMeshes();
        ship = new Ship();
        ship.init(input, field, this);
        Gold.createMeshes();
        Gold goldClass = new Gold();
        GoldInitializer gi = new GoldInitializer(ship, field, rand, this);
        golds = new LuminousActorPool(16, goldClass, gi);
        Enemy.createMeshes();
        Enemy enemyClass = new Enemy();
        EnemyInitializer ei = new EnemyInitializer(ship, field, rand, this);
        enemies = new LuminousActorPool(ENEMY_MAX, enemyClass, ei);
        Particle particleClass = new Particle();
        ParticleInitializer pi = new ParticleInitializer(field, rand);
        particles = new LuminousActorPool(256, particleClass, pi);
        Bonus bonusClass = new Bonus();
        BonusInitializer bi = new BonusInitializer();
        bonuses = new ActorPool(8, bonusClass, bi);
        LetterRender.createMeshes();
        for (int i = 0; i < 3; i++)
            bgm[i] = new Sound();
        bgm[0].loadSound("bgm1.ogg");
        bgm[1].loadSound("bgm2.ogg");
        bgm[2].loadSound("bgm3.ogg");
        for (int i = 0; i < 12; i++)
            se[i] = new Sound();
        se[0].loadChunk("getgold.wav", 0);
        se[1].loadChunk("boost.wav", 1);
        se[2].loadChunk("miss.wav", 2);
        se[3].loadChunk("invincible.wav", 3);
        se[4].loadChunk("invfast.wav", 3);
        se[5].loadChunk("enemyapp.wav", 4);
        se[6].loadChunk("enemycrash.wav", 5);
        se[7].loadChunk("extend.wav", 6);
        se[8].loadChunk("stagestart.wav", 7);
        se[9].loadChunk("stageend.wav", 7);
        se[10].loadChunk("startinv.wav", 7);
        se[11].loadChunk("accel.wav", 2);
    }

    public void start()
    {
        stage = 0;
        startTitle();
    }

    public void close()
    {
        for (int i = 0; i < 3; i++)
            bgm[i].free();
        for (int i = 0; i < 12; i++)
            se[i].free();
        LetterRender.deleteMeshes();
        Enemy.deleteMeshes();
        Gold.deleteMeshes();
        Ship.deleteMeshes();
    }

    public void playSe(int n)
    {
        if (state != IN_GAME && state != STAGE_CLEAR)
            return;
        se[n].playChunk();
    }

    public void stopSe(int n)
    {
        se[n].haltChunk();
    }

    public void addGold()
    {
        Gold gold = (Gold)golds.getInstance();
        gold.set();
    }

    public void addScore(int sc)
    {
        score = score + (sc);
        if (score > extendScore)
        {
            if (left < LEFT_MAX)
            {
                playSe(7);
                left++;
            }

            if (extendScore <= FIRST_EXTEND)
                extendScore = EVERY_EXTEND;
            else
                extendScore = extendScore + ((EVERY_EXTEND * (lap + 1)));
        }
    }

    public void addBonus(int sc, Vector pos, float size)
    {
        addScore(sc);
        Bonus bonus = (Bonus)bonuses.getInstanceForced();
        bonus.set(sc, pos, size);
    }

    public void getGold()
    {
        playSe(0);
        addBonus((GameMath.integer((ship.speed / (Ship.DEFAULT_SPEED / 2)))) * 10, ship.pos, 0.7f);
        leftGold--;
        if (leftGold - appGold >= 0)
            addGold();
        ship.addGauge();
        if (leftGold <= 0)
            startStageClear();
    }

    public void shipDestroyed()
    {
        playSe(2);
        left--;
        if (left < 0)
            startGameover();
    }

    public void addEnemy(int type, float size, float speed)
    {
        playSe(5);
        Enemy enemy = (Enemy)enemies.getInstance();
        if ((!((enemy) != null)) != null)
            return;
        enemy.set(type, size, speed);
    }

    public void addParticle(Vector pos, float deg, float ofs, float speed, float r, float g, float b)
    {
        Particle pt = (Particle)particles.getInstanceForced();
        pt.set(pos, deg, ofs, speed, r, g, b);
    }

    public void startStage(bool cont)
    {
        int st = stage % STAGE_NUM;
        lap = GameMath.integer(stage / STAGE_NUM);
        field.size.x = stgData[st][0];
        field.size.y = stgData[st][1];
        field.eyeZ = 300;
        field.alpha = 1;
        stageTimer = GameMath.integer(stgData[st][2] * 60);
        leftGold = GameMath.integer(stgData[st][3]);
        appGold = GameMath.integer(stgData[st][4]);
        enemyAppInterval = GameMath.integer(stgData[st][5]);
        int ei = 0;
        for (int i = 6; i < stgData[st].Length;)
        {
            int n = GameMath.integer(stgData[st][i]);
            i++;
            int tp = GameMath.integer(stgData[st][i]);
            i++;
            float sz = stgData[st][i];
            i++;
            float sp = stgData[st][i];
            i++;
            for (int j = 0; j < n; j++)
            {
                enemyTable[ei][0] = tp;
                enemyTable[ei][1] = sz;
                enemyTable[ei][2] = sp;
                ei++;
            }
        }

        restartStage();
        enemyTimer = 0;
        enemyNum = ei;
        field.start(GameMath.integer(st / 5));
        ship.startRound();
        ship.start();
        enemies.clear();
        bonuses.clear();
        particles.clear();
        golds.clear();
        for (int i = 0; i < appGold; i++)
        {
            addGold();
        }

        cnt = 0;
        if (st % 5 == 0 || cont)
        {
            bgm[(GameMath.integer(st / 5)) % bgm.Length].playMusic();
        }

        playSe(8);
    }

    public void restartStage()
    {
        enemyTimer = 120;
        enemyTableIdx = 0;
        enemies.clear();
    }

    public void initShipState()
    {
        left = 2;
        score = 0;
        extendScore = FIRST_EXTEND;
    }

    public void startInGameContinue()
    {
        state = IN_GAME;
        initShipState();
        startStage(true);
    }

    public void startInGame()
    {
        state = IN_GAME;
        initShipState();
        stage = 0;
        startStage(false);
    }

    public void startStageClear()
    {
        playSe(9);
        state = STAGE_CLEAR;
        field.eyeZa = 300;
        ship.speed = Ship.DEFAULT_SPEED;
        cnt = 0;
        if (stageTimer > 0)
            timeBonus = (GameMath.integer(stageTimer * 17 / 100)) * 10;
        else
            timeBonus = 0;
        if (stage % 5 == 4)
            Sound.fadeMusic();
    }

    public void gotoNextStage()
    {
        state = IN_GAME;
        stage++;
        startStage(false);
    }

    public void startTitle()
    {
        state = TITLE;
        startStage(false);
        cnt = 0;
        field.eyeZ = field.eyeZa;
        Sound.stopMusic();
    }

    public void startGameover()
    {
        state = GAMEOVER;
        cnt = 0;
        if (score > prefManager.hiScore)
        {
            prefManager.hiScore = score;
            prefManager.save();
        }

        if (score > CONTINUE_ENABLE_SCORE && stage < STAGE_NUM)
        {
            continueEnable = true;
            contCy = 0;
        }
        else
        {
            continueEnable = false;
        }

        Sound.fadeMusic();
    }

    public void startPause()
    {
        state = PAUSE;
        pauseCnt = 0;
    }

    public void resumePause()
    {
        state = IN_GAME;
    }

    public void stageMove()
    {
        enemyTimer--;
        if (enemyTimer < 0)
        {
            if (enemyTableIdx == 0 && lap >= 1)
            {
                int ei = enemyNum - 1;
                for (int i = 0; i < lap * 2; i++)
                {
                    addEnemy(GameMath.integer(enemyTable[ei][0]), enemyTable[ei][1] * (1 + lap * 0.1f), enemyTable[ei][2] * (1 + lap * 0.1f));
                    ei--;
                    if (ei < 0)
                        ei = enemyNum - 1;
                }
            }

            enemyTimer = enemyAppInterval;
            addEnemy(GameMath.integer(enemyTable[enemyTableIdx][0]), enemyTable[enemyTableIdx][1], enemyTable[enemyTableIdx][2]);
            enemyTableIdx++;
            if (enemyTableIdx >= enemyNum)
                enemyTimer = 999999999;
        }

        if (ship.cnt > -Ship.INVINCIBLE_CNT)
        {
            stageTimer--;
            if (stageTimer <= 0)
                startStageClear();
        }
    }

    public bool pPrsd = true;
    public void inGameMove()
    {
        stageMove();
        field.move();
        ship.move();
        golds.move();
        enemies.move();
        particles.move();
        bonuses.move();
        if (input.pause)
        {
            if (!pPrsd)
            {
                pPrsd = true;
                startPause();
            }
        }
        else
        {
            pPrsd = false;
        }
    }

    public bool btnPrsd, gotoNextState;
    public void stageClearMove()
    {
        if (cnt <= 64)
        {
            btnPrsd = true;
            gotoNextState = false;
        }
        else
        {
            if ((input.getButtonState() & (Input.PAD_BUTTON1 | Input.PAD_BUTTON2)) != 0)
            {
                if (!btnPrsd)
                    gotoNextState = true;
            }
            else
            {
                btnPrsd = false;
            }
        }

        if (cnt == 64)
        {
            addScore(timeBonus);
        }
        else if ((cnt > 64 && gotoNextState) || cnt > 300)
        {
            if (stageTimer > 0)
            {
                gotoNextStage();
            }
            else
            {
                left--;
                if (left < 0)
                {
                    ship.start();
                    startGameover();
                }
                else
                {
                    gotoNextStage();
                }
            }
        }

        field.move();
        field.alpha = field.alpha * (0.96f);
        particles.move();
        bonuses.move();
    }

    public void titleMove()
    {
        if (cnt <= 8)
        {
            btnPrsd = true;
        }
        else
        {
            if ((input.getButtonState() & (Input.PAD_BUTTON1 | Input.PAD_BUTTON2)) != 0)
            {
                if (!btnPrsd)
                    startInGame();
            }
            else
            {
                btnPrsd = false;
            }
        }

        stageMove();
        field.addSpeed(Ship.DEFAULT_SPEED / 2);
        field.move();
        enemies.move();
        particles.move();
    }

    public void gameoverMove()
    {
        if (cnt <= 64)
        {
            btnPrsd = true;
            gotoNextState = false;
        }
        else
        {
            if ((input.getButtonState() & (Input.PAD_BUTTON1 | Input.PAD_BUTTON2)) != 0)
            {
                if (!btnPrsd)
                    gotoNextState = true;
            }
            else
            {
                btnPrsd = false;
            }

            if (continueEnable)
            {
                int pad = input.getPadState();
                if ((pad & Input.PAD_UP) != 0)
                {
                    contCy = 0;
                    cnt = 65;
                }
                else if ((pad & Input.PAD_DOWN) != 0)
                {
                    contCy = 1;
                    cnt = 65;
                }
            }
        }

        if (cnt > 64 && gotoNextState)
        {
            if (continueEnable && contCy == 0)
                startInGameContinue();
            else
                startTitle();
        }
        else if (cnt > 500)
        {
            startTitle();
        }

        field.addSpeed(Ship.DEFAULT_SPEED / 2);
        field.move();
        enemies.move();
        particles.move();
    }

    public void pauseMove()
    {
        pauseCnt++;
        if (input.pause)
        {
            if (!pPrsd)
            {
                pPrsd = true;
                resumePause();
            }
        }
        else
        {
            pPrsd = false;
        }
    }

    public void move()
    {
        if (input.escape && state != TITLE)
            startTitle();
        switch (state)
        {
            case IN_GAME:
                inGameMove();
                break;
            case STAGE_CLEAR:
                stageClearMove();
                break;
            case TITLE:
                titleMove();
                break;
            case GAMEOVER:
                gameoverMove();
                break;
            case PAUSE:
                pauseMove();
                break;
            default:
                break;
        }

        cnt++;
    }

    public void inGameDraw(float[] model, float[] tint, Gfx.Blend blend)
    {
        float lineWidth = 1;
        bonuses.draw(model, tint, blend);
        field.draw(model, tint, blend);
        golds.draw(model, tint, blend);
        { var mesh = new Mesh("particles-draw"); particles.draw(model, tint, blend, mesh); if (mesh.count > 0) Gfx.Draw(mesh.count, mesh.Bindings(model, tint, lineWidth, blend == Gfx.Blend.Additive, 0, null, 640, 480),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend }); }
        ship.draw(model, tint, blend);
        enemies.draw(model, tint, blend);
    }

    public void stageClearDraw(float[] model, float[] tint, Gfx.Blend blend)
    {
        float lineWidth = 1;
        if (cnt < 32)
            field.draw(model, tint, blend);
        { var mesh = new Mesh("particles-draw"); particles.draw(model, tint, blend, mesh); if (mesh.count > 0) Gfx.Draw(mesh.count, mesh.Bindings(model, tint, lineWidth, blend == Gfx.Blend.Additive, 0, null, 640, 480),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend }); }
    }

    public void titleDraw(float[] model, float[] tint, Gfx.Blend blend)
    {
        float lineWidth = 1;
        { var mesh = new Mesh("particles-draw"); particles.draw(model, tint, blend, mesh); if (mesh.count > 0) Gfx.Draw(mesh.count, mesh.Bindings(model, tint, lineWidth, blend == Gfx.Blend.Additive, 0, null, 640, 480),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend }); }
        enemies.draw(model, tint, blend);
    }

    public void gameoverDraw(float[] model, float[] tint, Gfx.Blend blend)
    {
        float lineWidth = 1;
        field.draw(model, tint, blend);
        { var mesh = new Mesh("particles-draw"); particles.draw(model, tint, blend, mesh); if (mesh.count > 0) Gfx.Draw(mesh.count, mesh.Bindings(model, tint, lineWidth, blend == Gfx.Blend.Additive, 0, null, 640, 480),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend }); }
        enemies.draw(model, tint, blend);
    }

    public void inGameDrawLuminous(float[] model, float[] tint, Gfx.Blend blend)
    {
        float lineWidth = 1;
        field.drawLuminous(model, tint, blend);
        golds.drawLuminous(model, tint, blend);
        lineWidth = 2;
        { var mesh = new Mesh("particles-drawLuminous"); particles.drawLuminous(model, tint, blend, mesh); if (mesh.count > 0) Gfx.Draw(mesh.count, mesh.Bindings(model, tint, lineWidth, blend == Gfx.Blend.Additive, 0, null, 128, 128),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend }); }
        lineWidth = 1;
        ship.drawLuminous(model, tint, blend);
        enemies.drawLuminous(model, tint, blend);
    }

    public void stageClearDrawLuminous(float[] model, float[] tint, Gfx.Blend blend)
    {
        float lineWidth = 1;
        if (cnt < 32)
            field.drawLuminous(model, tint, blend);
        { var mesh = new Mesh("particles-drawLuminous"); particles.drawLuminous(model, tint, blend, mesh); if (mesh.count > 0) Gfx.Draw(mesh.count, mesh.Bindings(model, tint, lineWidth, blend == Gfx.Blend.Additive, 0, null, 128, 128),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend }); }
    }

    public void titleDrawLuminous(float[] model, float[] tint, Gfx.Blend blend)
    {
        float lineWidth = 1;
        field.drawLuminous(model, tint, blend);
        { var mesh = new Mesh("particles-drawLuminous"); particles.drawLuminous(model, tint, blend, mesh); if (mesh.count > 0) Gfx.Draw(mesh.count, mesh.Bindings(model, tint, lineWidth, blend == Gfx.Blend.Additive, 0, null, 128, 128),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend }); }
        enemies.drawLuminous(model, tint, blend);
    }

    public void gameoverDrawLuminous(float[] model, float[] tint, Gfx.Blend blend)
    {
        float lineWidth = 1;
        field.drawLuminous(model, tint, blend);
        { var mesh = new Mesh("particles-drawLuminous"); particles.drawLuminous(model, tint, blend, mesh); if (mesh.count > 0) Gfx.Draw(mesh.count, mesh.Bindings(model, tint, lineWidth, blend == Gfx.Blend.Additive, 0, null, 128, 128),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend }); }
        enemies.drawLuminous(model, tint, blend);
    }

    public void drawScore(float[] model, float[] tint, Gfx.Blend blend)
    {
        LetterRender.drawNum(model, tint, blend, score, 300, 20, 10);
    }

    public void drawHiScore(float[] model, float[] tint, Gfx.Blend blend)
    {
        LetterRender.drawNum(model, tint, blend, prefManager.hiScore, 620, 20, 10);
    }

    public void drawStageTimer(float[] model, float[] tint, Gfx.Blend blend)
    {
        LetterRender.drawTime(model, tint, blend, stageTimer * 17, 620, 20, 10);
    }

    public void inGameDrawStatus(float[] model, float[] tint, Gfx.Blend blend)
    {
        if (state == IN_GAME && cnt < 120)
        {
            LetterRender.drawString(model, tint, blend, "STAGE", 200, 180, 22);
            LetterRender.drawNum(model, tint, blend, stage + 1, 440, 180, 22);
        }

        drawScore(model, tint, blend);
        if (state == STAGE_CLEAR || cnt > 120)
            drawStageTimer(model, tint, blend);
        LetterRender.drawNum(model, tint, blend, left, 80, 460, 10);
        float[] parent1 = model;
        model = Transform.Translate(model, 30, 460, 0);
        model = Transform.Scale(model, 11, -11, 1);
        { Mesh shape2 = Ship.meshes[0]; if (shape2.count > 0) Gfx.Draw(shape2.count, shape2.Bindings(model, tint, 1, blend == Gfx.Blend.Additive, 0, null, 640, 480),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend }); }
        { Mesh shape3 = Ship.meshes[1]; if (shape3.count > 0) Gfx.Draw(shape3.count, shape3.Bindings(model, tint, 1, blend == Gfx.Blend.Additive, 0, null, 640, 480),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend }); }
        model = parent1;
        if (state == IN_GAME)
        {
            ship.drawGauge(model, tint, blend);
            LetterRender.drawNum(model, tint, blend, leftGold, 80, 430, 10);
            float[] parent4 = model;
            model = Transform.Translate(model, 30, 430, 0);
            model = Transform.Scale(model, 11, -11, 1);
            { Mesh shape5 = Gold.meshes[0]; if (shape5.count > 0) Gfx.Draw(shape5.count, shape5.Bindings(model, tint, 1, blend == Gfx.Blend.Additive, 0, null, 640, 480),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend }); }
            { Mesh shape6 = Gold.meshes[1]; if (shape6.count > 0) Gfx.Draw(shape6.count, shape6.Bindings(model, tint, 1, blend == Gfx.Blend.Additive, 0, null, 640, 480),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend }); }
            model = parent4;
        }
    }

    public void stageClearDrawStatus(float[] model, float[] tint, Gfx.Blend blend)
    {
        if (stageTimer > 0)
        {
            LetterRender.drawString(model, tint, blend, "STAGE CLEAR", 100, 150, 24);
            if (cnt > 32)
                LetterRender.drawString(model, tint, blend, "TIME BONUS", 80, 240, 15);
            if (cnt > 64)
                LetterRender.drawNum(model, tint, blend, timeBonus, 550, 290, 15);
        }
        else
        {
            LetterRender.drawString(model, tint, blend, "TIME OVER", 124, 150, 24);
        }
    }

    public void titleDrawStatus(float[] model, float[] tint, Gfx.Blend blend)
    {
        if ((cnt % 120) < 60)
            LetterRender.drawString(model, tint, blend, "PUSH BUTTON TO START", 320, 400, 8);
        drawScore(model, tint, blend);
        drawHiScore(model, tint, blend);
        if (logo == null) {
            logo = new Mesh("title-logo");
            logo.Vertex(80, 50, 0, new float[] { 0, 0, 0, 1 });
            logo.Vertex(180, 50, 0, new float[] { 1, 0, 0, 1 });
            logo.Vertex(180, 150, 0, new float[] { 1, 1, 0, 1 });
            logo.Vertex(80, 150, 0, new float[] { 0, 1, 0, 1 });
            logo.Quads(0, 4);
        }
        Gfx.Draw(logo.count, logo.Bindings(model, new float[] { 1, 1, 1, 1 }, 1, true, 0, A7xData.title),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
    }

    public void gameoverDrawStatus(float[] model, float[] tint, Gfx.Blend blend)
    {
        if (cnt > 64)
        {
            LetterRender.drawString(model, tint, blend, "GAME OVER", 220, 200, 15);
            if (continueEnable)
            {
                LetterRender.drawString(model, tint, blend, "CONTINUE", 250, 270, 9);
                if (contCy == 0)
                {
                    LetterRender.drawString(model, tint, blend, "YES", 380, 260, 10);
                    LetterRender.drawString(model, tint, blend, "NO", 395, 280, 5);
                }
                else
                {
                    LetterRender.drawString(model, tint, blend, "YES", 395, 260, 5);
                    LetterRender.drawString(model, tint, blend, "NO", 395, 280, 10);
                }
            }
        }

        drawScore(model, tint, blend);
        drawHiScore(model, tint, blend);
    }

    public void pauseDrawStatus(float[] model, float[] tint, Gfx.Blend blend)
    {
        if ((pauseCnt % 60) < 30)
            LetterRender.drawString(model, tint, blend, "PAUSE", 280, 220, 12);
    }



    public void draw()
    {
        float[] model = Transform.Translate(Transform.Perspective(), 0, 0, -field.eyeZ);
        float[] tint = null; Gfx.Blend blend = Gfx.Blend.Additive;
        Gfx.BeginPass(new PassOpts { Target = Game.glow, ClearColor = new float[] { 0, 0, 0, 0 } });
        switch (state)
        {
            case IN_GAME:
            case PAUSE:
                inGameDrawLuminous(model, tint, blend);
                break;
            case STAGE_CLEAR:
                stageClearDrawLuminous(model, tint, blend);
                break;
            case TITLE:
                titleDrawLuminous(model, tint, blend);
                break;
            case GAMEOVER:
                gameoverDrawLuminous(model, tint, blend);
                break;
            default:
                break;
        }

        Gfx.EndPass();
        Gfx.BeginPass(new PassOpts { Target = Gfx.MainTex, ClearColor = new float[] { 0, 0, 0, 0 } });
        switch (state)
        {
            case IN_GAME:
            case PAUSE:
                inGameDraw(model, tint, blend);
                break;
            case STAGE_CLEAR:
                stageClearDraw(model, tint, blend);
                break;
            case TITLE:
                titleDraw(model, tint, blend);
                break;
            case GAMEOVER:
                gameoverDraw(model, tint, blend);
                break;
            default:
                break;
        }

        if (glowMesh == null) {
            glowMesh = new Mesh("glow-overlay");
            int[] dx = new int[] { 0, 5, -5, 0, 0 }, dy = new int[] { 0, 0, 0, 5, -5 };
            for (int i = 0; i < 5; i++) {
                for (int c = 0; c < 4; c++) {
                    float u = c >= 2 ? 1 : 0, v = c == 1 || c == 2 ? 1 : 0;
                    float x = u * 640 + dx[i], y = v * 480 + (c >= 2 ? dx[i] : dy[i]);
                    glowMesh.Vertex(x / 320 - 1, 1 - y / 240, 0, new float[] { u, v, 0, 1 });
                }
            }
            glowMesh.Quads(0, 20);
        }
        var bindings = glowMesh.Bindings(Transform.Identity(), new float[] { 0.5f, 0.4f, 0.45f, 0 });
        bindings["image"] = Game.glow;
        var uniforms = (System.Collections.Generic.Dictionary<string, object>)bindings["uniforms"];
        uniforms["options"] = new float[] { 1, 0, 0, 2 };
        Gfx.Draw(glowMesh.count, bindings, new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = Gfx.Blend.Additive });
        model = Transform.Ortho();
        switch (state)
        {
            case IN_GAME:
                inGameDrawStatus(model, tint, blend);
                break;
            case STAGE_CLEAR:
                inGameDrawStatus(model, tint, blend);
                stageClearDrawStatus(model, tint, blend);
                break;
            case TITLE:
                titleDrawStatus(model, tint, blend);
                break;
            case GAMEOVER:
                gameoverDrawStatus(model, tint, blend);
                break;
            case PAUSE:
                pauseDrawStatus(model, tint, blend);
                break;
            default:
                break;
        }

        Gfx.EndPass();
    }
}
