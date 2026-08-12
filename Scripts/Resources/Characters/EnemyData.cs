using Godot;
using System;

[GlobalClass]
public partial class EnemyData : Resource
{
	[Export] public string EnemyId { get; set; }
	[Export] public PackedScene EnemyScene { get; set; }
}
