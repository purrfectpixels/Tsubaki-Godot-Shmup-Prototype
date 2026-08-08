using Godot;
using System;

[GlobalClass]
public partial class FlightPaths : Node2D
{
    [Export] public Path2D UpperRightPath { get; set; }
    [Export] public Path2D UpperLeftPath { get; set; }
    [Export] public Path2D LowerRightPath { get; set; }
    [Export] public Path2D LowerLeftPath { get; set; }
}
