// Copyright 2005 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Drawing;

public class Shot : Actor
{
    public const float SPEED = 0.6f;
    public const float LANCE_SPEED = 0.5f;
    public static ShotShape shape;
    public static LanceShape lanceShape;
    public static GunroarRand rand;
    public Field field;
    public EnemyPool enemies;
    public SparkPool sparks;
    public SmokePool smokes;
    public BulletPool bullets;
    public Vector pos;
    public int cnt;
    public int hitCnt;
    public float _deg;
    public int _damage;
    public bool lance;
    public static void init_0()
    {
        shape = new ShotShape();
        lanceShape = new LanceShape();
        rand = new GunroarRand();
    }

    public static void setRandSeed(int seed)
    {
        rand.setSeed(seed);
    }

    public static void close()
    {
        shape.close();
    }

    public Shot()
    {
        pos = new Vector();
        {
            hitCnt = 0;
            cnt = hitCnt;
        }

        _deg = 0;
        _damage = 1;
        lance = false;
    }

    public override void init(List<object> args)
    {
        field = (Field)args[0];
        enemies = (EnemyPool)args[1];
        sparks = (SparkPool)args[2];
        smokes = (SmokePool)args[3];
        bullets = (BulletPool)args[4];
    }

    public void set(Vector p, float d, bool lance = false, int dmg = -1)
    {
        pos.x = p.x;
        pos.y = p.y;
        {
            hitCnt = 0;
            cnt = hitCnt;
        }

        _deg = d;
        this.lance = lance;
        if (lance)
            _damage = 10;
        else
            _damage = 1;
        if (dmg >= 0)
            _damage = dmg;
        exists = true;
    }

    public override void move()
    {
        cnt++;
        if (hitCnt > 0)
        {
            hitCnt++;
            if (hitCnt > 30)
                remove();
            return;
        }

        float sp = 0;
        if (!lance)
        {
            sp = SPEED;
        }
        else
        {
            if (cnt < 10)
                sp = LANCE_SPEED * cnt / 10;
            else
                sp = LANCE_SPEED;
        }

        pos.x = pos.x + (sin(_deg) * sp);
        pos.y = pos.y + (cos(_deg) * sp);
        pos.y = pos.y - (field.lastScrollY);
        if (((field.getBlock_1(pos) >= Field.ON_BLOCK_THRESHOLD) || (!field.checkInOuterField_1(pos))) || (pos.y > field.size.y))
            remove();
        if (lance)
        {
            enemies.checkShotHit(pos, lanceShape, this);
        }
        else
        {
            bullets.checkShotHit(pos, shape, this);
            enemies.checkShotHit(pos, shape, this);
        }
    }

    public void remove()
    {
        if (lance && (hitCnt <= 0))
        {
            hitCnt = 1;
            return;
        }

        exists = false;
    }

    public void removeHitToBullet()
    {
        removeHit();
    }

    public void removeHitToEnemy(bool isSmallEnemy = false)
    {
        if (isSmallEnemy && lance)
            return;
        SoundManager.playSe("hit.wav");
        removeHit();
    }

    public void removeHit()
    {
        remove();
        int sn = 0;
        if (lance)
        {
            for (int i = 0; i < 10; i++)
            {
                Smoke s = smokes.getInstanceForced();
                float d = _deg + rand.nextSignedFloat(0.1f);
                float sp = rand.nextFloat(LANCE_SPEED);
                s.set_7(pos, sin(d) * sp, cos(d) * sp, 0, SmokeSmokeType.LANCE_SPARK, 30 + rand.nextInt(30), 1);
                s = smokes.getInstanceForced();
                d = _deg + rand.nextSignedFloat(0.1f);
                sp = rand.nextFloat(LANCE_SPEED);
                s.set_7(pos, -sin(d) * sp, -cos(d) * sp, 0, SmokeSmokeType.LANCE_SPARK, 30 + rand.nextInt(30), 1);
            }
        }
        else
        {
            Spark s = sparks.getInstanceForced();
            float d = _deg + rand.nextSignedFloat(0.5f);
            s.set(pos, sin(d) * SPEED, cos(d) * SPEED, 0.6f + rand.nextSignedFloat(0.4f), 0.6f + rand.nextSignedFloat(0.4f), 0.1f, 20);
            s = sparks.getInstanceForced();
            d = _deg + rand.nextSignedFloat(0.5f);
            s.set(pos, -sin(d) * SPEED, -cos(d) * SPEED, 0.6f + rand.nextSignedFloat(0.4f), 0.6f + rand.nextSignedFloat(0.4f), 0.1f, 20);
        }
    }

