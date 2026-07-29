using Godot;
using System;

[GlobalClass]
public partial class BaseCharacter : CharacterBody2D , IHurtable
{
	[ExportGroup("Components")]
	[Export] public HealthComponent HealthComponent { get; private set; }
	[Export] public MovementComponent MovementComponent { get; private set; }
	[Export] public Node CharacterVisualComponent { get; private set; }
	[ExportGroup("Identity")]
	[Export] public string CharacterName { get; private set; } = "Unnamed";
	
	public bool IsDead => IsInstanceValid(HealthComponent) && HealthComponent.IsDead;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		InitializeComponents();
	}

	private void InitializeComponents()
	{
		if(HealthComponent != null)
		{
			HealthComponent.HealthChanged += OnHealthChanged;
		}
	}

	public void TakeDamage(float damage)
	{
		if(HealthComponent != null)
		{
			HealthComponent.TakeDamage(damage);
		}
	}

	public void OnHealthChanged(float health, float maxHP)
	{
		GD.Print("Ouch! I'm ", CharacterName, " and I am hurt!");
	}
}
