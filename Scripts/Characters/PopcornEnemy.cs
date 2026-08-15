using Godot;
using System;

[GlobalClass]
public partial class PopcornEnemy : BaseEnemy
{

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		base._Ready();
		CurrentState = EnemyState.Entering;
	}

	// Runs every time this pooled instance is dispensed (not just the first time),
	// so state that _Ready only sets once still gets reapplied on reuse.
	protected override void OnSpawnedFromPool()
	{
		CurrentState = EnemyState.Entering;
	}

	// Called by WaveDirector after spawning the enemy to run on a path
	public void SetupPath(Path2D path, Vector2 formationOffset = default, float speedOverride = -1f, int playerIndex = -1)
	{
		PathLockedMovementComponent pathMoveComponent = GetMovementComponent<PathLockedMovementComponent>();
 
		if (pathMoveComponent == null) return;
		pathMoveComponent.StartPath(path, formationOffset, speedOverride);
		SetActiveMovement(pathMoveComponent);
	}

	// Called by WaveDirector after spawning the enemy to use seeking movement system
	public void SetupSeek(System.Collections.Generic.Stack<Node2D> targetPositions)
    {
        SeekMovementComponent movement = GetMovementComponent<SeekMovementComponent>();
        if (movement == null)
        {
            return;
        }

        movement.InsertStack(targetPositions);
        if (MovementComponent is not SeekMovementComponent)
		{
			SetActiveMovement(movement);
		}
    }

    protected override void OnDeath()
    {
        base.OnDeath();
		Despawn();
    }
}
