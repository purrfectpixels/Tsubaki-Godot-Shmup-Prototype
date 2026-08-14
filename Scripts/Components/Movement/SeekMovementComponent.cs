using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;

[GlobalClass]
public partial class SeekMovementComponent : MovementComponent
{
	[Export] public float ArrivalThreshold { get; set; } = 32f; // Distance at which player's position gets "locked in"
	[Export] public float FinalArrivalThreshold { get; set; } = 4f; // Distance considered "fully arrived" at a target

	private Stack<Node2D> _multiDestinationsStack = new Stack<Node2D>();
	private Node2D _target;
	private bool _isMoving = false;

	private bool _playerPositionLocked = false;
	private Vector2 _lockedTargetPosition;

    public Node2D CurrentTarget => _target;

	public void InsertStack(Stack<Node2D> multiDestinationsStack, int playerIndex = -1)
	{
		if (multiDestinationsStack == null || multiDestinationsStack.Count <= 0)
            return;

        _multiDestinationsStack = multiDestinationsStack;
        SeekNext();
	}

	public void SeekTo(Node2D targetNode)
	{
		if (!GodotObject.IsInstanceValid(targetNode))
        {
            SeekNext();
            return;
        }

        _target = targetNode;
        _isMoving = true;
        _playerPositionLocked = false;
        _lockedTargetPosition = Vector2.Zero;
	}

	public void Stop()
	{
		_isMoving = false;
        _target = null;
        _playerPositionLocked = false;
        _lockedTargetPosition = Vector2.Zero;
        if (ComponentOwner != null)
        {
            ComponentOwner.Velocity = Vector2.Zero;
        }
	}

    public override void Move(double delta)
    {
        if (!_isMoving) return;

        if (!GodotObject.IsInstanceValid(_target))
        {
            SeekNext();
            return;
        }

        Vector2 currentTargetPos = _target.GlobalPosition;

        if (_target is PlayerCharacter)
        {
            if (!_playerPositionLocked)
            {
                PlayerCharacter playerCharacter = PlayerService.Instance?.PlayerCharacter;
                if (GodotObject.IsInstanceValid(playerCharacter))
                {
                    currentTargetPos = playerCharacter.GlobalPosition;
                }
                else
                {
                    SeekNext();
                    return;
                }

                float distanceToPlayer = (currentTargetPos - ComponentOwner.GlobalPosition).Length();
                if (distanceToPlayer <= ArrivalThreshold)
                {
                    // Lock in the player's last known position instead of continuing to track them live.
                    _playerPositionLocked = true;
                    _lockedTargetPosition = currentTargetPos;
                }
            }

            if (_playerPositionLocked)
            {
                currentTargetPos = _lockedTargetPosition;
            }
        }

        Vector2 toTarget = currentTargetPos - ComponentOwner.GlobalPosition;
        float distance = toTarget.Length();

        if (distance <= FinalArrivalThreshold)
        {
            EmitSignal(SignalName.MovementCompleted);
            return;
        }

        ComponentOwner.Velocity = toTarget.Normalized() * BaseSpeed;
        ComponentOwner.MoveAndSlide();
    }

	public void SeekNext()
    {
        while (_multiDestinationsStack != null && _multiDestinationsStack.Count > 0)
        {
            Node2D nextNode = _multiDestinationsStack.Pop();
            if (GodotObject.IsInstanceValid(nextNode))
            {
                SeekTo(nextNode);
                return;
            }
        }

        Stop();
    }
}