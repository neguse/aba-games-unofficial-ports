// Copyright 2004 Kenta Cho. All rights reserved.
using System;
using static GameMath;
using static Drawing;
public class GameManager {

  public bool nowait = false;
  public int state;
  public int stage;
  public int mode = GameMode.EXTRA;

  public Pad pad;
  public PrefManager prefManager;
  public StageManager stageManager;
  public Screen screen;
  public Rand rand;
  public Field field;
  public Ship ship;
  public ParticlePool particles;
  public ActorPool fragments;
  public BulletActorPool bullets;
  public EnemyPool enemies;
  public SplinterPool splinters;
  public ActorPool signs;
  public DamageGauge gauge;
  public MobileLetterPool letters;
  public AttractManager attractManager;
  public int cnt;
  public int pauseCnt;
  public const float SLOWDOWN_START_BULLETS_SPEED = 30;
  public float interval;
  public int score;
  public const int LEFT_BONUS_NORMAL = 10000;
  public const int LEFT_BONUS_EXTRA = 30000;
  public int leftBonus;
  public const int LEFT_NUM = 2;
  public int left;
  public const int FIRST_EXTEND_NORMAL = 100000;
  public const int EVERY_EXTEND_NORMAL = 300000;
  public const int FIRST_EXTEND_EXTRA = 200000;
  public const int EVERY_EXTEND_EXTRA = 500000;
  public int firstExtend, everyExtend;
  public int extendScore;
  public const int BOSSTIMER_FREEZED = 999999999;
  public int bossTimer;
  public const int STAGE_END_CNT = 360;
  public int bossDstCnt;
  public const int CREDIT_NUM = 2;
  public int credit;

  public void init() {
    Drawing.BeginFrame();
    GameData.Initialize();
    pad = new Pad();
    prefManager = new PrefManager();
    screen = new Screen();
    rand = new Rand();
    field = new Field();
    field.init();
    ParticleInitializer pi = new ParticleInitializer();
    particles = new ParticlePool(128, pi);
    Fragment fragmentClass = new Fragment();
    FragmentInitializer fi = new FragmentInitializer();
    fragments = new ActorPool(128, fragmentClass, fi);
    ship = new Ship();
    ship.init(pad, field, particles, fragments, this);
    SplinterInitializer si = new SplinterInitializer(ship, field, particles, this);
    splinters = new SplinterPool(144, si);
    BulletActorInitializer bi = new BulletActorInitializer(field, ship, particles, splinters);
    bullets = new BulletActorPool(512, bi);
    ship.setBulletActorPool(bullets);
    ship.initStuckEnemies(splinters);
    gauge = new DamageGauge();
    EnemyInitializer ei = new EnemyInitializer
      (this, field, bullets, ship, splinters, particles, fragments, gauge);
    enemies = new EnemyPool(64, ei);
    bullets.setEnemies(enemies);
    ScoreSignInitializer ssi = new ScoreSignInitializer();
    ScoreSign scoreSignClass = new ScoreSign();
    signs = new ActorPool(32, scoreSignClass, ssi);
    MobileLetterInitializer mli = new MobileLetterInitializer();
    letters = new MobileLetterPool(32, mli, field);
    stageManager = new StageManager(this, enemies, field);
    bullets.setStageManager(stageManager);
    attractManager = new AttractManager(pad, prefManager, this);
    SoundManager.init(this);
    Tumiki.createDisplayLists();
    LetterRender.createDisplayLists();
  }

  public void start() {
    stage = 0;
    startTitle();
  }

  public void close() {

    SoundManager.close();
    LetterRender.deleteDisplayLists();
    Tumiki.deleteDisplayLists();
  }

  public void startTitle() {
    state = GameState.TITLE;
    Music.haltMusic();
    if (stage >= StageManager.STAGE_NUM)
      stage = StageManager.STAGE_NUM - 1;
    field.start(stage);
    field.setGroundY(0);
    letters.clear();
    signs.clear();
    attractManager.startTitle();
  }

  public void startInGame() {
    state = GameState.IN_GAME;
    score = 0;
    left = LEFT_NUM;
    if (mode == GameMode.NORMAL) {
      firstExtend = FIRST_EXTEND_NORMAL;
      everyExtend = EVERY_EXTEND_NORMAL;
      leftBonus = LEFT_BONUS_NORMAL;
    } else {
      firstExtend = FIRST_EXTEND_EXTRA;
      everyExtend = EVERY_EXTEND_EXTRA;
      leftBonus = LEFT_BONUS_EXTRA;
    }
    extendScore = firstExtend;
    startStage();
    field.setGroundY(0);
    Splinter.setSignNum(0);
    signs.clear();
  }

