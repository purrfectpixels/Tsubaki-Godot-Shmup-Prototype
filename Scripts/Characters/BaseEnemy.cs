using Godot;
using System;

[GlobalClass]
public partial class BaseEnemy : BaseCharacter
{
	[ExportGroup("Notifier")]
	[Export] public VisibleOnScreenNotifier2D VisibilityNotifier { get; set; }
	[ExportGroup("Cleanup")]
    [Export] public float OffscreenCullMargin { get; set; } = 150f;
	protected bool _isActivated = false;
	// Explaination: Enemies when spawned will be offscreen and will have to make their way to the screen.
	// Once they are on screen, they will be activated.
	// If they go offscreen again, they will be culled and removed from the scene.
	[ExportGroup("Combat")]
	[Export] public HitboxComponent Hitbox { get; set; }

	public override void _Ready()
	{
		base._Ready();
		if (Hitbox != null)
		{
			Hitbox.SetTeam(Team.Enemy);
			Hitbox.HitboxEntered += OnHitboxEntered;
		}
		else
		{
			GD.PrintErr("HitboxComponent is not assigned for enemy: ", Name);
		}
	}

	private bool IsOnScreen(float margin)
	{
		Rect2 view = GetViewportRect();
		return GlobalPosition.X < -OffscreenCullMargin || GlobalPosition.X > view.Size.X + OffscreenCullMargin ||
            GlobalPosition.Y < -OffscreenCullMargin || GlobalPosition.Y > view.Size.Y + OffscreenCullMargin;
	}

	protected void CullIfOffscreen()
	{
		if (!IsOnScreen(OffscreenCullMargin))
		{
			if (!_isActivated)
			{
				return;
			}
			QueueFree(); // TODO: Replace with object pooling.
		}
	}

	private void OnHitboxEntered() // Enemy collided with player's hurtbox, take damage.
	{
		if (HealthComponent != null)
		{
			if (!HealthComponent.IsImmune())
			{
				HealthComponent.TakeDamage(1f); // Assuming 1 damage for now, can be adjusted later.
				if (HealthComponent.IsDead)
				{
					Hitbox.Deactivate(); // Deactivate the hitbox if the character is dead
				}
			}
		}
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		CullIfOffscreen();
	}
}
