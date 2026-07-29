using Godot;
using System;

[GlobalClass]
public partial class HealthComponent : Node
{
	[Signal] public delegate void HealthChangedEventHandler(float newHealth, float maxHealth);
	[Signal] public delegate void DiedEventHandler();

	public bool IsDead { get; private set; }

	private float _health = 100.0f;
	private float _maxHP = 100.0f;

	private float _hitGraceTimer;

	[ExportGroup("Health data")]
	public float Health { get; private set; }
	[Export] public float MaxHealth 
	{
		get => _maxHP;
		set => _maxHP = value;
	}
	[Export] public float HitGraceDuration { get; set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Health = MaxHealth;
	}

	public void TakeDamage(float damage)
	{
		if (IsDead || _hitGraceTimer > 0)
		{
			return;
		}

		Health -= damage;
		_hitGraceTimer = HitGraceDuration;

		if(Health <= 0.0f)
		{
			Die();
		}
	}

	public void Die()
	{
		IsDead = true;
		EmitSignal(SignalName.Died);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		if(_hitGraceTimer > 0)
		{
			_hitGraceTimer -= (float)delta;
		}
	}
}
