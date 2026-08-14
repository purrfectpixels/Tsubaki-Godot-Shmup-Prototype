using Godot;
using System;

[GlobalClass]
public partial class PathWaveData : Resource
{
    [Export] public string EnemyId { get; set; }
    [Export] public NodePath PathNode { get; set; }
    [Export] public int Count { get; set; } = 5;
    [Export] public float Delay { get; set; } = 0.3f;
}