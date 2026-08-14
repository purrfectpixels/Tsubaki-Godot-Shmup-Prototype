using Godot;
using System;

[GlobalClass]
public partial class EnemyAttackController : AttackController
{
	protected BaseEnemy EnemyOwner { get; set; }
	protected bool IsRegistered { get; private set; }
	// Called when the node enters the scene tree for the first time.
	public sealed override void _Ready()
	{
		base._Ready();
		if (CharacterOwner != null)
		{
			if (CharacterOwner is not BaseEnemy)
			{
				GD.PrintErr("ERROR: Only enemies can use EnemyAttackController!");
				return;
			}
			else
			{
				EnemyOwner = (BaseEnemy)CharacterOwner;
			}
		}
		else
		{
			GD.PrintErr("ERROR: EnemyAttackController needs a BaseEnemy for CharacterOwner!");
			return;
		}

		IsRegistered = EnemyOwner.AttackControllers != null && EnemyOwner.AttackControllers.Contains(this);
		if (!IsRegistered)
		{
			GD.PrintErr($"{Name}: exists under {EnemyOwner.Name} but is not listed in its AttackControllers array, so it will stay inert. Add it there, or remove the node if it isn't supposed to be active.");
			return;
		}
 
		OnAttackReady();
	}

	protected virtual void OnAttackReady() { }
}
