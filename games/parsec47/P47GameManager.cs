// Copyright 2003 Kenta Cho. All rights reserved.
using System;
using static GameMath;
using static Drawing;
public class P47GameManager {

  public bool nowait = false;
  public int difficulty, parsecSlot;
    public const int ROLL = 0;
  public const int LOCK = 1;
  public int mode;
    public const int TITLE_STATE = 0;
  public const int IN_GAME = 1;
  public const int GAMEOVER = 2;
  public const int PAUSE = 3;
  public int state;

  public Pad pad;
  public const int ENEMY_MAX = 32;
  public P47PrefManager prefManager;
  public P47Screen screen;
  public P47Rand rand;
  public Field field;
  public Ship ship;
  public ActorPool enemies;
  public LuminousActorPool particles;
  public LuminousActorPool fragments;
  public BulletActorPool bullets;
  public ActorPool shots;
  public ActorPool rolls;
  public ActorPool targetLocks;
  public ActorPool bonuses;
  public BarrageManager barrageManager;
  public StageManager stageManager;
  public const int FIRST_EXTEND = 200000;
  public const int EVERY_EXTEND = 500000;
  public const int LEFT_MAX = 4;
  public int left;
  public int score, extendScore;
  public int cnt;
  public int pauseCnt;
  public const int BOSS_WING_NUM = 4;
  public int bossShield;
  public int[] bossWingShield = new int[BOSS_WING_NUM];
  public static float[] SLOWDOWN_START_BULLETS_SPEED = new float[] { 30, 42 };
  public float interval;
  public Title title;

  public void init() {
    pad = new Pad();
    prefManager = new P47PrefManager();
    screen = new P47Screen();
    rand = new P47Rand();
    Field.createDisplayLists();
    field = new Field();
    field.init();
    Ship.createDisplayLists();
    ship = new Ship();
    ship.init(pad, field, this);
    Particle particleClass = new Particle();
    ParticleInitializer pi = new ParticleInitializer();
    particles = new LuminousActorPool(128, particleClass, pi);
    Fragment fragmentClass = new Fragment();
    FragmentInitializer fi = new FragmentInitializer();
    fragments = new LuminousActorPool(128, fragmentClass, fi);
    BulletActor.createDisplayLists();
    BulletActorInitializer bi = new BulletActorInitializer(field, ship);
    bullets = new BulletActorPool(512, bi);
    LetterRender.createDisplayLists();
    Shot shotClass = new Shot();
    ShotInitializer shi = new ShotInitializer(field);
    shots = new ActorPool(32, shotClass, shi);
    Roll rollClass = new Roll();
    RollInitializer ri = new RollInitializer(ship, field, this);
    rolls = new ActorPool(4, rollClass, ri);
    Lock.init_0();
    Lock targetLockClass = new Lock();
    LockInitializer li = new LockInitializer(ship, field, this);
    targetLocks = new ActorPool(4, targetLockClass, li);
    Enemy enemyClass = new Enemy();
    EnemyInitializer ei = new EnemyInitializer
      (field, bullets, shots, rolls, targetLocks, ship, this);
    enemies = new ActorPool(ENEMY_MAX, enemyClass, ei);
    Bonus.init_0();
    Bonus bonusClass = new Bonus();
    BonusInitializer bni = new BonusInitializer(field, ship, this);
    bonuses = new ActorPool(128, bonusClass, bni);
    barrageManager = new BarrageManager();
    barrageManager.loadBulletMLs();
    EnemyType.init(barrageManager);
    stageManager = new StageManager();
    stageManager.init(this, barrageManager, field);
    title = new Title();
    title.init(pad, this, prefManager, field);
    interval = 16;
    SoundManager.init(this);
  }

  public void start() {
    startTitle();
  }

  public void close() {
    barrageManager.unloadBulletMLs();
    title.close();
    SoundManager.close();
    LetterRender.deleteDisplayLists();
    Field.deleteDisplayLists();
    Ship.deleteDisplayLists();
    BulletActor.deleteDisplayLists();
  }

  public void addScore(int sc) {
    score += sc;
    if (score > extendScore) {
      if (left < LEFT_MAX) {
	SoundManager.playSe(SoundManager.EXTEND);
	left++;
      }
      if (extendScore <= FIRST_EXTEND)
	extendScore = EVERY_EXTEND;
      else
	extendScore += EVERY_EXTEND;
    }
  }

  public void shipDestroyed() {
    if (mode == ROLL)
      releaseRoll();
    else
      releaseLock();
    clearBullets();
    left--;
    if (left < 0)
      startGameover();
  }

  public void addParticle(Vector pos, float deg, float ofs, float speed) {
    Particle pt = (Particle) particles.getInstanceForced();
    pt.set(pos, deg, ofs, speed);
  }

