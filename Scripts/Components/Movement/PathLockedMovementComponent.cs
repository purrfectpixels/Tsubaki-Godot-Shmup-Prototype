using Godot;
using System;

[GlobalClass]
public partial class PathLockedMovementComponent : MovementComponent
{
	[Signal] public delegate void ShootEventHandler();

	private Path2D _path;
	private float _distanceAlongPath = 0f;
	private float _pathLength = 0f;
	private bool _isMoving = false;
	private double _minimumShootDistanceProgress = 0.25;
	private double _maximumShootDistanceProgress = 0.75;
	private double _shootAtDistanceProgress = 0.25;
	// DEBUG REMOVE THIS 
	private bool _firedOnce = false;

	private Vector2 SampleGlobalPosition(float distance)
	{
		Vector2 localPosition = _path.Curve.SampleBaked(distance, cubic: false);
		return _path.ToGlobal(localPosition);
	}

	// Call once, right after spawning, to lock the owner onto a path
	public void StartPath(Path2D path)
	{
		_path = path;
		_distanceAlongPath = 0f;
		_isMoving = false;

		if (_path == null || _path.Curve == null)
		{
			GD.PrintErr("PathLockedMovementComponent: Provided path or its curve is null.");
			return;
		}
		DateTime now = DateTime.Now;
		_pathLength = _path.Curve.GetBakedLength();
		_isMoving = _pathLength > 0f;


		// Set the initial position to the start of the path
		if (_isMoving)
		{
			// Set the initial position to the start of the path
			ComponentOwner.GlobalPosition = SampleGlobalPosition(0f);
		}
	}

	public override void Move(double delta)
	{
		if (!_isMoving || _path == null || _path.Curve == null)
			return;

		_distanceAlongPath += BaseSpeed * (float)delta;

		Random rand = new Random();

		if (_distanceAlongPath >= _pathLength * _shootAtDistanceProgress && !_firedOnce)
		{
			if (rand.Next(0,10) >= 8)
				EmitSignal(SignalName.Shoot);
			_firedOnce = true;
		}

		if (_distanceAlongPath >= _pathLength)
		{
			ComponentOwner.GlobalPosition = SampleGlobalPosition(_pathLength);
			_isMoving = false;
			EmitSignal(SignalName.MovementCompleted);
			return;
		}

		ComponentOwner.GlobalPosition = SampleGlobalPosition(_distanceAlongPath);
	}
}
