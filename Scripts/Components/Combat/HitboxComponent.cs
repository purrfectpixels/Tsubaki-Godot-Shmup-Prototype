using Godot;
using System;

public enum Team
{
    Player,
    Enemy
}

[GlobalClass]
public partial class HitboxComponent : Area2D
{
	[Signal] public delegate void HitboxEnteredEventHandler(Area2D hurtbox);
    private CollisionShape2D _collisionShape { get; set; }

    private Team _team = Team.Player;
    public Team Team => _team;

    public override void _Ready()
    {
		if (GetChild(0) is CollisionShape2D collisionShape)
		{
			_collisionShape = collisionShape;
		}
		else
		{
			GD.PrintErr("HitboxComponent requires a CollisionShape2D as its first child.");
		}
		Deactivate(); // Start with the hitbox deactivated
        BodyEntered += OnBodyEntered;
    }

    public void SetTeam(Team team)
	{
		_team = team;

		if (_team == Team.Player)
		{
			CollisionLayer = GlobalConstants.PlayerLayer;
			CollisionMask = GlobalConstants.EnemyLayer | GlobalConstants.WorldLayer;
		}
		else if (_team == Team.Enemy)
		{
			CollisionLayer = GlobalConstants.EnemyLayer;
			CollisionMask = GlobalConstants.PlayerLayer | GlobalConstants.WorldLayer;
		}

		// Re-enable collisions when pulled from the pool
		SetDeferred(Area2D.PropertyName.Monitoring, true);
		SetDeferred(Area2D.PropertyName.Monitorable, true);

		if (_collisionShape != null)
		{
			_collisionShape.SetDeferred(CollisionShape2D.PropertyName.Disabled, false);
		}
	}

    public void Deactivate()
    {
		SetDeferred(Area2D.PropertyName.Monitoring, false);
    	SetDeferred(Area2D.PropertyName.Monitorable, false);
        // Disabling the shape directly safely stops collision detection without locking physics
        if (_collisionShape != null)
        {
            _collisionShape.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
        }
    }

    private void OnBodyEntered(Node body)
    {
        if (body is TileMapLayer)
        {
            EmitSignal(SignalName.HitboxEntered);
        }
    }
}