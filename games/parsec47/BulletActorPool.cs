// Copyright 2003 Kenta Cho. All rights reserved.
using System;
using static GameMath;
using static Drawing;
public class BulletActorPool: ActorPool {

  public int cnt;

  public BulletActorPool(int n, ActorInitializer ini) : base(n, new BulletActor(), ini) {
    P47Bullet.world = new P47World(this);
    BulletActor.init_0();
    cnt = 0;
  }

  public void addBullet_2(float deg, float speed) {
    BulletActor ba = (BulletActor) getInstance();
    if (ba == null)
      return;
    P47Bullet rb = (P47Bullet) P47Bullet.now;
    if (rb.isMorph) {
      PatternState[] runner = P47Bullet.createRunner(rb.morphParser[rb.morphIdx]);
      ba.set_15(runner, P47Bullet.now.pos.x, P47Bullet.now.pos.y, deg, speed,
	     P47Bullet.now.rankNum,
	     rb.speedRank, rb.shape, rb.color, rb.bulletSize, rb.xReverse,
	     rb.morphParser, rb.morphNum, rb.morphIdx + 1, rb.morphCnt - 1);
    } else {
      ba.set_10(P47Bullet.now.pos.x, P47Bullet.now.pos.y, deg, speed,
	     P47Bullet.now.rankNum,
	     rb.speedRank, rb.shape, rb.color, rb.bulletSize, rb.xReverse);
    }
  }

  public void addScriptBullet(PatternState state, float deg, float speed) {
    BulletActor ba = (BulletActor) getInstance();
    if (ba == null)
      return;
    PatternState[] runner = new PatternState[] {state};
    P47Bullet rb = (P47Bullet) P47Bullet.now;
    if (rb.isMorph)
      ba.set_15(runner, P47Bullet.now.pos.x, P47Bullet.now.pos.y, deg, speed,
	     P47Bullet.now.rankNum,
	     rb.speedRank, rb.shape, rb.color, rb.bulletSize, rb.xReverse,
	     rb.morphParser, rb.morphNum, rb.morphIdx, rb.morphCnt);
    else
      ba.set_11(runner, P47Bullet.now.pos.x, P47Bullet.now.pos.y, deg, speed,
	     P47Bullet.now.rankNum,
	     rb.speedRank, rb.shape, rb.color, rb.bulletSize, rb.xReverse);
  }

  public BulletActor addBullet_11(PatternState[] runner,
			       float x, float y, float deg, float speed,
			       float rank,
			       float speedRank, int shape, int color, float size, float xReverse) {
    BulletActor ba = (BulletActor) getInstance();
    if (ba == null)
      return null;
    ba.set_11(runner, x, y, deg, speed, rank, speedRank, shape, color, size, xReverse);
    ba.setInvisible();
    return ba;
  }

  public BulletActor addBullet_12(int parser,
			       PatternState[] runner,
			       float x, float y, float deg, float speed,
			       float rank,
			       float speedRank, int shape, int color, float size, float xReverse) {
    BulletActor ba =
      addBullet_11(runner, x, y, deg, speed, rank, speedRank, shape, color, size, xReverse);
    if (ba == null)
      return null;
    ba.setTop(parser);
    return ba;
  }

  public BulletActor addBullet_15(int parser,
			       PatternState[] runner,
			       float x, float y, float deg, float speed,
			       float rank,
			       float speedRank, int shape, int color, float size, float xReverse,
			       int[] morph, int morphNum, int morphCnt) {
    BulletActor ba = (BulletActor) getInstance();
    if (ba == null)
      return null;
    ba.set_15(runner, x, y, deg, speed, rank,
	   speedRank, shape, color, size, xReverse,
	   morph, morphNum, 0, morphCnt);
    ba.setTop(parser);
    return ba;
  }

  public override void move() {
    base.move();
    cnt++;
  }

  public int getTurn() {
    return cnt;
  }

  public void killMe(P47Bullet bullet) {
    bullet.owner.remove();
  }

  public override void clear() {
    for (int index0 = 0; index0 < actor.Length; index0++) {
      if (actor[index0].isExist)
	((BulletActor) actor[index0]).remove();
    }
  }

}
