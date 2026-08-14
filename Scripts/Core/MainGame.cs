using Godot;
using System;

[GlobalClass]
public partial class MainGame : Node2D
{
	[Export] public Node2D LevelContainer { get; set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GD.Print("Hello World!");
	}

	public Node2D GetCurrentLevel()
	{
		return LevelContainer.GetChild<Node2D>(0);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
