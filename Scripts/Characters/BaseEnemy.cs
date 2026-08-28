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
	[Export] public HurtboxComponent Hurtbox { get; set; }
	[Export] public EnemyBulletEmitter LeftBarrel { get; set; }
	[Export] public EnemyBulletEmitter RightBarrel { get; set; }
	[Export] public Godot.Collections.Array<AttackController> AttackControllers { get; set; }
	[ExportGroup("Movement")]
	[Export] public Godot.Collections.Array<MovementComponent> RegisteredMovementComponents { get; set; }
	public EnemyState CurrentState { get; protected set; } = EnemyState.Spawning;
	public bool SuppressAutoMovementCompletion { get; set; } = false;
	public bool IsActivated => _isActivated;

	// Set by EnemyPool when this instance is dispensed. Identifies which pool
	// stack to return this enemy to instead of freeing it outright.
	public string PoolId { get; set; }

	protected bool facingRight = true;

	public override void _Ready()
	{
		base._Ready();
		Hurtbox ??= GetNodeOrNull<HurtboxComponent>("HurtboxComponent");
		Hitbox ??= GetNodeOrNull<HitboxComponent>("HitboxComponent");
		VisibilityNotifier ??= GetNodeOrNull<VisibleOnScreenNotifier2D>("VisibleOnScreenNotifier2D");
		if (Hitbox != null)
		{
			Hitbox.SetTeam(Team.Enemy);
			Hitbox.HitboxEntered += OnHitboxEntered;
		}
		else
		{
			GD.PrintErr("HitboxComponent is not assigned for enemy: ", Name);
		}

		if (Hurtbox == null)
		{
			GD.PrintErr("Error: Please assign HurtboxComponent!");
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
			Despawn(); // TODO: Replace with object pooling.
		}
	}

	// Returns this enemy to the EnemyPool it was dispensed from, or frees it if it
	// wasn't spawned through a pool (e.g. an enemy placed directly in a level).
	protected void Despawn()
	{
		if (!string.IsNullOrEmpty(PoolId) && EnemyPool.Instance != null)
		{
			EnemyPool.Instance.Return(this);
		}
		else
		{
			QueueFree();
		}
	}

	// Called by EnemyPool right after this instance is dispensed (freshly built or
	// reused) and re-parented/repositioned. Brings every component back to a clean,
	// alive state. Override OnSpawnedFromPool, not this, to add subclass-specific setup.
	public void ActivateFromPool()
	{
		CurrentState = EnemyState.Spawning;
		_isActivated = false;
		facingRight = true;
		SuppressAutoMovementCompletion = false;
 
		Visible = true;
		SetProcess(true);
		SetPhysicsProcess(true);
		Velocity = Vector2.Zero;
 
		HealthComponent?.ResetHealth();
		Hurtbox?.Activate();
		Hitbox?.SetTeam(Team.Enemy); // re-enables monitoring/collision
 
		if (RegisteredMovementComponents != null)
		{
			foreach (MovementComponent movementComponent in RegisteredMovementComponents)
			{
				movementComponent?.StopAndReset();
			}
		}
 
		if (AttackControllers != null)
		{
			foreach (AttackController attackController in AttackControllers)
			{
				attackController?.ResetAttack();
			}
		}
 
		OnSpawnedFromPool();
	}
 
	// Override to set an enemy's starting EnemyState/movement once it's dispensed
	// from the pool (e.g. PopcornEnemy sets itself to Entering).
	protected virtual void OnSpawnedFromPool() { }
 
	// Called by EnemyPool right before parking this instance back in its stack.
	public void ReturnToPool()
	{
		CurrentState = EnemyState.Dying;
		_isActivated = false;
 
		Visible = false;
		SetProcess(false);
		SetPhysicsProcess(false);
		Velocity = Vector2.Zero;
 
		Hitbox?.Deactivate();
		Hurtbox?.Deactivate();
 
		if (RegisteredMovementComponents != null)
		{
			foreach (MovementComponent movementComponent in RegisteredMovementComponents)
			{
				movementComponent?.StopAndReset();
			}
		}
 
		if (AttackControllers != null)
		{
			foreach (AttackController attackController in AttackControllers)
			{
				attackController?.ResetAttack();
			}
		}
	}

	private void OnHitboxEntered(Area2D area) // Enemy collided with player's hurtbox, take damage.
	{
		if (area is not HurtboxComponent)
			return;
		if (HealthComponent != null)
		{
			if (!HealthComponent.IsImmune())
			{
				HealthComponent.TakeDamage(GlobalConstants.BaseBulletDamage); // Assuming 1 damage for now, can be adjusted later.
			}
		}
	}

	protected bool IsFacingRight()
	{
		PlayerCharacter playerCharacter = PlayerService.Instance.PlayerCharacter;
		if (playerCharacter == null)
		{
			if (Mathf.Abs(Velocity.X) > 0.01f)
			{
				facingRight = Velocity.X > 0f;
			}
		}
		else
		{
			facingRight = playerCharacter.GlobalPosition.X > GlobalPosition.X;
		}
			
		return facingRight;
	}

	public EnemyBulletEmitter GetActiveBarrel()
	{
		return IsFacingRight() ? RightBarrel : LeftBarrel;
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		CullIfOffscreen();

		// Process attack
		if (CurrentState != EnemyState.Dying)
		{
			foreach (AttackController attack in AttackControllers)
			{
				attack?.ProcessAttack(delta);
			}
		}
	}

	public T GetMovementComponent<T>() where T : MovementComponent
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
	public void SetActiveMovement(MovementComponent component)
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
		if (SuppressAutoMovementCompletion) return;
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
