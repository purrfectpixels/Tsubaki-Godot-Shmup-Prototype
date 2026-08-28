using Godot;
using System;

public partial class TouchJoystickService : Node
{
	public static TouchJoystickService Instance { get; private set; }

	// Multiplier applied to raw finger/mouse movement before it becomes ship movement.
	// 1.0 = exact 1:1 tracking. Lower it (e.g. 0.5) for touchpad-style "extra precision"
	// where the finger has to travel further than the ship actually moves.
	[Export] public float Sensitivity { get; set; } = 1.0f;

	public bool IsActive { get; private set; }
	public Vector2 Origin { get; private set; } = Vector2.Zero;
	public Vector2 CurrentPosition { get; private set; } = Vector2.Zero;

	[Signal] public delegate void JoystickEngagedEventHandler(Vector2 origin);
	[Signal] public delegate void JoystickMovedEventHandler(Vector2 currentPosition);
	[Signal] public delegate void JoystickReleasedEventHandler();

	// -2 = nothing tracked, -1 = mouse, >=0 = a real finger index.
	private int _activeIndex = -2;
	private Vector2 _lastPosition = Vector2.Zero;
	// Raw pixel movement accumulated since the last ConsumeOffset() call.
	private Vector2 _accumulatedOffset = Vector2.Zero;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Instance = this;
	}

	public override void _Input(InputEvent @event)
	{
		switch (@event)
		{
			case InputEventScreenTouch touch:
				HandlePress(touch.Index, touch.Position, touch.Pressed);
				break;

			case InputEventScreenDrag drag:
				if (_activeIndex == drag.Index)
					UpdateStick(drag.Position);
				break;

			case InputEventMouseButton mouseButton when mouseButton.ButtonIndex == MouseButton.Left:
				HandlePress(-1, mouseButton.Position, mouseButton.Pressed);
				break;

			// FIXED: Checked ButtonMask to prevent hover-dragging
			case InputEventMouseMotion motion when (motion.ButtonMask & MouseButtonMask.Left) != 0:
				if (_activeIndex == -1)
					UpdateStick(motion.Position);
				break;
		}
	}

	// Consumes and clears the pixel movement accumulated since the last call.
	// PlayerMovementComponent should call this once per physics frame - whatever
	// it returns is exactly how far the finger/mouse moved this frame, already
	// scaled by Sensitivity. Vector2.Zero when nothing is being touched.
	public Vector2 ConsumeOffset()
	{
		Vector2 offset = _accumulatedOffset;
		_accumulatedOffset = Vector2.Zero;
		return offset;
	}

	private void UpdateStick(Vector2 position)
	{
		Vector2 frameDelta = position - _lastPosition;
		_accumulatedOffset += frameDelta * Sensitivity;

		_lastPosition = position;
		CurrentPosition = position;

		EmitSignal(SignalName.JoystickMoved, CurrentPosition);
		GetViewport().SetInputAsHandled();
	}

	private void HandlePress(int index, Vector2 position, bool pressed)
	{
		if (pressed)
		{
			// Already tracking a finger/mouse - ignore any additional presses (single stick).
			if (_activeIndex != -2) return;

			Rect2 effectiveArea = GetViewport().GetVisibleRect();
			if (!effectiveArea.HasPoint(position)) return;

			_activeIndex = index;
			Origin = position;
			_lastPosition = position;
			CurrentPosition = position;
			IsActive = true;
			_accumulatedOffset = Vector2.Zero;

			EmitSignal(SignalName.JoystickEngaged, Origin);
			GetViewport().SetInputAsHandled();
		}
		else
		{
			if (_activeIndex != index) return;
			Release();
		}
	}

	private void Release()
	{
		_activeIndex = -2;
		IsActive = false;
		_accumulatedOffset = Vector2.Zero;
		_lastPosition = Vector2.Zero; // Reset position cache

		EmitSignal(SignalName.JoystickReleased);
	}
}