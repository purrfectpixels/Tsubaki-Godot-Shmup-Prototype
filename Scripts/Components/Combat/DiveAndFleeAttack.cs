using Godot;
using System;

// Classic shmup "diver" behaviour: swoop onto the screen to an entry point, pause
// just long enough to fire a few bursts at the player, then dash back off-screen to
// an exit point where BaseEnemy's offscreen cull recycles it via the EnemyPool.
//
// Reuses SeekMovementComponent for both the dive-in and flee-out legs (it already
// knows how to move an owner to a Node2D target and signal MovementCompleted) - this
// controller only owns the timing/firing state machine and the phase transitions
// between the two seeks, driving them manually via EnemyOwner.SuppressAutoMovementCompletion.
[GlobalClass]
public partial class DiveAndFleeAttack : EnemyAttackController
{
	private enum Phase { Inactive, Diving, Hovering, Fleeing, Done }

	[ExportGroup("Movement")]
	[Export] public float DiveSpeed { get; set; } = 500f;
	[Export] public float FleeSpeed { get; set; } = 650f;

	[ExportGroup("Attack")]
	[Export] public float HoverDelay { get; set; } = 0.15f; // beat before the first shot
	[Export] public int BurstCount { get; set; } = 3;
	[Export] public float BurstInterval { get; set; } = 0.14f;
	[Export] public int BulletsPerBurst { get; set; } = 1;
	[Export] public float SpreadDegrees { get; set; } = 20f;
	[Export] public float BulletSpeed { get; set; } = 300f;

	private SeekMovementComponent _seek;
	private Node2D _exitPoint;
	private Phase _phase = Phase.Inactive;
	private float _phaseTimer;
	private int _burstsFired;

	// References for dynamic target cleanup
	private Marker2D _offsetEntryMarker;
    private Marker2D _offsetExitMarker;

	protected override void OnAttackReady()
	{
		_seek = EnemyOwner?.GetMovementComponent<SeekMovementComponent>();
		if (_seek != null)
		{
			_seek.MovementCompleted += OnSeekArrived;
		}
		else
		{
			GD.PrintErr($"{Name}: DiveAndFleeAttack requires a SeekMovementComponent registered on its owner.");
		}
	}

	// Kicks off the whole dive-in / hover-and-fire / flee-out sequence. Call this
	// once the enemy knows where to dive to and where to run away to afterwards
	// (e.g. from WaveDirector right after spawning it).
	public void BeginDiveAndFlee(Node2D entryPoint, Node2D exitPoint, Vector2 formationOffset)
	{
		if (_seek == null || entryPoint == null || exitPoint == null || EnemyOwner == null) return;

		// Clean up previous transient targets if re-initialized
        CleanupMarkers();

        // 1. Create a dynamic Marker2D for offset entry position
        _offsetEntryMarker = new Marker2D
        {
            GlobalPosition = entryPoint.GlobalPosition + formationOffset
        };
        entryPoint.GetTree().CurrentScene.AddChild(_offsetEntryMarker);

        // 2. Create a dynamic Marker2D for offset exit position
        _offsetExitMarker = new Marker2D
        {
            GlobalPosition = exitPoint.GlobalPosition + formationOffset
        };
        exitPoint.GetTree().CurrentScene.AddChild(_offsetExitMarker);

		_exitPoint = exitPoint;
		_burstsFired = 0;
		_phaseTimer = 0f;
		_phase = Phase.Diving;

		// We drive the seeks by hand through the phases below, so stop BaseEnemy's
		// default "auto SeekNext() on MovementCompleted" behaviour from firing.
		EnemyOwner.SuppressAutoMovementCompletion = true;
		EnemyOwner.SetActiveMovement(_seek);
		_seek.BaseSpeed = DiveSpeed;
		_seek.SeekTo(_offsetEntryMarker);
	}

	private void CleanupMarkers()
    {
        if (GodotObject.IsInstanceValid(_offsetEntryMarker))
        {
            _offsetEntryMarker.QueueFree();
            _offsetEntryMarker = null;
        }

        if (GodotObject.IsInstanceValid(_offsetExitMarker))
        {
            _offsetExitMarker.QueueFree();
            _offsetExitMarker = null;
        }
    }

	private void OnSeekArrived()
	{
		switch (_phase)
		{
			case Phase.Diving:
				_phase = Phase.Hovering;
				_phaseTimer = 0f;
				break;

			case Phase.Fleeing:
				_phase = Phase.Done;
				if (EnemyOwner != null)
				{
					EnemyOwner.SuppressAutoMovementCompletion = false;
				}
				CleanupMarkers();
				break;
		}
	}

	public override void ProcessAttack(double delta)
	{
		if (_phase != Phase.Hovering) return;

		_phaseTimer += (float)delta;

		float nextShotTime = HoverDelay + BurstInterval * _burstsFired;
		if (_burstsFired < BurstCount && _phaseTimer >= nextShotTime)
		{
			ExecuteAttack();
			_burstsFired++;
		}

		if (_burstsFired >= BurstCount)
		{
			Flee();
		}
	}

	public override void ExecuteAttack()
	{
		EnemyBulletEmitter barrel = EnemyOwner?.GetActiveBarrel();
		if (barrel == null) return;

		if (BulletsPerBurst <= 1)
		{
			barrel.FireAimedSingle(BulletSpeed);
		}
		else
		{
			barrel.FireSpread(BulletsPerBurst, SpreadDegrees, BulletSpeed, baseDegrees: 0f, aimAtPlayer: true);
		}
	}

	private void Flee()
	{
		if (_seek == null || _exitPoint == null) return;

		_phase = Phase.Fleeing;
		_seek.BaseSpeed = FleeSpeed;
		_seek.SeekTo(_offsetExitMarker);
	}

	// Called by EnemyPool (via BaseEnemy.ActivateFromPool/ReturnToPool) so this
	// controller doesn't resume mid-sequence the next time its owner is reused.
	public override void ResetAttack()
	{
		_phase = Phase.Inactive;
		_phaseTimer = 0f;
		_burstsFired = 0;
		_exitPoint = null;

		CleanupMarkers();
	}
}