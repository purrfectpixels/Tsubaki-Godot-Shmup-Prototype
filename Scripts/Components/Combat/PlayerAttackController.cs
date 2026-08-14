using Godot;
using System;

[GlobalClass]
public partial class PlayerAttackController : AttackController
{
	[ExportGroup("Combat")]
    [Export] public BulletData BulletData { get; set; }
    [Export] public float ShootCooldown { get; set; } = 0.5f;

    private float _timeSinceLastShot = 0f;

    public override void ProcessAttack(double delta)
    {
        if (_timeSinceLastShot < ShootCooldown)
        {
            _timeSinceLastShot += (float)delta;
        }

        if (_timeSinceLastShot >= ShootCooldown)
        {
            ExecuteAttack();
            _timeSinceLastShot = 0f;
        }
    }

    public override void ExecuteAttack()
    {
        if (CharacterOwner is not PlayerCharacter player) return;
        if (player.BulletSpawnPoint == null || player.CharacterSprite == null) return;

        float facing = player.CharacterSprite.FlipH ? -1f : 1f;
        Vector2 localOffset = player.BulletSpawnPoint.Position;
        localOffset.X *= facing;
        Vector2 spawnPosition = player.ToGlobal(localOffset);

        ObjectPool.Instance.SpawnBullet<BaseBullet>(BulletData, new Vector2(spawnPosition.X, spawnPosition.Y - 24), new Vector2(facing, 0f), Team.Player);
        ObjectPool.Instance.SpawnBullet<BaseBullet>(BulletData, spawnPosition, new Vector2(facing, 0f), Team.Player);
        ObjectPool.Instance.SpawnBullet<BaseBullet>(BulletData, new Vector2(spawnPosition.X, spawnPosition.Y + 24), new Vector2(facing, 0f), Team.Player);
    }
}
