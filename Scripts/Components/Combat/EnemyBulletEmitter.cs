using Godot;
using System;

[GlobalClass]
public partial class EnemyBulletEmitter : Node2D
{
	[ExportGroup("Bullet Pool Settings")]
	[Export] public string BulletMeshId { get; set; }
	[Export] public BulletsManager BulletsManager { get; set; }

	private float _spiralAngle;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		BulletsManager = GetTree().Root.FindChild("BulletsManager", recursive: true, owned: false) as BulletsManager;
		if (BulletsManager == null)
		{
			GD.PrintErr("Could not find BulletsManager node in the scene tree!");
		}
	}

	private Vector2 DirectionToPlayer()
	{
		PlayerCharacter playerCharacter = PlayerService.Instance?.PlayerCharacter;

		if (playerCharacter == null || playerCharacter.HurtboxOrigin == null)
		{
			return Vector2.Left; // FALLBACK
		}

		return (playerCharacter.HurtboxOrigin.GlobalPosition - GlobalPosition).Normalized();
	}

	// Fire single bullet at the player
	public void FireAimedSingle(float speed)
	{
		if (BulletsManager == null) return;
		BulletsManager.SpawnBullet(BulletMeshId, GlobalPosition, DirectionToPlayer() * speed);
	}

	// Fire x amount of bullets fanned across spread degrees
	public void FireSpread(int count, float spreadDegrees, float speed, float baseDegrees, bool aimAtPlayer)
	{
		if (BulletsManager == null || count <= 0) return;
 
        float baseAngle = aimAtPlayer ? DirectionToPlayer().Angle() : Mathf.DegToRad(baseDegrees);

		float halfSpread = Mathf.DegToRad(spreadDegrees) * 0.5f;

		float step = count > 1 ? (halfSpread * 2f) / (count - 1) : 0f;

		for (int i = 0; i < count; i++)
        {
            float angle = baseAngle - halfSpread + step * i;
            Vector2 velocity = Vector2.FromAngle(angle) * speed;
            BulletsManager.SpawnBullet(BulletMeshId, GlobalPosition, velocity);
        }
	}

	public void ResetSpiral()
    {
        _spiralAngle = 0f;
    }
}
