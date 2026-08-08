using Godot;
using System;

[GlobalClass]
public partial class GuidingPoints : Node2D
{
    [Export] public Marker2D CenterLeftGuidingPoint { get; set; }
    [Export] public Marker2D CenterRightGuidingPoint { get; set; }
    [Export] public Marker2D CenterLeftExitPoint { get; set; }
    [Export] public Marker2D CenterRightExitPoint { get; set; }
}
