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
	[ExportGroup("Touch Smoothing")]
    // Higher values = tighter, more responsive tracking.
    // Lower values = smoother, but can add noticeable lag.
    // 35f to 50f is the sweet spot for shmups.
    [Export] public float TouchSmoothing { get; set; } = 40f;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		PlayerCharacter = GetParent<PlayerCharacter>();
	}

    public override void Move(double delta)
    {
        Vector2 targetVelocity = Vector2.Zero;
		Vector2 direction;
		float dt = (float)delta;
		float speed;

		TouchJoystickService touch = TouchJoystickService.Instance;
		if (touch != null && touch.IsActive)
		{
			Vector2 offset = touch.ConsumeOffset();

			// Raw target velocity calculated from touch drag this frame
            targetVelocity = dt > 0f ? offset / dt : Vector2.Zero;

            // Frame-rate independent Lerp smoothing to eliminate touch polling jitter
            PlayerCharacter.Velocity = PlayerCharacter.Velocity.Lerp(targetVelocity, 1f - Mathf.Exp(-TouchSmoothing * dt));
		}
		else
		{
			direction = Input.GetVector("move_left", "move_right", "move_up", "move_down");
			speed = Input.IsActionPressed("focus") ? _focusSpeed : BaseSpeed;

			PlayerCharacter.Velocity = targetVelocity;
		}
 
		PlayerCharacter.Velocity = ClampVelocityToViewportBounds(PlayerCharacter.Velocity, dt);
		PlayerCharacter.MoveAndSlide();
    }

	private Vector2 ClampVelocityToViewportBounds(Vector2 velocity, float delta)
	{
		Marker2D hurtboxOrigin = PlayerCharacter.HurtboxOrigin;
		if (hurtboxOrigin == null)
		{
			GD.PrintErr($"{PlayerCharacter.Name}: No HurtboxOrigin assigned, cannot clamp to viewport.");
			return velocity;
		}

		float radius = PlayerCharacter.HurtboxRadius;
		Rect2 worldRect = GetWorldViewportRect();

		float minX = worldRect.Position.X + radius;
		float maxX = worldRect.End.X - radius;
		float minY = worldRect.Position.Y + radius;
		float maxY = worldRect.End.Y - radius;

		// Guard against a viewport smaller than the hurtbox
		if (minX > maxX) { float mid = worldRect.Position.X + worldRect.Size.X * 0.5f; minX = maxX = mid; }
		if (minY > maxY) { float mid = worldRect.Position.Y + worldRect.Size.Y * 0.5f; minY = maxY = mid; }

		Vector2 hurtboxPos = hurtboxOrigin.GlobalPosition;
		Vector2 predictedPos = hurtboxPos + velocity * delta;

		// Only kill velocity that's actively trying to push FURTHER past a boundary.
		// If the hurtbox is already out of bounds (pushed there externally) and the
		// player is trying to move back IN, this does not interfere.
		if (predictedPos.X < minX && velocity.X < 0f) velocity.X = 0f;
		if (predictedPos.X > maxX && velocity.X > 0f) velocity.X = 0f;
		if (predictedPos.Y < minY && velocity.Y < 0f) velocity.Y = 0f;
		if (predictedPos.Y > maxY && velocity.Y > 0f) velocity.Y = 0f;

		return velocity;
	}

	private Rect2 GetWorldViewportRect()
	{
		Viewport viewport = PlayerCharacter.GetViewport();
		Rect2 screenRect = viewport.GetVisibleRect();
		Transform2D worldTransform = viewport.GetCanvasTransform().AffineInverse();

		Vector2 topLeft = worldTransform * screenRect.Position;
		Vector2 topRight = worldTransform * new Vector2(screenRect.End.X, screenRect.Position.Y);
		Vector2 bottomLeft = worldTransform * new Vector2(screenRect.Position.X, screenRect.End.Y);
		Vector2 bottomRight = worldTransform * screenRect.End;

		float minX = Mathf.Min(Mathf.Min(topLeft.X, topRight.X), Mathf.Min(bottomLeft.X, bottomRight.X));
		float maxX = Mathf.Max(Mathf.Max(topLeft.X, topRight.X), Mathf.Max(bottomLeft.X, bottomRight.X));
		float minY = Mathf.Min(Mathf.Min(topLeft.Y, topRight.Y), Mathf.Min(bottomLeft.Y, bottomRight.Y));
		float maxY = Mathf.Max(Mathf.Max(topLeft.Y, topRight.Y), Mathf.Max(bottomLeft.Y, bottomRight.Y));

		return new Rect2(minX, minY, maxX - minX, maxY - minY);
	}
}
