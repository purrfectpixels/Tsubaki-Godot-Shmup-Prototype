using Godot;
using System;

[GlobalClass]
public partial class TestEmitter : Marker2D
{
    [ExportGroup("Bullet Pool Settings")]
    [Export] public string BulletMeshId { get; set; } = "Pellet"; // Match this with your BulletMesh Id

    [ExportGroup("Pattern Settings")]
    [Export] public int Arms { get; set; } = 8;                  // Number of spiral arms
    [Export] public float BulletSpeed { get; set; } = 250f;      // Speed of each bullet
    [Export] public float FireRate { get; set; } = 0.02f;        // Seconds between bursts
    [Export] public float RotationSpeed { get; set; } = 2.5f;    // Angular velocity (radians/sec)
    [Export] public float AngleStepPerBurst { get; set; } = 0.1f; // Shift angle slightly each shot

    [Export] public BulletsManager BulletsManager { get; set;}
    private float _fireTimer = 0f;
    private float _currentAngle = 0f;

    public override void _Ready()
    {
        // Try to locate the BulletsManager in the scene
        BulletsManager = GetTree().Root.FindChild("BulletsManager", recursive: true, owned: false) as BulletsManager;

        if (BulletsManager == null)
        {
            GD.PrintErr("TestEmitter: Could not find BulletsManager node in the scene tree!");
        }
    }

    public override void _Process(double delta)
    {
        float floatDelta = (float)delta;

        // Continuously rotate the emitter head
        _currentAngle += RotationSpeed * floatDelta;

        _fireTimer += floatDelta;
        if (_fireTimer >= FireRate)
        {
            _fireTimer -= FireRate;
            FireSpiralPattern();
        }
    }

    private void FireSpiralPattern()
    {
        if (BulletsManager == null) return;

        // Increment angle offset per shot for a tighter, curved trail effect
        _currentAngle += AngleStepPerBurst;

        // Calculate angle distance between each arm
        float angleStep = Mathf.Tau / Arms; // Tau = 2 * PI

        for (int i = 0; i < Arms; i++)
        {
            float armAngle = _currentAngle + (i * angleStep);
            
            // Calculate velocity vector based on angle and speed
            Vector2 velocity = new Vector2(Mathf.Cos(armAngle), Mathf.Sin(armAngle)) * BulletSpeed;

            // Spawn from the Marker2D's global position
            BulletsManager.SpawnBullet(BulletMeshId, GlobalPosition, velocity);
        }
    }
}