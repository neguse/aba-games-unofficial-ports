// Copyright 2004 Kenta Cho. All rights reserved.
using System;
using static GameMath;
using static Lub;
public class TumikiSet {
  public Tumiki[] tumiki;
  public int score, fireScore, fireScoreInterval;
  public float sizeXm, sizeXp, sizeYm, sizeYp, size;
  public TumikiSet(int score, int fireScore, int fireScoreInterval, Tumiki[] tumiki) {
    this.score = score; this.fireScore = fireScore; this.fireScoreInterval = fireScoreInterval; this.tumiki = tumiki;
    sizeYm = float.MaxValue; sizeXm = sizeYm; sizeYp = 1.17549435e-38f; sizeXp = sizeYp;
    foreach (Tumiki t in tumiki) {
      sizeXm = Math.Min(sizeXm, t.ofs.x - t.size.x); sizeXp = Math.Max(sizeXp, t.ofs.x + t.size.x);
      sizeYm = Math.Min(sizeYm, t.ofs.y - t.size.y); sizeYp = Math.Max(sizeYp, t.ofs.y + t.size.y);
    }
    size = -sizeXm + sizeXp - sizeYm + sizeYp;
  }
  public static TumikiSet getInstance(string name) { return GameData.Find_tumiki(name); }
public int addTopBullets(int barragePtnIdx, BulletActorPool bullets, EnemyTopBullet[] etb,
			   BulletTarget target, int type) {
    int etbIdx = 0;
    foreach (Tumiki t in tumiki) {
      BulletActor ba = t.addTopBullet(barragePtnIdx, bullets, target, type);
      if (ba != null) {
	etb[etbIdx].actor = ba;
	etb[etbIdx].tumiki = t;
	etb[etbIdx].deactivated = false;
	etbIdx++;
      }
    }
    return etbIdx;
  }

  public void breakIntoFragmentsAt(ActorPool fragments, float x, float y, float d) {
    foreach (Tumiki t in tumiki) {
      float ox = t.ofs.x * cos(d) - t.ofs.y * sin(d);
      float oy = t.ofs.x * sin(d) + t.ofs.y * cos(d);
      Fragment fr = (Fragment) fragments.getInstanceForced();
      fr.set(t.shape, t.color, x + ox, y + oy, t.size);
    }
  }

  public void breakIntoFragments(ActorPool fragments, Vector pos, float d) {
    breakIntoFragmentsAt(fragments, pos.x, pos.y, d);
  }

  public void drawRotated(float[] model, float[] tint, Gfx.Blend blend, bool depth, Gfx.Cull cull, float width, Vector pos, float z, float deg) {
    foreach (Tumiki t in tumiki)
      t.drawRotated(model, tint, blend, depth, cull, width, pos, z, 0, deg);
  }

  public void drawShadeRotated(float[] model, float[] tint, Gfx.Blend blend, bool depth, Gfx.Cull cull, float width, Vector pos, float z, int shade, float deg) {
    foreach (Tumiki t in tumiki)
      t.drawRotated(model, tint, blend, depth, cull, width, pos, z, shade, deg);
  }

  public void drawShadeScaled(float[] model, float[] tint, Gfx.Blend blend, bool depth, Gfx.Cull cull, float width, Vector pos, float z, int shade, float deg, float size) {
    foreach (Tumiki t in tumiki)
      t.drawScaled(model, tint, blend, depth, cull, width, pos, z, shade, deg, size);
  }

  public void draw(float[] model, float[] tint, Gfx.Blend blend, bool depth, Gfx.Cull cull, float width, Vector pos, float z) {
    foreach (Tumiki t in tumiki)
      t.draw(model, tint, blend, depth, cull, width, pos, z, 0);
  }

  public void drawShade(float[] model, float[] tint, Gfx.Blend blend, bool depth, Gfx.Cull cull, float width, Vector pos, float z, int shade) {
    foreach (Tumiki t in tumiki)
      t.draw(model, tint, blend, depth, cull, width, pos, z, shade);
  }

  public void drawAt(float[] model, float[] tint, Gfx.Blend blend, bool depth, Gfx.Cull cull, float width, float x, float y, float z, bool damaged, bool wounded) {
    foreach (Tumiki t in tumiki)
      t.drawAt(model, tint, blend, depth, cull, width, x, y, z, 0, damaged, wounded);
  }

  public bool checkHit(Vector p, float x, float y) {
    foreach (Tumiki t in tumiki)
      if (t.checkHit(p, x, y))
	return true;
    return false;
  }
}
