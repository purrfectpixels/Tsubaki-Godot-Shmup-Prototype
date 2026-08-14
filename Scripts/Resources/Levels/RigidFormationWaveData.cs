using Godot;
using System;

[GlobalClass]
public partial class RigidFormationWaveData : Resource
{
    [Export] public string EnemyId { get; set; }
    [Export] public NodePath PathNode { get; set; } // Points to live Path2D in level
    [Export] public FormationShape Shape { get; set; } = FormationShape.Line;
    [Export] public int Count { get; set; } = 5;
    [Export] public float Spacing { get; set; } = 16f;
    [Export] public float PathSpeed { get; set; } = 150f;
    [Export] public int DirectionFactor { get; set; } = -1;
}