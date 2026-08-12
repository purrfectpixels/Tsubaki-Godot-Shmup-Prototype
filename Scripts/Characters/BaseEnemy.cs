using Godot;
using System;

public enum EnemyState
{
	Spawning,
	Entering,
	Guided,
	Active,
	Exiting,
	Dying
}

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
	[Export] public EnemyBulletEmitter LeftBarrel { get; set; }
	[Export] public EnemyBulletEmitter RightBarrel { get; set; }
	[ExportGroup("Movement")]
	[Export] public Godot.Collections.Array<MovementComponent> RegisteredMovementComponents { get; set; }
	public EnemyState CurrentState { get; protected set; } = EnemyState.Spawning;

	protected bool facingRight = true;

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

		if (HealthComponent != null)
		{
			HealthComponent.Died += OnDeath;
		}
		if (VisibilityNotifier != null)
		{
			VisibilityNotifier.ScreenEntered += OnEnterScreen;
		}
		else
		{
			GD.PrintErr("Visibility notifier not assigned!");
		}
	}

	private void OnEnterScreen()
	{
		_isActivated = true;
	}

	private bool IsOnScreen(float margin)
	{
		Rect2 view = GetViewportRect().Grow(margin);
		return view.HasPoint(GlobalPosition);
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
				HealthComponent.TakeDamage(GlobalConstants.BaseDamage); // Assuming 1 damage for now, can be adjusted later.
			}
		}
	}

	protected virtual void Shoot()
	{
		
	}

	protected bool IsFacingRight()
	{
		if (Mathf.Abs(Velocity.X) > 0.01f)
		{
			facingRight = Velocity.X > 0f;
		}
		return facingRight;
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		CullIfOffscreen();
	}

	protected T GetMovementComponent<T>() where T : MovementComponent
	{
		foreach(MovementComponent movementComponent in RegisteredMovementComponents)
		{
			if (movementComponent is T typed)
			{
				return typed;
			}
		}
		GD.PrintErr($"{Name}: no {typeof(T).Name} found among MovementComponents.");
		return null;
	}

	// Switch movement component
	protected void SetActiveMovement(MovementComponent component)
    {
        if (MovementComponent != null)
        {
            MovementComponent.MovementCompleted -= OnActiveMovementCompleted;
        }
 
        MovementComponent = component;
 
        if (MovementComponent != null)
        {
            MovementComponent.MovementCompleted += OnActiveMovementCompleted;
        }
    }

	private void OnActiveMovementCompleted()
    {
        OnMovementCompleted(MovementComponent);
    }
	
	// Override in a subclass to react to movement component reaching it's end
	protected virtual void OnMovementCompleted(MovementComponent movementComponent)
	{
		if (movementComponent is SeekMovementComponent seekMovement)
		{
			seekMovement.SeekNext();	
		}
	}

	protected virtual void OnDeath()
	{
		CurrentState = EnemyState.Dying;
		Hitbox?.Deactivate(); // Deactivate the hitbox when the enemy dies
	}
}
