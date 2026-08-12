using Godot;
using System;

public enum FlightPathType
{
    UpperRight,
    UpperLeft,
    LowerRight,
    LowerLeft
}

[GlobalClass]
public partial class FlightPaths : Node2D
{
    [Export] public Path2D UpperRightPath { get; set; }
    [Export] public Path2D UpperLeftPath { get; set; }
    [Export] public Path2D LowerRightPath { get; set; }
    [Export] public Path2D LowerLeftPath { get; set; }

    public Path2D GetFlightPath(FlightPathType type)
    {
        return type switch
        {
            FlightPathType.UpperRight => UpperRightPath,
            FlightPathType.UpperLeft => UpperLeftPath,
            FlightPathType.LowerRight => LowerRightPath,
            FlightPathType.LowerLeft => LowerLeftPath,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}
