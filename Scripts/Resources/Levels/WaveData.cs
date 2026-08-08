using Godot;

public enum SpawnType
{
    Path, // This is for flying enemies that follow a path
    WorldPosition // This is for ground enemies that spawn at a specific world position
}

[GlobalClass]
public partial class WaveData : Resource
{
    [Export] public PackedScene EnemyScene { get; set; }
    [Export] public PackedScene EnemyPath { get; set; }
    [Export] public SpawnType SpawnType { get; set; } = SpawnType.Path;
    [Export] public Vector2 SpawnPosition { get; set; } = Vector2.Zero;
    [Export] public int EnemyCount { get; set; } = 5;
    [Export] public float SpawnInterval { get; set; } = 1.0f;
}