using Godot;
using System;

public partial class PlayerService : Node
{
	public static PlayerService Instance { get; private set; }

	// Direct reference to the player character
	public PlayerCharacter PlayerCharacter { get; private set; }

	[Signal]
	public delegate void PlayerRegisteredEventHandler(PlayerCharacter playerCharacter);
	[Signal]
	public delegate void PlayerUnregisteredEventHandler();

	public override void _Ready()
	{
		Instance = this;
	}

	public void RegisterPlayer(PlayerCharacter playerCharacter)
	{
		PlayerCharacter = playerCharacter;
		EmitSignal(SignalName.PlayerRegistered, playerCharacter);
	}

	public void UnregisterPlayer()
	{
		if (PlayerCharacter != null)
		{
			PlayerCharacter = null;
			EmitSignal(SignalName.PlayerUnregistered);
		}
	}
}
