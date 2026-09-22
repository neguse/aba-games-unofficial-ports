#include "bulletmlparser.h"
#include "bulletmlparser-tinyxml.h"
#include "bulletmlrunner.h"
#include <cmath>
#include <cstdio>
#include <cstdlib>
#include <vector>

static int turn, randomState = 123;
static float rankValue;
struct Body;
static std::vector<Body*> bodies;

struct Body : BulletMLRunner {
    int id;
    float direction = 270, speed = 1, ax = 0, ay = 0, x = 0, y = 0;
    bool alive = true;
    Body(BulletMLParser* p) : BulletMLRunner(p), id(bodies.size()) {}
    Body(BulletMLState* p) : BulletMLRunner(p), id(bodies.size()) {}
    double getBulletDirection() { return direction; }
    double getBulletSpeed() { return speed; }
    double getAimDirection() { return 200 + (turn % 113) * .25f; }
    double getDefaultSpeed() { return 1; }
    double getRank() { return rankValue; }
    int getTurn() { return turn; }
    double getRand() {
        randomState = (randomState * 25173 + 13849) & 65535;
        return (float)randomState / 65536.f;
    }
    void createSimpleBullet(double d, double s) { spawn(0, d, s); }
    void createBullet(BulletMLState* p, double d, double s) { spawn(p, d, s); }
    void spawn(BulletMLState* p, double d, double s) {
        printf("F %d %d %d %.9g %.9g\n", turn, id, p ? (int)bodies.size() : -1, (float)d, (float)s);
        if (p) {
            Body* b = new Body(p); b->direction = d; b->speed = s;
            b->x = x; b->y = y; bodies.push_back(b);
        }
    }
    void doVanish() { alive = false; printf("V %d %d\n", turn, id); }
    void doChangeDirection(double v) { direction = v; }
    void doChangeSpeed(double v) { speed = v; }
    void doAccelX(double v) { ax = v; }
    void doAccelY(double v) { ay = v; }
    double getBulletSpeedX() { return ax; }
    double getBulletSpeedY() { return ay; }
};

int main(int argc, char** argv) {
    if (argc != 4) return 2;
    rankValue = atof(argv[2]);
    BulletMLParserTinyXML parser(argv[1]); parser.build();
    bodies.push_back(new Body(&parser));
    for (turn = 0; turn < atoi(argv[3]); ++turn) {
        size_t count = bodies.size();
        for (size_t i = 0; i < count; ++i) {
            Body& b = *bodies[i];
            if (!b.alive) continue;
            if (!b.isEnd()) b.run();
            b.x += (float)std::sin(b.direction * (float)(3.141592653589793 / 180)) * b.speed + b.ax;
            b.y += (float)std::cos(b.direction * (float)(3.141592653589793 / 180)) * b.speed - b.ay;
            printf("B %d %d %.9g %.9g %.9g %.9g %d %.9g %.9g\n", turn, b.id, b.direction, b.speed, b.ax, b.ay, b.isEnd(), b.x, b.y);
        }
    }
    for (Body* b : bodies) delete b;
}
