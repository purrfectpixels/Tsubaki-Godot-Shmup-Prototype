using Godot;
using System;

[GlobalClass]
public partial class BulletData : Resource
{
    [Export] public string BulletName { get; set; }
    [Export] public PackedScene BulletScene { get; set; }
    [Export] public int InitialPoolSize { get; set; } = 20;
    [Export] public float BaseSpeed { get; set; } = 500f;
    [Export] public float BaseDamage { get; set; } = 10f;
}
