using Godot;
using System;
using System.Collections.Generic;

public partial class BulletsManager : Node2D
{
	private Godot.Collections.Array<BulletMesh> _bulletMeshes = new Godot.Collections.Array<BulletMesh>();
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var allChildren = GetChildren();
		foreach(var child in allChildren)
		{
			if(child is BulletMesh bulletMesh)
			{
				_bulletMeshes.Add(bulletMesh);
			}
		}
	}

	public void SpawnBullet(string Id, Vector2 position, Vector2 velocity)
	{
		foreach(var bulletMesh in _bulletMeshes)
		{
			if(bulletMesh.Id == Id)
			{
				bulletMesh.SpawnBullet(position, velocity);
				return;
			}
		}
		GD.Print($"No BulletMesh found with Id: {Id}");
	}

    public override void _Process(double delta)
    {
        base._Process(delta);
		if(PlayerService.Instance.PlayerCharacter != null)
		{
			foreach(var c in _bulletMeshes)
			{
				c.Player = PlayerService.Instance.PlayerCharacter;
			}
		}
    }

}
