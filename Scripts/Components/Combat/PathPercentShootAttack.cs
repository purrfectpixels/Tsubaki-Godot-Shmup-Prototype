using Godot;
using System;

[GlobalClass]
public partial class PathPercentShootAttack : EnemyAttackController
{
	[ExportGroup("Shot")]
	[Export] public float BulletSpeed { get; set; } = 200f;
	// = 1: a single aimed shot, > 1 = a fanned volley
	[Export] public int BulletCount { get; set; } = 1;
	[Export] public float SpreadDegree { get; set; } = 30f;

	private PathLockedMovementComponent _pathLockedMovementComponent;

    // Called when the node enters the scene tree for the first time.
    protected override void OnAttackReady()
    {
        _pathLockedMovementComponent = EnemyOwner?.GetMovementComponent<PathLockedMovementComponent>();

		if (_pathLockedMovementComponent != null)
		{
			_pathLockedMovementComponent.Shoot += ExecuteAttack;
		}
    }


    public override void ExecuteAttack()
    {
        EnemyBulletEmitter barrel = EnemyOwner?.GetActiveBarrel();

		if (barrel == null) return;

		if (BulletCount <= 1)
		{
			barrel.FireAimedSingle(BulletSpeed);
		}
		else
		{
			barrel.FireSpread(BulletCount, SpreadDegree, BulletSpeed, baseDegrees: 0f, aimAtPlayer: true);
		}
    }
}
