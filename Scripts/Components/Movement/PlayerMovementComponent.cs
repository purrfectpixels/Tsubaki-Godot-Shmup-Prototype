using Godot;
using System;

[GlobalClass]
public partial class PlayerMovementComponent : MovementComponent
{
	private float _focusSpeed = 150f;
	private float _deceleration = 5000f;
	protected PlayerCharacter PlayerCharacter { get; private set; }
	[ExportGroup("Movement data")]
	[Export] public float FocusSpeed { get => _focusSpeed; set => _focusSpeed = value; }
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		PlayerCharacter = GetParent<PlayerCharacter>();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		Move(delta);
	}

    public override void Move(double delta)
    {
        Vector2 velocity = PlayerCharacter.Velocity;
		Vector2 direction = Input.GetVector("move_left", "move_right", "move_up", "move_down");
		if (direction != Vector2.Zero)
		{
			if(Input.IsActionPressed("focus"))
			{
				velocity = direction * _focusSpeed;
			}
			else
			{
				velocity = direction * BaseSpeed;
			}
		}
		else
		{
			velocity = velocity.MoveToward(Vector2.Zero, _deceleration * (float)delta);
		}
		PlayerCharacter.Velocity = velocity;
		PlayerCharacter.MoveAndSlide();
    }
}