  public void startInGameFirst() {
    stage = 0;
    startInGame();
    state = GameState.START_GAME;
    ship.startStage();
    Splinter.setSignNum(2);
    credit = CREDIT_NUM;
  }

  public void startEnding() {
    state = GameState.END_GAME;
    ship.backToHome();
    addScore(left * leftBonus, ship.pos);
    left = 0;
    credit = 0;
    letters.add("MISSION COMPLETED!", 110, 400, 12, 500, 1);
    SoundManager.playBgmOnce(Bgm.ENDING);
  }

  public void drawWarning() {
    letters.add("WARNING", 132, 210, 32, 250, 0);
    letters.add("HERE COMES A GIGANTIC TOY", 80, 280, 10, 250, -3);
  }

  public void setInGame() {
    state = GameState.IN_GAME;
  }

  public static string[] stageMessage =
    new string[] {
     "WE ARE TUMIKI FIGHTERS!",
     "JUST OVER THE HORIZON",
     "PANIC ON MEADOW",
     "COASTLINE UNDER FIRE",
     "JUNK CITY CENTRAL"
    };
  public void startStage() {
    fragments.clear();
    particles.clear();
    letters.clear();
    enemies.clear();
    bullets.clear();
    splinters.clear();
    ship.start();
    stageManager.start(stage);
    field.start(stage);
    gauge.init();
    bossTimer = BOSSTIMER_FREEZED;
    bossDstCnt = -1;
    letters.add("STAGE " + (stage + 1).ToString(), 180, 150, 24, 240, -4);
    letters.add(stageMessage[stage], 320 - stageMessage[stage].Length * 10, 270, 10, 240, -2);
    int si = stage % (SoundManager.STAGE_BGM_NUM * 2 - 1);
    if (si >= SoundManager.STAGE_BGM_NUM)
      si = SoundManager.STAGE_BGM_NUM * 2 - si - 2;
    SoundManager.playBgm(Bgm.STG1 + si);
  }

  public void startGameover() {
    state = GameState.GAMEOVER;
    setScreenShake(0, 0);
    letters.clear();
    cnt = 0;
    prefManager.setHiScore(score, stage);
    Music.fadeMusic();
  }

  public void startPause() {
    state = GameState.PAUSE;
    pauseCnt = 0;
  }

  public void resumePause() {
    state = GameState.IN_GAME;
  }

  public void addScore(int sc, Vector pos) {
    if (sc <= 0)
      return;
    score += sc;
    ScoreSign ss = (ScoreSign) signs.getInstanceForced();
    float s = 0.3f + (float) sc / 3000;
    if (s > 1.2f)
      s = 1.2f;
    ss.set(pos, sc, s);
    if (score > extendScore) {
      SoundManager.playSe(Se.EXTEND);
      left++;
      if (extendScore <= firstExtend)
	extendScore = everyExtend;
      else
	extendScore += everyExtend;
    }
  }

  public void shipDestroyed() {
    bullets.clearVisible();
    left--;
    if (left < 0)
      startGameover();
  }

  public int bossDestroyed() {
    Music.fadeMusic();
    bullets.clearVisible();
    ship.breakStuckEnemies();
    bullets.clear();
    if (bossDstCnt < 0)
      bossDstCnt = 0;

    int bs = ((int) bossTimer / 60) * 10 + 10000;
    if (bs > 20000)
      bs = 20000;
    else if (bs < 10000)
      bs = 10000;
    if (mode == GameMode.EXTRA)
      bs *= 3;
    return bs;
  }

  public void bossInAttack(int tm) {
    bossTimer = integer(tm * 16.66667f);
  }

  public void setRank(float r) {
    stageManager.setRank(r);
  }

  public void move() {
    if (pad.escape) {
      startTitle();
      return;
    }
    switch (state) {
    case GameState.START_GAME:
    case GameState.IN_GAME:
    case GameState.END_GAME:
      moveInGame();
      break;
    case GameState.GAMEOVER:
      moveGameover();
      break;
    case GameState.PAUSE:
      movePause();
      break;
    case GameState.TITLE:
      moveTitle();
      break;
    }
    cnt++;
  }

  public bool pPrsd = true;

