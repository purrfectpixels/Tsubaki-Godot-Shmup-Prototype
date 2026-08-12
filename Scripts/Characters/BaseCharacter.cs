using Godot;
using System;

[GlobalClass]
public partial class BaseCharacter : CharacterBody2D , IHurtable
{
	[ExportGroup("Components")]
	[Export] public HealthComponent HealthComponent { get; private set; }
	[Export] public MovementComponent MovementComponent { get; set; }
	[Export] public CharacterVisualComponent CharacterVisualComponent { get; private set; }
	[ExportGroup("Identity")]
	[Export] public string CharacterName { get; private set; } = "Unnamed";
	[Export] public Sprite2D CharacterSprite { get; private set; }
	
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
			HealthComponent.Hurt += OnHurt;
		}
	}

	public bool IsCurrentlyImmune()
	{
		if (HealthComponent != null)
		{
			return HealthComponent.IsImmune();
		}
		return false;
	}

	public void TakeDamage(float damage)
	{
		if(HealthComponent != null)
		{
			HealthComponent.TakeDamage(damage);
		}
		else
		{
			GD.PrintErr("HealthComponent is null for character: ", CharacterName);
		}
	}

	public void OnHurt(float damage)
    {
        GD.Print("I am hurt! I lost ", damage);
        if(CharacterVisualComponent != null)
        {
            CharacterVisualComponent.HitFlashRepeatedly();
        }
    }

	public void OnHealthChanged(float health, float maxHP)
	{
		GD.Print("Ouch! I'm ", CharacterName, " and I am hurt!");
	}
}
