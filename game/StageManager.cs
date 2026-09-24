// Copyright 2004 Kenta Cho. All rights reserved.
using System;
using static GameMath;
public class StageManager {

  public const int STAGE_NUM = 5;
  public float rank, speedRank;

  public const int APPEARANCE_MAX = 8;
  public GameManager manager;
  public EnemyPool enemies;
  public Field field;
  public StagePattern[] stage = new StagePattern[STAGE_NUM];
  public EnemyAppearancePattern[] current;
  public int patternIndex;
  public EnemyAppearancePattern nextApp;
  public int cnt;
  public EnemyAppearance[] appearance = new EnemyAppearance[APPEARANCE_MAX];
  public int eaIdx;
  public Rand rand;
  public int warningCnt;
  public bool bossComing;

  public StageManager(GameManager manager, EnemyPool enemies, Field field) {
    this.manager = manager;
    this.enemies = enemies;
    this.field = field;
    rand = new Rand();
    for (int _i = 0; _i < APPEARANCE_MAX; _i++) { appearance[_i] = new EnemyAppearance(); }
    eaIdx = appearance.Length;
    stage = GameData.stage;
  }

  public void start(int sn) {
    current = stage[sn].pattern;
    patternIndex = 0;
    nextApp = current[patternIndex];
    patternIndex++;
    rand.setSeed(stage[sn].randSeed);
    warningCnt = stage[sn].warningCnt;
    BulletInst.setRandSeed(stage[sn].randSeed);
    cnt = 0;
    bossComing = false;
    rank = 0;
    speedRank = 1;
    foreach (EnemyAppearance ea in appearance)
      ea.cnt = -1;
  }

  public void setAppearance(EnemyAppearancePattern eap, bool isBoss) {
    eaIdx--;
    if (eaIdx < 0)
      eaIdx = appearance.Length - 1;
    EnemyAppearance ea = appearance[eaIdx];
    ea.pattern = eap;
    ea.cnt = 0;
    ea.wait = eap.waitTillEnemiesDestroyed;
    ea.isBoss = isBoss;
  }

  public void move() {
    if (nextApp != null) {
      while (nextApp.startTime <= cnt) {
	setAppearance(nextApp, bossComing);
	if (bossComing)
	  bossComing = false;
	if (patternIndex >= current.Length) {
	  nextApp = null;
	  break;
	}
	nextApp = current[patternIndex];
    patternIndex++;
      }
    }
    cnt++;
    if (cnt == warningCnt) {
      manager.drawWarning();
      bossComing = true;
      Music.fadeMusic();
    }
    if (cnt >= warningCnt && cnt <= warningCnt + 200 && (cnt - warningCnt) % 100 == 0)
      SoundManager.playSe(Se.WARNING);
    if (cnt == warningCnt + 240) {
      if (manager.stage == STAGE_NUM - 1)
	SoundManager.playBgm(Bgm.LAST_BOSS);
      else
	SoundManager.playBgm(Bgm.BOSS);
    }
    foreach (EnemyAppearance ea in appearance) {
      if (ea.cnt < 0)
	continue;
      if (ea.wait && Enemy.totalNum <= 0)
	ea.wait = false;
      if ((ea.cnt % ea.pattern.interval) == 0) {
	EnemyAppearancePattern p = ea.pattern;
	float x = 0, y = 0;
	switch (p.posType) {
	case AppearancePos.FRONT:
	  x = field.size.x - p.spec.sizeXm * 1.1f;
	  y = field.size.y * (p.pos + rand.nextSignedFloat(p.width));
	  break;
	case AppearancePos.TOP:
	  x = field.size.x * (p.pos + rand.nextSignedFloat(p.width));
	  y = field.size.y - p.spec.sizeYm * 1.1f;
	  break;
	}
	if (!ea.wait)
	  addEnemy(x, y, p.spec, p.move, ea.isBoss);
      }
      ea.cnt++;
      if (ea.cnt >= ea.pattern.duration) {
	ea.cnt = -1;
	continue;
      }
    }
  }

  public void addEnemy(float x, float y, EnemySpec spec, EnemyMovePattern move, bool isBoss) {
    Enemy en = (Enemy) enemies.getInstance();
    if (en == null)
      return;
    en.set(x, y, spec, move, isBoss);
  }

  public void setRank(float r) {
    rank = r * 0.24f;
  }
}

public class EnemyAppearance {

  public EnemyAppearancePattern pattern;
  public int cnt;
  public bool wait;
  public bool isBoss;
}
