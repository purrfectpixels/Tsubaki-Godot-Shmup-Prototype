using Godot;
using System;

public partial class PlayerCharacter : BaseCharacter
{
    [ExportGroup("Hurtbox Settings")]
    [Export] public Marker2D HurtboxOrigin { get; set; }
    [Export] public float HurtboxRadius { get; set; } = 4f;
    [ExportGroup("Combat Settings")]
    [Export] public Marker2D BulletSpawnPoint { get; set; }
    [ExportGroup("COMBAT TESTING REMOVE LATER")]
    [Export] public BulletData BulletData { get; set; } // FOR TESTING PURPOSES, REMOVE LATER
    [Export] public float ShootCooldown { get; set; } = 0.5f;
    private float _timeSinceLastShot = 0f;

    public override void _Ready()
    {
        base._Ready();
        // Register the player character with the PlayerService
        PlayerService.Instance.RegisterPlayer(this);
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        // Unregister the player character when it exits the scene tree
        PlayerService.Instance.UnregisterPlayer();
    }

    public void StressShoot(int bulletCount)
    {
        float angleStep = 360f / bulletCount;
        for (int i = 0; i < bulletCount; i++)
        {
            float angleRad = Mathf.DegToRad(i * angleStep);
            Vector2 direction = Vector2.Right.Rotated(angleRad);

            ObjectPool.Instance.SpawnBullet<BaseBullet>(BulletData, BulletSpawnPoint.GlobalPosition, direction);
        }
    }

    public void Shoot()
    {
        if (BulletSpawnPoint != null)
        {
            // Implementation for shooting
            ObjectPool.Instance.SpawnBullet<BaseBullet>(BulletData, BulletSpawnPoint.GlobalPosition, new Vector2(0.5f, -0.5f)); // Example direction, adjust as needed
            ObjectPool.Instance.SpawnBullet<BaseBullet>(BulletData, BulletSpawnPoint.GlobalPosition, Vector2.Right);
            ObjectPool.Instance.SpawnBullet<BaseBullet>(BulletData, BulletSpawnPoint.GlobalPosition, new Vector2(0.5f, 0.5f));
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (_timeSinceLastShot < ShootCooldown)
        {
            _timeSinceLastShot += (float)delta;
        }
        if(_timeSinceLastShot >= ShootCooldown && Input.IsActionPressed("shoot"))
        {
            //StressShoot(50); // Example: Shoot 3 bullets in a spread pattern
            Shoot();
            _timeSinceLastShot = 0f;
        }
    }
}
