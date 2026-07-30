using Godot;
using System;

[GlobalClass]
public partial class BaseBullet : Node2D
{
	private int _outOfBoundsMargin = 50;
	private BulletData _bulletData;
	private bool _isActive = false;
	private Vector2 _screenSize;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
		_screenSize = GetViewportRect().Size;
    }

	public void Initialize(BulletData bulletData, Vector2 position, Vector2 direction)
	{
		_bulletData = bulletData;
		Position = position;
		Rotation = direction.Angle();
		Visible = true;
		SetProcess(true);
		SetPhysicsProcess(true);
		_isActive = true;
	}

	public void Reset()
	{
		Visible = false;
		SetProcess(false);
		SetPhysicsProcess(false);
		_isActive = false;
	}

	public virtual void Recycle()
	{
		if(_bulletData != null)
		{
			ObjectPool.Instance.ReturnBullet(this, _bulletData.BulletName);
		}
	}

	public void Move(double delta)
	{
		if(_bulletData != null)
		{
			Vector2 direction = Vector2.FromAngle(Rotation);
			Position += direction * _bulletData.BaseSpeed * (float)delta;
		}
	}

	private bool CheckOOB()
	{
		if(Position.X < -_outOfBoundsMargin || Position.X > _screenSize.X + _outOfBoundsMargin || Position.Y < -_outOfBoundsMargin || Position.Y > _screenSize.Y + _outOfBoundsMargin)
		{
			return true;
		}
		return false;
	}

	public void ManualUpdate(double delta)
	{
		Move(delta);
		if(CheckOOB())
		{
			Recycle();
		}
	}
}
