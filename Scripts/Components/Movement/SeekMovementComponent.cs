using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;

[GlobalClass]
public partial class SeekMovementComponent : MovementComponent
{
	[Export] public float ArrivalThreshold { get; set; } = 32f;

	private Stack<Vector2> _multiDestinationsStack = new Stack<Vector2>();
	private Vector2 _target;
	private bool _isMoving = false; 
	private int _playerIndex = -1;
	private int _currentIndex = 0;

	public void InsertStack(Stack<Vector2> multiDestinationsStack, int playerIndex = -1)
	{
		if (multiDestinationsStack.Count <= 0)
			return;
		_playerIndex = playerIndex;
		_multiDestinationsStack = multiDestinationsStack;
		SeekTo(_multiDestinationsStack.Pop()); 
	}

	public void SeekTo(Vector2 globalTarget)
	{
		_target = globalTarget;
		_isMoving = true;
	}

	public void Stop()
	{
		_isMoving = false;
		ComponentOwner.Velocity = Vector2.Zero;
	}

    public override void Move(double delta)
    {
        if (!_isMoving)
		{
			return;
		}
		
		if (_currentIndex == _playerIndex)
		{
			PlayerCharacter playerCharacter = PlayerService.Instance.PlayerCharacter;

			if (playerCharacter != null)
			{
				_target = playerCharacter.GlobalPosition;
			}
			else
			{
				// No player to chase right now — bail out of this leg instead of stalling forever
				SeekNext();
				return;
			}
		}

		Vector2 toTarget = _target - ComponentOwner.GlobalPosition;
		float distance = toTarget.Length();

		if (distance <= ArrivalThreshold)
		{
			ComponentOwner.GlobalPosition = _target;
			EmitSignal(SignalName.MovementCompleted);
			if (_playerIndex >= 0)
			{
				_currentIndex++;
			}
			return;
		}

		ComponentOwner.Velocity = toTarget.Normalized() * BaseSpeed;
		ComponentOwner.MoveAndSlide();
    }

	public void SeekNext()
	{
		if (_multiDestinationsStack.Count > 0)
		{
			SeekTo(_multiDestinationsStack.Pop()); 
		}
		else
		{
			Stop();
		}
	}
}
