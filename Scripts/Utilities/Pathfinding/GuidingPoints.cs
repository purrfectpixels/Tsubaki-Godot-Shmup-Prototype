using Godot;
using System;

public enum GuidingPointType
{
    CenterLeft, // 0
    CenterRight, // 1
    Player // 2
}

public enum ExitPointType
{
    CenterLeft,
    CenterRight,
    Player
}

[GlobalClass]
public partial class GuidingPoints : Node2D
{
    [Export] public Marker2D CenterLeftGuidingPoint { get; set; }
    [Export] public Marker2D CenterRightGuidingPoint { get; set; }
    [Export] public Marker2D CenterLeftExitPoint { get; set; }
    [Export] public Marker2D CenterRightExitPoint { get; set; }

    public Vector2 GetGuidingPoint(GuidingPointType type)
    {
        Node2D marker =  PlayerService.Instance?.PlayerCharacter;
        if (marker == null)
        {
            marker = CenterLeftGuidingPoint;
        }
        return type switch
        {
            GuidingPointType.CenterLeft => CenterLeftGuidingPoint.GlobalPosition,
            GuidingPointType.CenterRight => CenterRightGuidingPoint.GlobalPosition,
            GuidingPointType.Player => marker.GlobalPosition,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    public Node2D GetExitPoint(ExitPointType type)
    {
        Node2D marker =  PlayerService.Instance?.PlayerCharacter;
        if (marker == null)
        {
            marker = CenterLeftGuidingPoint;
        }
        return type switch
        {
            ExitPointType.CenterLeft => CenterLeftExitPoint,
            ExitPointType.CenterRight => CenterRightExitPoint,
            ExitPointType.Player => marker,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}
