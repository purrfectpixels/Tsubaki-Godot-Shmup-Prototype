using Godot;
using System;

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
}