  public void addFragments(int n, float x1, float y1, float x2, float y2, float z,
			   float speed, float deg) {
    for (int index0 = 0; index0 < n; index0++) {
      Fragment ft = (Fragment) fragments.getInstanceForced();
      ft.set(x1, y1, x2, y2, z, speed, deg);
    }
  }

  public void addEnemy(Vector pos, float d, EnemyType type, int moveParser) {
    Enemy en = (Enemy) enemies.getInstance();
    if (en == null)
      return;
    en.set(pos, d, type, moveParser);
  }

  public void clearBullets() {
    for (int index1 = 0; index1 < bullets.actor.Length; index1++) {
      if (!bullets.actor[index1].isExist)
	continue;
      ((BulletActor) bullets.actor[index1]).toRetro();
    }
  }

  public void addBoss(Vector pos, float d, EnemyType type) {
    Enemy en = (Enemy) enemies.getInstance();
    if (en == null)
      return;
    en.setBoss(pos, d, type);
  }

  public void addShot(Vector pos, float deg) {
    Shot shot = (Shot) shots.getInstance();
    if (shot == null)
      return;
    shot.set(pos, deg);
  }

  public void addRoll() {
    Roll roll = (Roll) rolls.getInstance();
    if (roll == null)
      return;
    roll.set();
  }

  public void addLock() {
    Lock targetLock = (Lock) targetLocks.getInstance();
    if (targetLock == null)
      return;
    targetLock.set();
  }

  public void releaseRoll() {
    for (int index2 = 0; index2 < rolls.actor.Length; index2++) {
      if (!rolls.actor[index2].isExist)
	continue;
      ((Roll) rolls.actor[index2]).released = true;
    }
  }

  public void releaseLock() {
    for (int index3 = 0; index3 < targetLocks.actor.Length; index3++) {
      if (!targetLocks.actor[index3].isExist)
	continue;
      ((Lock) targetLocks.actor[index3]).released = true;
    }
  }

  public void addBonus(Vector pos, Vector ofs, int num) {
    for (int index4 = 0; index4 < num; index4++) {
      Bonus bonus = (Bonus) bonuses.getInstance();
      if (bonus == null)
	return;
      bonus.set(pos, ofs);
    }
  }

  public void setBossShieldMeter(int bs, int s1, int s2, int s3, int s4, float r) {
    r *= 0.7f;
    bossShield = GameMath.integer(bs * r);
    bossWingShield[0] = GameMath.integer(s1 * r) ;
    bossWingShield[1] = GameMath.integer(s2 * r);
    bossWingShield[2] = GameMath.integer(s3 * r);
    bossWingShield[3] = GameMath.integer(s4 * r);
  }

    public const int PRACTICE = 0;
  public const int NORMAL = 1;
  public const int HARD = 2;
  public const int EXTREME = 3;
  public const int QUIT = 4;

  public void startStage(int difficulty, int parsecSlot, int startParsec, int mode) {
    enemies.clear();
    bullets.clear();
    this.difficulty = difficulty;
    this.parsecSlot = parsecSlot;
    this.mode = mode;
    int stageType = rand.nextInt(99999);
    switch (difficulty) {
    case PRACTICE:
      stageManager.setRank(1, 4, startParsec, stageType);
      ship.setSpeedRate(0.7f);
      Bonus.setSpeedRate(0.6f);
      break;
    case NORMAL:
      stageManager.setRank(10, 8, startParsec, stageType);
      ship.setSpeedRate(0.9f);
      Bonus.setSpeedRate(0.8f);
      break;
    case HARD:
      stageManager.setRank(22, 12, startParsec, stageType);
      ship.setSpeedRate(1);
      Bonus.setSpeedRate(1);
      break;
    case EXTREME:
      stageManager.setRank(36, 16, startParsec, stageType);
      ship.setSpeedRate(1.2f);
      Bonus.setSpeedRate(1.3f);
      break;
    case QUIT:
      stageManager.setRank(0, 0, 0, 0);
      ship.setSpeedRate(1);
      Bonus.setSpeedRate(1);
      break;
    }
  }

  public void initShipState() {
    left = 2;
    score = 0;
    extendScore = FIRST_EXTEND;
    ship.start();
  }

  public void startInGame() {
    state = IN_GAME;
    initShipState();
    startStage(difficulty, parsecSlot, title.getStartParsec(difficulty, parsecSlot), mode);
  }

  public void startTitle() {
    state = TITLE_STATE;
    title.start();
    initShipState();
    bullets.clear();
    ship.cnt = 0;
    startStage(difficulty, parsecSlot, title.getStartParsec(difficulty, parsecSlot), mode);
    cnt = 0;
    Sound.stopMusic();
  }

