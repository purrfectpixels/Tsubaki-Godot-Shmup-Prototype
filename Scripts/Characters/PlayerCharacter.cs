using Godot;
using System;

[GlobalClass]
public partial class PlayerCharacter : BaseCharacter
{
    private float _hurtboxRadius = 4f;
    public float HurtboxRadius => _hurtboxRadius;
    [ExportGroup("Hurtbox Settings")]
    [Export] public Marker2D HurtboxOrigin { get; set; }
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
        QueueRedraw();
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
        if (BulletSpawnPoint == null || CharacterSprite == null)
            return;

        float facing = CharacterSprite.FlipH ? -1f : 1f;
        Vector2 localOffset = BulletSpawnPoint.Position;
        localOffset.X *= facing;
        Vector2 spawnPosition = ToGlobal(localOffset);

        ObjectPool.Instance.SpawnBullet<BaseBullet>(BulletData, spawnPosition, new Vector2(0.15f * facing, -0.15f), Team.Player);
        ObjectPool.Instance.SpawnBullet<BaseBullet>(BulletData, spawnPosition, new Vector2(facing, 0f), Team.Player);
        ObjectPool.Instance.SpawnBullet<BaseBullet>(BulletData, spawnPosition, new Vector2(0.15f * facing, 0.15f), Team.Player);
    }

    private void FlipCharacter()
    {
        if(Input.IsActionJustPressed("look_left"))
        {
            CharacterSprite.FlipH = true;
        }
        else if(Input.IsActionJustPressed("look_right"))
        {
            CharacterSprite.FlipH = false;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        FlipCharacter();
        if (_timeSinceLastShot < ShootCooldown)
        {
            _timeSinceLastShot += (float)delta;
        }
        if(_timeSinceLastShot >= ShootCooldown)
        {
            //StressShoot(50); // Example: Shoot 3 bullets in a spread pattern
            Shoot();
            _timeSinceLastShot = 0f;
        }
    }

    public override void _Draw()
    {
        base._Draw();
        GD.Print("Draw called, HurtboxOrigin: ", HurtboxOrigin);
        // Draw the hurtbox for the player to see
        if (HurtboxOrigin != null)
        {
            Vector2 localPos = ToLocal(HurtboxOrigin.GlobalPosition);
            // Filled white circle
            DrawCircle(localPos, _hurtboxRadius, Colors.White);

            // Red outline on top
            DrawArc(
                localPos,
                _hurtboxRadius,
                0f,
                Mathf.Tau,          // full circle: 0 to 2π
                32,                 // point count — raise for smoother curve
                Colors.Red,
                2f,                 // outline width in pixels
                true                // antialiased
            );
        }
        else
        {
            GD.PrintErr($"{Name}: No HurtboxOrigin assigned, cannot draw hurtbox.");
        }
    }
}
