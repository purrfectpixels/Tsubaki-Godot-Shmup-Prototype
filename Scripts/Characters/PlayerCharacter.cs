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

    public void Shoot()
    {
        if (BulletSpawnPoint != null)
        {
            // Implementation for shooting
            ObjectPool.Instance.SpawnBullet<BaseBullet>(BulletData, BulletSpawnPoint.GlobalPosition, Vector2.Right);
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
            Shoot();
            _timeSinceLastShot = 0f;
        }
    }
}
