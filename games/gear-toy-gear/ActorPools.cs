// Copyright 2009 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;

public class ActorPools
{
    private GtgFrame frame;
    private GameState gameState;
    private Field field;
    private Player player;
    private LaserPool playerLasers;
    private PlayerHomingLaserPool playerHomingLasers;
    private ShotPool shots;
    private EnemyPool enemies;
    private MiddleEnemyPool middleEnemies;
    private LaserPool lasers;
    private TurretPool turrets;
    private HomingLaserPool homingLasers;
    private BulletPool bullets;
    private PillarPool pillars;
    private ParticlePool particles;
    private PlatePool plates;
    private Stage stage;
    private float shadowDepthOffset;
    public ActorPools(GtgFrame frame, Pad pad, Replay replay, Record record, Sound sound)
    {
        this.frame = frame;
        field = new Field(frame);
        particles = new ParticlePool(5120, frame);
        plates = new PlatePool(32, frame, field);
        gameState = new GameState(frame, pad, record, sound, plates);
        player = new Player(frame, gameState, field, pad, replay, particles, sound);
        pillars = new PillarPool(128, frame, field, player);
        bullets = new BulletPool(128, frame, field, player, pillars, particles);
        enemies = new EnemyPool(32, frame, gameState, field, bullets, player, pillars, particles, sound);
        lasers = new LaserPool(256, frame, field);
        turrets = new TurretPool(16, frame, field, lasers, pillars, player, particles, sound);
        homingLasers = new HomingLaserPool(16, frame, field, lasers, player, pillars, particles, sound);
        middleEnemies = new MiddleEnemyPool(8, frame, gameState, field, turrets, homingLasers, bullets, player, particles, sound);
        shots = new ShotPool(128, frame, field, enemies, middleEnemies, pillars, player, particles);
        playerLasers = new LaserPool(256, frame, field);
        playerHomingLasers = new PlayerHomingLaserPool(32, frame, field, playerLasers, player, enemies, middleEnemies, particles);
        stage = new Stage(enemies, middleEnemies, pillars, player, gameState);
        turrets.SetParams(middleEnemies);
        player.SetParams(shots, playerHomingLasers, enemies, middleEnemies);
        enemies.SetParams(playerHomingLasers);
        middleEnemies.SetParams(playerHomingLasers);
        pillars.SetParams(stage);
    }

    public void Start(int randomSeed)
    {
        particles.Clear();
        plates.Clear();
        pillars.Clear();
        bullets.Clear();
        enemies.Clear();
        lasers.Clear();
        turrets.Clear();
        homingLasers.Clear();
        middleEnemies.Clear();
        shots.Clear();
        playerLasers.Clear();
        playerHomingLasers.Clear();
        enemies.SetRandomSeed(randomSeed);
        middleEnemies.SetRandomSeed(randomSeed);
        particles.SetRandomSeed(randomSeed);
        gameState.SetRandomSeed(randomSeed);
        stage.Start(randomSeed);
        shadowDepthOffset = 0;
        gameState.Initialize();
        field.Initialize();
        player.Initialize();
    }

    public void Update()
    {
        shadowDepthOffset -= Stage.PlayerDepthSpeed * SimulationTime.Step;
        gameState.Update();
        stage.Update();
        field.Update();
        player.Update();
        shots.Update();
        playerLasers.Update();
        playerHomingLasers.Update();
        enemies.Update();
        middleEnemies.Update();
        lasers.Update();
        turrets.Update();
        homingLasers.Update();
        bullets.Update();
        pillars.Update();
        particles.Update();
        plates.Update();
    }

    public void UpdateField()
    {
        field.Update();
    }

    public void Draw()
    {
        frame.ShadowDepthOffset = shadowDepthOffset;
        player.SetLookAt();
        shots.Draw();
        lasers.Draw();
        bullets.Draw();
        enemies.Draw();
        middleEnemies.Draw();
        pillars.Draw();
        field.Draw();
    }

    public void DrawBloom()
    {
        player.SetLookAt();
        lasers.DrawEdge();
        bullets.DrawEdge();
        pillars.DrawEdge();
    }

    public void DrawEdge()
    {
        player.SetLookAt();
        player.Draw();
        lasers.DrawEdge();
        bullets.DrawEdge();
        enemies.DrawEdge();
        middleEnemies.DrawEdge();
        pillars.DrawEdge();
        plates.Draw();
        Letter.Draw();
    }

    public void DrawParticle()
    {
        particles.Draw();
        playerLasers.Draw();
    }

    public void DrawGameState()
    {
        gameState.Draw();
        stage.Draw();
    }

    public void DrawScore()
    {
        gameState.DrawLastScore();
    }

    public void DrawField()
    {
        field.Draw();
    }

    public void StartRecord()
    {
        player.StartRecord();
    }

    public void StartReplay()
    {
        player.StartReplay();
    }

    public void StartTitle()
    {
        Stage.PlayerDepthSpeed = 1.0f;
    }
}
