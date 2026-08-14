using Godot;
using System;

[GlobalClass]
public partial class WaveData : Resource
{
	[Export] public string enemyId { get; set; }
	[Export] public NodePath spawnPoint { get; set; }
	[Export] public Godot.Collections.Array<NodePath> guidingPoints { get; set; }
	[Export] public NodePath exitPoint { get; set; }
	[Export] public FormationShape formationShape { get; set; }
	[Export] public int count { get; set; }
	[Export] public float spacing { get; set; } = 8.0f;
	[Export] public float staggerDelay { get; set; } = 0.25f;
}
