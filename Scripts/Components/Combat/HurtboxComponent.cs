using Godot;
using System;

[GlobalClass]
public partial class HurtboxComponent : Area2D
{
    [ExportGroup("DEPRECATED SETTINGS FOR TESTING PURPOSES")]
    [Export] public Team Team { get; set; } = Team.Player;
    private CollisionShape2D _collisionShape { get; set; }
    public override void _Ready()
    {
        if (GetChild(0) is CollisionShape2D collisionShape)
        {
            _collisionShape = collisionShape;
            if (GetParent() is PlayerCharacter playerCharacter)
            {
                if (_collisionShape.Shape is CircleShape2D circleShape)
                {
                    circleShape.Radius = playerCharacter.HurtboxRadius; // Set the radius to the player's hurtbox radius
                }
            }
        }
        else
        {
            GD.PrintErr("HurtboxComponent requires a CollisionShape2D as its first child.");
        }
        if (GetParent() is BaseEnemy character)
        {
            Team = Team.Enemy;
        }
        SetupCollision();
        AreaEntered += OnAreaEntered;
        Monitoring = true;
        Monitorable = false;
    }
    
    public void SetupCollision()
    {
        if (Team == Team.Player)
        {
            CollisionLayer = GlobalConstants.PlayerLayer;
            CollisionMask = GlobalConstants.EnemyLayer | GlobalConstants.WorldLayer;
        }
        else if (Team == Team.Enemy)
        {
            CollisionLayer = GlobalConstants.EnemyLayer;
            CollisionMask = GlobalConstants.PlayerLayer | GlobalConstants.WorldLayer;
        }
    }

    public void Activate()
    {
        SetDeferred(Area2D.PropertyName.Monitoring, true);
        SetDeferred(Area2D.PropertyName.Monitorable, false);
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

    private void OnAreaEntered(Area2D area)
    {
        if (area is HitboxComponent hitbox)
        {
            if (hitbox.Team != Team)
            {
                float damage = GlobalConstants.BaseDamage;
                if (GetParent() is BaseCharacter character)
                {
                    character.TakeDamage(damage);
                    if(character.IsDead)
                    {
                        Deactivate(); // Deactivate the hurtbox if the character is dead
                    }
                }
                hitbox.EmitSignal(HitboxComponent.SignalName.HitboxEntered);
            }
        }
    }
}