    public override void draw()
    {
        if (lance)
        {
            float x = pos.x, y = pos.y;
            float size = 0.25f, a = 0.6f;
            int hc = hitCnt;
            for (int i = 0; i < cnt / 4 + 1; i++)
            {
                size = size * (0.9f);
                a = a * (0.8f);
                if (hc > 0)
                {
                    hc--;
                    continue;
                }

                float d = i * 13 + cnt * 3;
                for (int j = 0; j < 6; j++)
                {
                    glPushMatrix();
                    glTranslatef(x, y, 0);
                    glRotatef(-_deg * 180 / PI, 0, 0, 1);
                    glRotatef(d, 0, 1, 0);
                    GrScreen.setColor(0.4f, 0.8f, 0.8f, a);
                    glBegin(GL_LINE_LOOP);
                    glVertex3f(-size, LANCE_SPEED, size / 2);
                    glVertex3f(size, LANCE_SPEED, size / 2);
                    glVertex3f(size, -LANCE_SPEED, size / 2);
                    glVertex3f(-size, -LANCE_SPEED, size / 2);
                    glEnd();
                    GrScreen.setColor(0.2f, 0.5f, 0.5f, a / 2);
                    glBegin(GL_TRIANGLE_FAN);
                    glVertex3f(-size, LANCE_SPEED, size / 2);
                    glVertex3f(size, LANCE_SPEED, size / 2);
                    glVertex3f(size, -LANCE_SPEED, size / 2);
                    glVertex3f(-size, -LANCE_SPEED, size / 2);
                    glEnd();
                    glPopMatrix();
                    d = d + (60);
                }

                x = x - (sin(deg) * LANCE_SPEED * 2);
                y = y - (cos(deg) * LANCE_SPEED * 2);
            }
        }
        else
        {
            glPushMatrix();
            GrScreen.glTranslate(pos);
            glRotatef(-_deg * 180 / PI, 0, 0, 1);
            glRotatef(cnt * 31, 0, 1, 0);
            shape.draw();
            glPopMatrix();
        }
    }

    public float deg
    {
        get
        {
            return _deg;
        }

        set
        {
            _deg = value;
        }
    }

    public int damage
    {
        get
        {
            return _damage;
        }

        set
        {
            _damage = value;
        }
    }

    public bool removed()
    {
        if (hitCnt > 0)
            return true;
        else
            return false;
    }
}

public class ShotPool : ActorPool<Shot>
{
    public ShotPool(int n, List<object> args) : base(n, args, () => new Shot())
    {
    }

    public bool existsLance()
    {
        foreach (Shot s in actor)
            if (s.exists)
                if (s.lance && (!s.removed()))
                    return true;
        return false;
    }
}

public class ShotShape : CollidableDrawable
{
    public ShotShape()
    {
        initializeShape();
        setCollision();
    }

    public override void createDisplayList()
    {
        GrScreen.setColor(0.1f, 0.33f, 0.1f);
        glBegin(GL_QUADS);
        glVertex3f(0, 0.3f, 0.1f);
        glVertex3f(0.066f, 0.3f, -0.033f);
        glVertex3f(0.1f, -0.3f, -0.05f);
        glVertex3f(0, -0.3f, 0.15f);
        glVertex3f(0.066f, 0.3f, -0.033f);
        glVertex3f(-0.066f, 0.3f, -0.033f);
        glVertex3f(-0.1f, -0.3f, -0.05f);
        glVertex3f(0.1f, -0.3f, -0.05f);
        glVertex3f(-0.066f, 0.3f, -0.033f);
        glVertex3f(0, 0.3f, 0.1f);
        glVertex3f(0, -0.3f, 0.15f);
        glVertex3f(-0.1f, -0.3f, -0.05f);
        glEnd();
    }

    public override void setCollision()
    {
        _collision = new Vector(0.33f, 0.33f);
    }
}

public class LanceShape : Collidable
{
    public Vector getCollision()
    {
        return collisionBounds;
    }

    public bool checkCollision(float ax, float ay, Collidable shape = null)
    {
        float cx = 0, cy = 0;
        if (shape != null)
        {
            cx = collisionBounds.x + shape.getCollision().x;
            cy = collisionBounds.y + shape.getCollision().y;
        }
        else
        {
            cx = collisionBounds.x;
            cy = collisionBounds.y;
        }

        if ((ax <= cx) && (ay <= cy))
            return true;
        else
            return false;
    }

    public Vector _collision;
    public LanceShape()
    {
        _collision = new Vector(0.66f, 0.66f);
    }

    public Vector collisionBounds
    {
        get
        {
            return _collision;
        }

        set
        {
            _collision = value;
        }
    }
}
