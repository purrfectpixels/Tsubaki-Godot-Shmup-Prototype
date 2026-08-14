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

        AttackController?.ProcessAttack(delta);
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
