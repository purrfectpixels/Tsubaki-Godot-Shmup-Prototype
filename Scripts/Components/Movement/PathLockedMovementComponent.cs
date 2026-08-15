using Godot;
using System;

[GlobalClass]
public partial class PathLockedMovementComponent : MovementComponent
{
	[Signal] public delegate void ShootEventHandler();

	[ExportGroup("Shoot Triggers")]
	[Export] public float[] ShootTriggerPercents { get; set; } = Array.Empty<float>();

	private Path2D _path;
	private float _distanceAlongPath = 0f;
	private float _pathLength = 0f;
	private bool _isMoving = false;
	private bool[] _shootTriggersFired = Array.Empty<bool>();
	// Needed for rigid formation
	private Vector2 _formationOffset = Vector2.Zero;

	public float ProgressRatio => _pathLength > 0f ? Mathf.Clamp(_distanceAlongPath / _pathLength, 0f, 1f) : 0f;

	private Vector2 SampleGlobalPosition(float distance)
	{
		if (_formationOffset == Vector2.Zero)
		{
			Vector2 localPosition = _path.Curve.SampleBaked(distance, cubic: false);
			return _path.ToGlobal(localPosition);
		}
 
		// SampleBakedWithRotation gives a transform whose origin is the point on the curve and
		// whose X axis points along the direction of travel, so transforming the offset by it
		// both rotates the offset to match the path's current heading and translates it into place.
		Transform2D pathLocalTransform = _path.Curve.SampleBakedWithRotation(distance, cubic: false);
		Vector2 offsetLocalPosition = pathLocalTransform * _formationOffset;
		return _path.ToGlobal(offsetLocalPosition);
	}

	// Call once, right after spawning, to lock the owner onto a path
	public void StartPath(Path2D path, Vector2 formationOffset = default, float speedOverride = -1f)
	{
		_path = path;
		_distanceAlongPath = 0f;
		_isMoving = false;
		_formationOffset = formationOffset;
		_shootTriggersFired = new bool[ShootTriggerPercents.Length];

		if (speedOverride > 0f)
		{
			BaseSpeed = speedOverride;
		}

		if (_path == null || _path.Curve == null)
		{
			GD.PrintErr("PathLockedMovementComponent: Provided path or its curve is null.");
			return;
		}
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
		CheckShootTriggers();
	
		if (_distanceAlongPath >= _pathLength)
		{
			ComponentOwner.GlobalPosition = SampleGlobalPosition(_pathLength);
			_isMoving = false;
			EmitSignal(SignalName.MovementCompleted);
			return;
		}

		ComponentOwner.GlobalPosition = SampleGlobalPosition(_distanceAlongPath);
	}

	public override void StopAndReset()
	{
		_path = null;
		_distanceAlongPath = 0f;
		_pathLength = 0f;
		_isMoving = false;
		_shootTriggersFired = Array.Empty<bool>();
		_formationOffset = Vector2.Zero;
	}

	private void CheckShootTriggers()
	{
		float progress = ProgressRatio;
		for (int i = 0; i < ShootTriggerPercents.Length; i++)
		{
			if(!_shootTriggersFired[i] && progress >= ShootTriggerPercents[i])
			{
				_shootTriggersFired[i] = true;
				EmitSignal(SignalName.Shoot);
			}
		}
	}
}
