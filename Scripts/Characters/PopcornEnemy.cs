using Godot;
using System;

[GlobalClass]
public partial class PopcornEnemy : BaseEnemy
{
	[ExportGroup("Enemy AI Settings")]
	[Export] public Node AttackController { get; set; }
	[ExportGroup("Combat Settings")]
	[Export] public float BulletSpeed { get; set; } = 150f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		base._Ready();
		CurrentState = EnemyState.Entering;
	}

    protected override void Shoot()
    {
        if (LeftBarrel == null && RightBarrel == null)
		{
			return;
		}

		if (!_isActivated)
		{
			return;
		}

		if(IsFacingRight())
		{
			RightBarrel.FireAimedSingle(BulletSpeed);
		}
		else
		{
			LeftBarrel.FireAimedSingle(BulletSpeed);
		}
    }

	private void OnShoot()
	{
		Shoot();
	}

	// Called by WaveDirector after spawning the enemy to run on a path
	public void SetupPath(Path2D path, int playerIndex = -1)
	{
		PathLockedMovementComponent pathMoveComponent = GetMovementComponent<PathLockedMovementComponent>();

		if (pathMoveComponent == null) return;
		pathMoveComponent.Shoot += OnShoot;
		pathMoveComponent.StartPath(path);
		SetActiveMovement(pathMoveComponent);
	}

	// Called by WaveDirector after spawning the enemy to use seeking movement system
	public void SetupSeek(System.Collections.Generic.Stack<Vector2> targetPositions, int playerIndex = -1)
    {
        SeekMovementComponent movement = GetMovementComponent<SeekMovementComponent>();
        if (movement == null)
        {
            return;
        }

        movement.InsertStack(targetPositions, playerIndex);
        if (MovementComponent is not SeekMovementComponent)
		{
			SetActiveMovement(movement);
		}
    }

    protected override void OnDeath()
    {
        base.OnDeath();
		QueueFree(); // TODO: Replace this with object pooling
    }

}