  public void startGameover() {
    state = GAMEOVER;
    bonuses.clear();
    shots.clear();
    rolls.clear();
    targetLocks.clear();
    setScreenShake(0, 0);
    interval = 16;
    cnt = 0;
    if (score > prefManager.hiScore[mode][difficulty][parsecSlot])
      prefManager.hiScore[mode][difficulty][parsecSlot] = score;
    if (stageManager.parsec > prefManager.reachedParsec[mode][difficulty])
      prefManager.reachedParsec[mode][difficulty] = stageManager.parsec;
    Sound.fadeMusic();
    prefManager.save();
  }

  public void startPause() {
    state = PAUSE;
    pauseCnt = 0;
  }

  public void resumePause() {
    state = IN_GAME;
  }

  public void stageMove() {
    stageManager.move();
  }

  public bool pPrsd = true;

  public void inGameMove() {
    stageMove();
    field.move();
    ship.move();
    bonuses.move();
    shots.move();
    enemies.move();
    if (mode == ROLL)
      rolls.move();
    else
      targetLocks.move();
    BulletActor.resetTotalBulletsSpeed();
    bullets.move();
    particles.move();
    fragments.move();
    moveScreenShake();
    if (pad.pause) {
      if (!pPrsd) {
	pPrsd = true;
	startPause();
      }
    } else {
      pPrsd = false;
    }
    if (!nowait) {

      if (BulletActor.totalBulletsSpeed > SLOWDOWN_START_BULLETS_SPEED[mode]) {
	float sm = BulletActor.totalBulletsSpeed / SLOWDOWN_START_BULLETS_SPEED[mode];
	if (sm > 1.75f)
	  sm = 1.75f;
	interval += (sm * 16 - interval) * 0.1f;

      } else {
	interval += (16 - interval) * 0.08f;

      }
    }
  }

  public bool btnPrsd = true;

  public void titleMove() {
    title.move();
    if (cnt <= 8) {
      btnPrsd = true;
    } else {
      int btn = pad.getButtonState();
      if ((btn & Pad.PAD_BUTTON1) != 0) {
	if (!btnPrsd) {
	  title.setStatus();
	  if (difficulty >= P47PrefManager.DIFFICULTY_NUM)
	    startTitle();
	  else
	    startInGame();
	  return;
	}
      } else if ((btn & Pad.PAD_BUTTON2) != 0) {
	if (!btnPrsd) {
	  title.changeMode();
	  btnPrsd = true;
	}
      } else {
	btnPrsd = false;
      }
    }
    stageMove();
    field.move();
    enemies.move();
    bullets.move();
  }

  public void gameoverMove() {
    bool gotoNextState = false;
    if (cnt <= 64) {
      btnPrsd = true;
    } else {
      if ((pad.getButtonState() & (Pad.PAD_BUTTON1 | Pad.PAD_BUTTON2)) != 0) {
	if (!btnPrsd)
	  gotoNextState = true;
      } else {
	btnPrsd = false;
      }
    }
    if (cnt > 64 && gotoNextState) {
	startTitle();
    } else if (cnt > 500) {
	startTitle();
    }
    field.move();
    enemies.move();
    bullets.move();
    particles.move();
    fragments.move();
  }

