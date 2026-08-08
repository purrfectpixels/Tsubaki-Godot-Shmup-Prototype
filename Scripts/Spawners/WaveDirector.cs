using Godot;
using System;

public partial class WaveDirector : Node
{
	[Export] public FlightPaths FlightPaths { get; set; } // This is for enemies that follow a path. 
	// Contains all generic flight paths for enemies to follow.
	[Export] public SpawnPoints SpawnPoints { get; set; } // This is for enemies that follow the guiding points 
	// or the guiding point and then the player. Contains all generic spawn points for enemies to spawn at.
	[Export] public GuidingPoints GuidingPoints { get; set; } // This is for enemies to follow initially before 
	// they start following the player or exit point. Contains both guiding points and exit points for enemies to follow.
	
	private void InitalizationTest()
	{
		int errorCount = 0;
		if (FlightPaths == null)
		{
			GD.PrintErr("FlightPaths is not assigned in WaveDirector.");
			errorCount++;
		}
		if (SpawnPoints == null)
		{
			GD.PrintErr("SpawnPoints is not assigned in WaveDirector.");
			errorCount++;
		}
		if (GuidingPoints == null)
		{
			GD.PrintErr("GuidingPoints is not assigned in WaveDirector.");
			errorCount++;
		}

		if (FlightPaths.UpperRightPath == null)
		{
			GD.PrintErr("UpperRightPath is not assigned in FlightPaths.");
			errorCount++;
		}
		if (FlightPaths.UpperLeftPath == null)
		{
			GD.PrintErr("UpperLeftPath is not assigned in FlightPaths.");
			errorCount++;
		}
		if (FlightPaths.LowerRightPath == null)
		{
			GD.PrintErr("LowerRightPath is not assigned in FlightPaths.");
			errorCount++;
		}
		if (FlightPaths.LowerLeftPath == null)
		{
			GD.PrintErr("LowerLeftPath is not assigned in FlightPaths.");
			errorCount++;
		}
		if (SpawnPoints.UpperRightSpawnpoint == null)
		{
			GD.PrintErr("UpperRightSpawnpoint is not assigned in SpawnPoints.");
			errorCount++;
		}
		if (SpawnPoints.UpperLeftSpawnpoint == null)
		{
			GD.PrintErr("UpperLeftSpawnpoint is not assigned in SpawnPoints.");
			errorCount++;
		}
		if (SpawnPoints.LowerRightSpawnpoint == null)
		{
			GD.PrintErr("LowerRightSpawnpoint is not assigned in SpawnPoints.");
			errorCount++;
		}
		if (SpawnPoints.LowerLeftSpawnpoint == null)
		{
			GD.PrintErr("LowerLeftSpawnpoint is not assigned in SpawnPoints.");
			errorCount++;
		}
		if (GuidingPoints.CenterLeftGuidingPoint == null)
		{
			GD.PrintErr("CenterLeftGuidingPoint is not assigned in GuidingPoints.");
			errorCount++;
		}
		if (GuidingPoints.CenterRightGuidingPoint == null)
		{
			GD.PrintErr("CenterRightGuidingPoint is not assigned in GuidingPoints.");
			errorCount++;
		}
		if (GuidingPoints.CenterLeftExitPoint == null)
		{
			GD.PrintErr("CenterLeftExitPoint is not assigned in GuidingPoints.");
			errorCount++;
		}
		if (GuidingPoints.CenterRightExitPoint == null)
		{
			GD.PrintErr("CenterRightExitPoint is not assigned in GuidingPoints.");
			errorCount++;
		}
		GD.Print($"WaveDirector initialization test completed with {errorCount} errors.");
	}	
	public override void _Ready()
	{
		InitalizationTest();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