  public void moveInGame() {
    if (bossDstCnt > STAGE_END_CNT - 120 && stage >= StageManager.STAGE_NUM - 1) {
      stage++;
      bossDstCnt = -1;
      bossTimer = BOSSTIMER_FREEZED;
      startEnding();
      return;
    }
    if (bossDstCnt > STAGE_END_CNT) {
      stage++;
      startStage();
      return;
    } else if (bossDstCnt == STAGE_END_CNT - 119) {
      SoundManager.playSe(Se.PROPELLER);
    }
    if (state == GameState.IN_GAME)
      stageManager.move();
    field.move();
    splinters.move();
    if (bossDstCnt > STAGE_END_CNT - 120) {
      bullets.clearVisible();
      bullets.clear();
      ship.endMove();
    } else if (state == GameState.IN_GAME) {
      ship.move();
    } else if (state == GameState.START_GAME) {
      ship.startMove();
    } else {
      ship.backToHomeMove();
    }
    BulletActor.resetTotalBulletsSpeed();
    bullets.move();
    Enemy.resetTotalNum();
    enemies.move();
    particles.move();
    fragments.move();
    signs.move();
    gauge.move();
    letters.move();
    moveScreenShake();
    Tumiki.move();
    if (bossDstCnt >= 0) {
      bossDstCnt++;
      ship.breakStuckEnemies();
    } else if (bossTimer < BOSSTIMER_FREEZED) {
      bossTimer -= 17;
      if (bossTimer < 0) {
	bullets.clearVisible();
	bullets.clear();
	ship.breakStuckEnemies();
	bossTimer = 0;
	bossDstCnt = 0;
	Music.fadeMusic();
      }
    }
    if (pad.pause) {
      if (!pPrsd) {
	pPrsd = true;
	if (state == GameState.IN_GAME)
	  startPause();
      }
    } else {
      pPrsd = false;
    }

  }

  public bool btnPrsd, arwPrsd;
  public bool isContinue;

  public void moveGameover() {
    bool gotoNextState = false;
    if (cnt == 48)
      letters.add("GAME OVER", 100, 230, 28, 400, 4);
    if (cnt <= 64) {
      arwPrsd = true; btnPrsd = arwPrsd;
      isContinue = false;
    } else {
      if ((pad.getButtonState() & (Pad.PAD_BUTTON1 | Pad.PAD_BUTTON2)) != 0) {
	if (!btnPrsd)
	  gotoNextState = true;
      } else {
	btnPrsd = false;
      }
      if (credit > 0) {
	int key = (pad.getPadState() & (Pad.PAD_LEFT | Pad.PAD_RIGHT));
	if (key != 0) {
	  if (!arwPrsd) {
	    arwPrsd = true;
	    if ((key & Pad.PAD_LEFT) != 0)
	      isContinue = true;
	    if ((key & Pad.PAD_RIGHT) != 0)
	      isContinue = false;
	  }
	} else {
	  arwPrsd = false;
	}
      }
    }
    if ((cnt > 64 && gotoNextState) || cnt > 700) {
      if (isContinue && cnt <= 700) {
	credit--;
	startInGame();
      } else {
	startTitle();
      }
    }
    field.move();
    bullets.move();
    enemies.move();
    particles.move();
    fragments.move();
    letters.move();
    Tumiki.move();
  }

  public void movePause() {
    pauseCnt++;
    if (pad.pause) {
      if (!pPrsd) {
	pPrsd = true;
	resumePause();
      }
    } else {
      pPrsd = false;
    }
  }

  public void moveTitle() {
    attractManager.moveTitle();
    field.move();
    Tumiki.move();
  }

  public void draw() {
    screen.clear();
    screen.viewOrthoFixed();
    glDisable(GL_CULL_FACE);
    field.drawBack();
    glEnable(GL_CULL_FACE);
    screen.viewPerspective();

    glPushMatrix();
    setEyepos();
    switch (state) {
    case GameState.START_GAME:
    case GameState.IN_GAME:
    case GameState.PAUSE:
    case GameState.END_GAME:
      drawInGame();
      break;
    case GameState.GAMEOVER:
      drawGameover();
      break;
    case GameState.TITLE:
      drawTitle();
      break;
    }
    glPopMatrix();

    screen.viewOrthoFixed();
    glDisable(GL_CULL_FACE);
    letters.draw();
    switch (state) {
    case GameState.IN_GAME:
      drawStatusInGame();
      break;
    case GameState.GAMEOVER:
    case GameState.END_GAME:
      drawStatusGameover();
      break;
    case GameState.PAUSE:
      drawStatusPause();
      break;
    case GameState.TITLE:
      drawStatusTitle();
      break;
    case GameState.START_GAME:
      break;
    }
    glEnable(GL_CULL_FACE);
    screen.viewPerspective();
  }