  public void pauseMove() {
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

  public void move() {
    if (pad.escape) {
      startTitle();
      return;
    }
    switch (state) {
    case IN_GAME:
      inGameMove();
      break;
    case TITLE_STATE:
      titleMove();
      break;
    case GAMEOVER:
      gameoverMove();
      break;
    case PAUSE:
      pauseMove();
      break;
    default: break;
    }
    cnt++;
  }

  public void inGameDraw() {
    field.draw();
    P47Screen.setRetroColor(0.2f, 0.7f, 0.5f, 1);
    glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
    bonuses.draw();
    glBlendFunc(GL_SRC_ALPHA, GL_ONE);
    Screen.setColorAlpha(Particle.R, Particle.G, Particle.B, 1);
    glBegin(GL_LINES);
    particles.draw();
    glEnd();
    P47Screen.setRetroColor(Fragment.R, Fragment.G, Fragment.B, 1);
    fragments.draw();
    P47Screen.setRetroZ(0);
    ship.draw();
    P47Screen.setRetroColor(0.8f, 0.8f, 0.2f, 0.8f);
    shots.draw();
    P47Screen.setRetroColor(1.0f, 0.8f, 0.5f, 1);
    if (mode == ROLL)
      rolls.draw();
    else
      targetLocks.draw();
    enemies.draw();
    bullets.draw();
  }

  public void titleDraw() {
    field.draw();
    enemies.draw();
    bullets.draw();
  }

  public void gameoverDraw() {
    field.draw();
    Screen.setColorAlpha(Particle.R, Particle.G, Particle.B, 1);
    glBegin(GL_LINES);
    particles.draw();
    glEnd();
    P47Screen.setRetroColor(Fragment.R, Fragment.G, Fragment.B, 1);
    fragments.draw();
    P47Screen.setRetroZ(0);
    enemies.draw();
    bullets.draw();
  }

  public void inGameDrawLuminous() {
    glBegin(GL_LINES);
    particles.drawLuminous();
    fragments.drawLuminous();
    glEnd();
  }

  public void titleDrawLuminous() {
  }

  public void gameoverDrawLuminous() {
    glBegin(GL_LINES);
    particles.drawLuminous();
    fragments.drawLuminous();
    glEnd();
  }

  public void drawBoard(int x, int y, int width, int height) {
    Drawing.Color(0, 0, 0, 1);
    glBegin(GL_QUADS);
    glVertex2f(x, y);
    glVertex2f(x + width, y);
    glVertex2f(x + width, y + height);
    glVertex2f(x, y + height);
    glEnd();
  }

  public void drawSideBoards() {
    glDisable(GL_BLEND);
    drawBoard(0, 0, 160, 480);
    drawBoard(480, 0, 160, 480);
    glEnable(GL_BLEND);
  }

  public void drawScore() {
    LetterRender.drawNum(score, 120, 28, 25, LetterRender.TO_UP);
    LetterRender.drawNum(Bonus.bonusScore, 24, 20, 12, LetterRender.TO_UP);
  }

  public void drawLeft() {
    if (left < 0)
      return;
    LetterRender.drawString("LEFT", 520, 260, 25, LetterRender.TO_DOWN);
    LetterRender.changeColor(LetterRender.RED);
    LetterRender.drawNum(left, 520, 450, 25, LetterRender.TO_DOWN);
    LetterRender.changeColor(LetterRender.WHITE);
  }

  public void drawParsec() {
    int ps = stageManager.parsec;
    if (ps < 10)
      LetterRender.drawNum(stageManager.parsec, 600, 26, 25, LetterRender.TO_DOWN);
    else if (ps < 100)
      LetterRender.drawNum(stageManager.parsec, 600, 68, 25, LetterRender.TO_DOWN);
    else
      LetterRender.drawNum(stageManager.parsec, 600, 110, 25, LetterRender.TO_DOWN);
  }

  public void drawBox(int x, int y, int w, int h) {
    if (w <= 0)
      return;
    Screen.setColorAlpha(1, 1, 1, 0.5f);
    P47Screen.drawBoxSolid(x, y, w, h);
    Screen.setColorAlpha(1, 1, 1, 1);
    P47Screen.drawBoxLine(x, y, w, h);
  }

  public void drawBossShieldMeter() {
    drawBox(165, 6, bossShield, 6);
    int y = 24;
    for (int index5 = 0; index5 < BOSS_WING_NUM; index5++) {
      switch (index5 % 2) {
      case 0:
	drawBox(165, y, bossWingShield[index5], 6);
	break;
      case 1:
	drawBox(475 - bossWingShield[index5], y, bossWingShield[index5], 6);
	y += 12;
	break;
      }
    }
  }

  public void drawSideInfo() {
    drawSideBoards();
    drawScore();
    drawLeft();
    drawParsec();
  }

  public void inGameDrawStatus() {
    drawSideInfo();
    if (stageManager.bossSection)
      drawBossShieldMeter();
  }

  public void titleDrawStatus() {
    drawSideBoards();
    drawScore();
    title.draw();
  }

  public void gameoverDrawStatus() {
    drawSideInfo();
    if (cnt > 64) {
      LetterRender.drawString("GAME OVER", 220, 200, 15, LetterRender.TO_RIGHT);
    }
  }

  public void pauseDrawStatus() {
    drawSideInfo();
    if ((pauseCnt % 60) < 30)
      LetterRender.drawString("PAUSE", 280, 220, 12, LetterRender.TO_RIGHT);
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

  public void draw() {
    screen.clear();
    glPushMatrix();
    setEyepos();
    switch (state) {
    case IN_GAME:
    case PAUSE:
      inGameDraw();
      break;
    case TITLE_STATE:
      titleDraw();
      break;
    case GAMEOVER:
      gameoverDraw();
      break;
    default: break;
    }
    glPopMatrix();

    screen.viewOrthoFixed();
    switch (state) {
    case IN_GAME:
      inGameDrawStatus();
      break;
    case TITLE_STATE:
      titleDrawStatus();
      break;
    case GAMEOVER:
      gameoverDrawStatus();
      break;
    case PAUSE:
      pauseDrawStatus();
      break;
    default: break;
    }
    screen.viewPerspective();
  }
}
