using Godot;
using System;

[GlobalClass]
public partial class BaseBullet : Node2D
{
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
			Position = new Vector2(Position.X + _bulletData.BaseSpeed * (float)delta, Position.Y);
		}
	}

	private bool CheckOOB()
	{
		if(Position.X < 0 || Position.X > _screenSize.X || Position.Y < 0 || Position.Y > _screenSize.Y)
		{
			return true;
		}
		return false;
	}

    public override void _PhysicsProcess(double delta)
    {
        Move(delta);
		if(CheckOOB())
		{
			Recycle();
		}
    }

}
