// Copyright 2004 Kenta Cho. All rights reserved.
using System;
using static GameMath;
using static Drawing;
public class BulletActorPool: ActorPool {

  public int cnt;

  public BulletActorPool(int n, ActorInitializer ini) : base(n, new BulletActor(), ini) {
    BulletInst.world = new BulletWorld(this);
    BulletActor.resetIds();
    cnt = 0;
  }

  public void setEnemies(EnemyPool enemies) {
    foreach (Actor a in actor)
      ((BulletActor) a).setEnemies(enemies);
  }

  public void setStageManager(StageManager stageManager) {
    foreach (Actor a in actor)
      ((BulletActor) a).setStageManager(stageManager);
  }

  public void addBullet(float deg, float speed) {
    BulletActor ba = (BulletActor) getInstance();
    if (ba == null)
      return;
    BulletInst rb = (BulletInst) BulletInst.now;
    if (rb.deactivated)
      return;
    int nmi = rb.morphIdx + 1;
    if (nmi < rb.morphNum) {
      PatternState[] runner = BulletInst.createRunner(rb.parser[nmi]);
        ba.setPattern(runner, BulletInst.now.pos.x, BulletInst.now.pos.y, deg, speed,
	     rb.ranks[nmi], rb.speeds[nmi],
	     rb.shape, rb.color, rb.bulletSize, rb.xReverse, rb.yReverse, rb.target, rb.type,
	     rb.parser, rb.ranks, rb.speeds, rb.morphNum, nmi);
      ba.setMorphSeed();
    } else {
      nmi--;
      ba.set(BulletInst.now.pos.x, BulletInst.now.pos.y, deg, speed,
	     rb.ranks[nmi], rb.speeds[nmi],
	     rb.shape, rb.color, rb.bulletSize, rb.xReverse, rb.yReverse, rb.target, rb.type);
    }
  }

  public void addScriptBullet(PatternState state, float deg, float speed) {
    BulletActor ba = (BulletActor) getInstance();
    if (ba == null)
      return;
    BulletInst rb = (BulletInst) BulletInst.now;
    if (rb.deactivated)
      return;
    PatternState[] runner = new PatternState[] {state};
    ba.setPattern(runner, BulletInst.now.pos.x, BulletInst.now.pos.y, deg, speed,
	   rb.ranks[rb.morphIdx], rb.speeds[rb.morphIdx],
	   rb.shape, rb.color, rb.bulletSize, rb.xReverse, rb.yReverse, rb.target, rb.type,
	   rb.parser, rb.ranks, rb.speeds, rb.morphNum, rb.morphIdx);
  }

  public BulletActor addTopBullet(int[] parser,
				  float[] ranks, float[] speeds,
				  float x, float y, float deg, float speed,
				  int shape, int color, float size,
				  float xReverse, float yReverse,
				  BulletTarget target, int type,
				  int prevWait, int postWait) {
    PatternState[] runner = BulletInst.createRunner(parser[0]);
    BulletActor ba = (BulletActor) getInstance();
    if (ba == null)
      return null;
    ba.setPattern(runner, x, y, deg, speed,
	   ranks[0], speeds[0],
	   shape, color, size, xReverse, yReverse, target, type,
	   parser, ranks, speeds, parser.Length, 0);
    ba.setWait(prevWait, postWait);
    ba.setTop();
    return ba;
  }

  public BulletActor addMoveBullet(int parser, float speed,
				   float x, float y, float deg, BulletTarget target) {
    PatternState[] runner = BulletInst.createRunner(parser);
    BulletActor ba = (BulletActor) getInstance();
    if (ba == null)
      return null;
    ba.setPattern(runner, x, y, deg, 0,
	   0, speed,
	   0, 0, 0, 1, 1, target, BulletType.MOVE,
	   null, null, null, 0, 0);
    ba.setInvisible();
    return ba;
  }

  public override void move() {
    base.move();
    cnt++;
  }

  public void drawShots() {
    foreach (Actor ac in actor)
      if (ac.isExist) {
	BulletActor ba = (BulletActor) ac;
	if (ba.bullet.type == BulletType.SHIP)
	  ac.draw();
      }
  }

  public void drawBullets() {
    foreach (Actor ac in actor)
      if (ac.isExist) {
	BulletActor ba = (BulletActor) ac;
	if (ba.bullet.type == BulletType.ENEMY)
	  ac.draw();
      }
  }

  public int getTurn() {
    return cnt;
  }

  public void killMe(BulletInst bullet) {
    bullet.owner.remove();
  }

  public override void clear() {
    foreach (Actor ac in actor) {
      if (ac.isExist)
	((BulletActor) ac).removeForced();
    }
  }

  public void clearVisible() {
    foreach (Actor ac in actor) {
      if (ac.isExist)
	((BulletActor) ac).removeForcedVisible();
    }
  }

  public void clearVisibleEnemy() {
    foreach (Actor ac in actor) {
      if (ac.isExist)
	((BulletActor) ac).removeForcedVisibleEnemy();
    }
  }

  public void clearStuckEnemyHit(StuckEnemy se) {
    se.setWideCollision();
    foreach (Actor ac in actor) {
      if (ac.isExist) {
	BulletActor ba = (BulletActor) ac;
	if (se.checkHit(ba.bullet.pos)) {
	  ba.removeForcedVisible();
	}
      }
    }
  }

}