  public void drawInGame() {
    glEnable(GL_DEPTH_TEST);
    field.draw();
    enemies.draw();
    splinters.draw();
    if (state == GameState.START_GAME)
      ship.drawFriendly();
    else if (state == GameState.END_GAME)
      ship.drawFriendlyBack();
    ship.draw();
    glDisable(GL_DEPTH_TEST);
    glEnable(GL_BLEND);
    glBegin(GL_QUADS);
    particles.draw();
    glEnd();
    glDisable(GL_BLEND);
    fragments.draw();
    bullets.drawShots();
    signs.draw();
    glEnable(GL_DEPTH_TEST);
    gauge.draw();
    drawLeft();
    glDisable(GL_DEPTH_TEST);
    glLineWidth(2);
    bullets.drawBullets();
    glLineWidth(1);
  }

  public void drawGameover() {
    glEnable(GL_DEPTH_TEST);
    field.draw();
    enemies.draw();
    glDisable(GL_DEPTH_TEST);
    glEnable(GL_BLEND);
    glBegin(GL_QUADS);
    particles.draw();
    glEnd();
    glDisable(GL_BLEND);
    fragments.draw();
    bullets.drawShots();
  }

  public void drawTitle() {
    glEnable(GL_DEPTH_TEST);
    field.draw();
    glDisable(GL_DEPTH_TEST);
  }

  public void drawInfo() {
    LetterRender.drawString("SCORE", 4, 4, 12, LetterDirection.TO_RIGHT, -3);
    LetterRender.drawNum(score, 300, 4, 12, LetterDirection.TO_RIGHT, 3);
    if (bossTimer < BOSSTIMER_FREEZED && (bossDstCnt & 31) > 8) {
      LetterRender.drawTime(bossTimer, 600, 32, 10, 3);
    }
  }

  public void drawLeft() {
    float x = -field.size.x * 0.85f, sz = 0.4f;
    for (int i = 0; i < left; i++, x += 2) {
      glPushMatrix();
      glScalef(sz, sz, sz);
      ship.drawLeft(x / sz, -field.size.y * 0.82f / sz, 1 / sz);
      glPopMatrix();
    }
  }

  public void drawStatusInGame() {
    drawInfo();
  }

  public void drawStatusGameover() {
    drawInfo();
    if (credit > 0 && cnt > 64) {
      LetterRender.drawString("CONTINUE", 280, 400, 10, LetterDirection.TO_RIGHT, 1);
      if (isContinue) {
	LetterRender.drawString("YES", 480, 400, 12, LetterDirection.TO_RIGHT, 0);
	LetterRender.drawString("NO", 562, 402, 8, LetterDirection.TO_RIGHT, 2);
      } else {
	LetterRender.drawString("YES", 483, 402, 8, LetterDirection.TO_RIGHT, 2);
	LetterRender.drawString("NO", 560, 400, 12, LetterDirection.TO_RIGHT, 0);
      }
      LetterRender.drawString("CREDIT " + credit.ToString(), 32, 420, 8,
			      LetterDirection.TO_RIGHT, 3);
    }
  }

  public void drawStatusPause() {
    drawInfo();
    if ((pauseCnt % 60) < 30)
      LetterRender.drawString("PAUSE", 280, 220, 12, LetterDirection.TO_RIGHT, -5);
  }

  public void drawStatusTitle() {
    attractManager.drawTitle();
  }

  public int screenShakeCnt;
  public float screenShakeIntense;

  public void setScreenShake(int cnt, float intense) {
    screenShakeCnt = cnt;
    screenShakeIntense = intense;
  }

  public void moveScreenShake() {
    if (screenShakeCnt > 0)
      screenShakeCnt--;
  }

  public void setEyepos() {
    float x = 0, y = 0;
    if (screenShakeCnt > 0) {
      x = rand.nextSignedFloat(screenShakeIntense * (screenShakeCnt + 10));
      y = rand.nextSignedFloat(screenShakeIntense * (screenShakeCnt + 10));
    }
    glTranslatef(x, y, -field.eyeZ);
  }
}
  public static class GameState { public const int START_GAME = 0; public const int IN_GAME = 1; public const int GAMEOVER = 2; public const int PAUSE = 3; public const int TITLE = 4; public const int END_GAME = 5; }
  public static class GameMode { public const int NORMAL = 0; public const int EXTRA = 1; }
