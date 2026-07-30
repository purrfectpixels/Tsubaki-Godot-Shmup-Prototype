using Godot;
using System;

public partial class PlayerCharacter : BaseCharacter
{
    [Export] public Marker2D BulletSpawnPoint { get; set; }
    [ExportGroup("COMBAT TESTING REMOVE LATER")]
    [Export] public BulletData BulletData { get; set; } // FOR TESTING PURPOSES, REMOVE LATER
    [Export] public float ShootCooldown { get; set; } = 0.5f;
    private float _timeSinceLastShot = 0f;

    public override void _Ready()
    {
        base._Ready();
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
            StressShoot(50); // Example: Shoot 3 bullets in a spread pattern
            _timeSinceLastShot = 0f;
        }
    }
}
