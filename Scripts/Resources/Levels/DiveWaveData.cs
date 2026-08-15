using Godot;
using System;

[GlobalClass]
public partial class DiveWaveData : Resource
{
    [Export] public string EnemyId { get; set; }
    [Export] public NodePath SpawnPoint { get; set; }  // Where the enemy first appears (usually just offscreen)
    [Export] public NodePath EntryPoint { get; set; }  // Point it dives to before it starts firing
    [Export] public NodePath ExitPoint { get; set; }   // Point it flees towards afterwards
    [Export] public FormationShape Shape { get; set; } = FormationShape.Line;
    [Export] public float Spacing { get; set; } = 32f;
    [Export] public int Count { get; set; } = 1;
    [Export] public float StaggerDelay { get; set; } = 0.4f;
}