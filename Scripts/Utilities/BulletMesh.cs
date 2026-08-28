using Godot;
using System;

[GlobalClass]
public partial class BulletMesh : MultiMeshInstance2D
{
	private int _bulletsCount = 50000;
	private TinyBullet[] _bullets;
	private PlayerCharacter _player;
	public PlayerCharacter Player
	{
		get => _player;
		set => _player = value;
	}
	private float _margin = 48f;

	[ExportGroup("Internal Settings")]
	[Export] public string Id { get; set; } = "Untitled";
	[ExportGroup("Bullet Settings")]
	[Export] public Texture2D BulletTexture { get; set; }
	[Export] public int BulletCount
	{
		get => _bulletsCount;
		set => _bulletsCount = value;
	}
	[Export] public float BulletRadius { get; set; } = 2f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_bullets = new TinyBullet[_bulletsCount];
		// 1 - Setup QuadMesh
		QuadMesh quadMesh = new QuadMesh();
		if(BulletTexture != null)
		{
			quadMesh.Size = BulletTexture.GetSize();
			Texture = BulletTexture;
		}
		// 2 - Setup the MultiMesh
		Multimesh = new MultiMesh
		{
			TransformFormat = MultiMesh.TransformFormatEnum.Transform2D,
			UseColors = false,
			UseCustomData = false,
			InstanceCount = _bulletsCount,
			Mesh = quadMesh
		};
		// 3 - Hide all instances initially
		Multimesh.VisibleInstanceCount = 0;
	}

	public void SpawnBullet(Vector2 position, Vector2 velocity)
	{
		int index = Multimesh.VisibleInstanceCount;

		if(index >= _bulletsCount)
		{
			// Pool is full!
			return;
		}

		_bullets[index] = new TinyBullet
		{
			Position = position,
			Velocity = velocity,
			Radius = BulletRadius,
			IsActive = true
		};

		float rotation = velocity.Angle();
    	Multimesh.SetInstanceTransform2D(index, new Transform2D(rotation, position));

		Multimesh.VisibleInstanceCount++;
	}

	public void UpdateBullets(float delta, Rect2 viewportRect)
	{
		int i = 0;
		while (i < Multimesh.VisibleInstanceCount)
		{
			// Update position
			_bullets[i].Position += _bullets[i].Velocity * delta;

			// Check despawn conditions
			bool outOfBounds = !viewportRect.Grow(_margin).HasPoint(_bullets[i].Position);
			bool hitPlayer = CheckPlayerCollision(_bullets[i].Position, _bullets[i].Radius);

			if(outOfBounds || hitPlayer)
			{
				if (hitPlayer)
				{
					OnBulletHitPlayer(i);
				}

				// Swap i with back
				int lastIndex = Multimesh.VisibleInstanceCount - 1;

				if (i != lastIndex)
				{
					_bullets[i] = _bullets[lastIndex];

					Transform2D lastTransform = Multimesh.GetInstanceTransform2D(lastIndex);
					Multimesh.SetInstanceTransform2D(i, lastTransform);
				}

				// Deactivate back slot
				_bullets[lastIndex].IsActive = false;

				// Shrink total visible count by 1
				Multimesh.VisibleInstanceCount--;

				continue;
			}

			// Update active bullet transform
			float rotation = _bullets[i].Velocity.Angle();
			var transform = new Transform2D(rotation, _bullets[i].Position);
			Multimesh.SetInstanceTransform2D(i, transform);

			i++;
		}
	}

	private bool CheckPlayerCollision(Vector2 bulletPosition, float bulletRadius)
	{
		if (_player == null) return false;
		if (_player.HurtboxOrigin == null) return false;

		float combinedRadius = bulletRadius + _player.HurtboxRadius;

		return bulletPosition.DistanceSquaredTo(_player.HurtboxOrigin.GlobalPosition) <= (combinedRadius * combinedRadius);
	}

	private void OnBulletHitPlayer(int bulletIndex)
	{
		_bullets[bulletIndex].IsActive = false;
		// Move transform off screen
		Multimesh.SetInstanceTransform2D(bulletIndex, new Transform2D(0, new Vector2(-9999, -9999)));
		// Hurt the player and trigger cooldown
		GD.Print($"Bullet {bulletIndex} hit the player!");
		if (!_player.IsCurrentlyImmune())
			_player.TakeDamage(GlobalConstants.BaseBulletDamage);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		float deltaTime = (float)delta;
		Rect2 viewportRect = GetViewportRect();
		UpdateBullets(deltaTime, viewportRect);
	}
}
