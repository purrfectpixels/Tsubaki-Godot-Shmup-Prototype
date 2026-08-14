using Godot;
using System;

[GlobalClass]
public partial class AttackController : Node
{
	protected BaseCharacter CharacterOwner { get; private set; }

    public override void _Ready()
    {
        base._Ready();
		CharacterOwner = GetParent<BaseCharacter>();
    }

	public virtual void ProcessAttack(double delta) { }
    public virtual void ExecuteAttack() { }
}
