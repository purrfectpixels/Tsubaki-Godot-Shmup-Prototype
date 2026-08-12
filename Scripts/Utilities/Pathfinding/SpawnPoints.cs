using Godot;
using System;

public enum SpawnPointType
{
    UpperRight, // 0
    UpperLeft, // 1
    LowerRight, // 2
    LowerLeft, // 3
    FirstTop, // 4
    SecondTop, // 5
    FirstBottom, // 6
    SecondBottom // 7
}

[GlobalClass]
public partial class SpawnPoints : Node2D
{
    [Export] public Marker2D UpperRightSpawnpoint { get; set; }
    [Export] public Marker2D UpperLeftSpawnpoint { get; set; }
    [Export] public Marker2D LowerRightSpawnpoint { get; set; }
    [Export] public Marker2D LowerLeftSpawnpoint { get; set; }
    [Export] public Marker2D FirstTopSpawnpoint { get; set; }
    [Export] public Marker2D SecondTopSpawnpoint { get; set; }
    [Export] public Marker2D FirstBottomSpawnpoint { get; set; }
    [Export] public Marker2D SecondBottomSpawnpoint { get; set; }

    public Marker2D GetSpawnPoint(SpawnPointType type)
    {
        return type switch
        {
            SpawnPointType.UpperRight => UpperRightSpawnpoint,
            SpawnPointType.UpperLeft => UpperLeftSpawnpoint,
            SpawnPointType.LowerRight => LowerRightSpawnpoint,
            SpawnPointType.LowerLeft => LowerLeftSpawnpoint,
            SpawnPointType.FirstTop => FirstTopSpawnpoint,
            SpawnPointType.SecondTop => SecondTopSpawnpoint,
            SpawnPointType.FirstBottom => FirstBottomSpawnpoint,
            SpawnPointType.SecondBottom => SecondBottomSpawnpoint,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}
