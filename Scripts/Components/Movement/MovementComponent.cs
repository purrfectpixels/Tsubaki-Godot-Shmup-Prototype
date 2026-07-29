using Godot;
using System;

[GlobalClass]
public abstract partial class MovementComponent : Node
{
	[ExportGroup("Movement data")]
	[Export] protected float BaseSpeed { get; set; } = 200f;

	protected CharacterBody2D ComponentOwner;

    public override void _Ready()
    {
        ComponentOwner = GetParent<CharacterBody2D>();
    }

	public abstract void Move(double delta);
}
