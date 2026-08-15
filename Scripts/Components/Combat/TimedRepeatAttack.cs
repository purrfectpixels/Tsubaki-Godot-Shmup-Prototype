using Godot;
using System;

[GlobalClass]
public partial class TimedRepeatAttack : EnemyAttackController
{
	[ExportGroup("Timing")]
	[Export] public float DelayBeforeFiring { get; set; } = 1.0f;
	[Export] public float FireInterval { get; set; } = 0.75f;
	[ExportGroup("Shot")]
	[Export] public float BulletSpeed { get; set; } = 200f;
 
	private float _onscreenTimer;
	private float _fireTimer;
	private bool _isFiring;

    public override void ProcessAttack(double delta)
    {
		if (EnemyOwner == null || !EnemyOwner.IsActivated) return;

		if (!_isFiring)
		{
			_onscreenTimer += (float)delta;
			if (_onscreenTimer >= DelayBeforeFiring)
			{
				_isFiring = true;
				_fireTimer = FireInterval; // fire immediately on the first eligible frame
			}
			return;
		}

		_fireTimer += (float)delta;
		if (_fireTimer >= FireInterval)
		{
			ExecuteAttack();
			_fireTimer = 0f;
		}
    }

    public override void ExecuteAttack()
    {
        EnemyOwner?.GetActiveBarrel()?.FireAimedSingle(BulletSpeed);
    }

	public override void ResetAttack()
    {
        _onscreenTimer = 0f;
        _fireTimer = 0f;
        _isFiring = false;
    }
}
