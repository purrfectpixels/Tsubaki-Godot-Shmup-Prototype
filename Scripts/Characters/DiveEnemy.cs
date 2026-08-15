using Godot;
using System;

// A "diver": swoops onto screen, fires at the player, then flees back offscreen to
// be recycled by the EnemyPool. Movement is handled entirely by the reused
// SeekMovementComponent; DiveAndFleeAttack orchestrates the phases and firing.
[GlobalClass]
public partial class DiveEnemy : BaseEnemy
{
	public override void _Ready()
	{
		base._Ready();
		CurrentState = EnemyState.Entering;
	}

	protected override void OnSpawnedFromPool()
	{
		CurrentState = EnemyState.Entering;
	}

	// Called by WaveDirector right after spawning/positioning this enemy to start
	// its dive-in, fire, and flee-out sequence.
	public void SetupDiveAndFlee(Node2D entryPoint, Node2D exitPoint, Vector2 formationOffset)
	{
		DiveAndFleeAttack diveAttack = GetDiveAttack();
		if (diveAttack == null)
		{
			GD.PrintErr($"{Name}: No DiveAndFleeAttack found among AttackControllers.");
			return;
		}

		diveAttack.BeginDiveAndFlee(entryPoint, exitPoint, formationOffset);
	}

	private DiveAndFleeAttack GetDiveAttack()
	{
		if (AttackControllers == null) return null;

		foreach (AttackController attackController in AttackControllers)
		{
			if (attackController is DiveAndFleeAttack diveAttack)
			{
				return diveAttack;
			}
		}
		return null;
	}

	protected override void OnDeath()
	{
		base.OnDeath();
		Despawn();
	}
}