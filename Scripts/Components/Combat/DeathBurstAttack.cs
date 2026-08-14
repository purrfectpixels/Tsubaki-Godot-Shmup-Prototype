using Godot;
using System;

[GlobalClass]
public partial class DeathBurstAttack : EnemyAttackController
{
	[ExportGroup("Burst")]
	[Export] public int BulletCount { get; set; } = 12;
	[Export] public float BulletSpeed { get; set; } = 160f;
	[Export] public float SpreadDegrees { get; set; } = 360f;
	
	// Called when the node enters the scene tree for the first time.
	protected override void OnAttackReady()
	{
		if (EnemyOwner?.HealthComponent != null)
		{
			EnemyOwner.HealthComponent.Died += OnOwnerDied;
		}
	}

	private void OnOwnerDied()
	{
		ExecuteAttack();
	}

	public override void ExecuteAttack()
	{
		EnemyBulletEmitter barrel = EnemyOwner?.GetActiveBarrel();
		if (barrel == null) return;
 
		barrel.FireSpread(BulletCount, SpreadDegrees, BulletSpeed, baseDegrees: 0f, aimAtPlayer: false);
	}
}
