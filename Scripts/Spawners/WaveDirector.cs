using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public enum FormationShape
{
    Line, // 0
    Arc, // 1
    Grid, // 2
    VWedge // 3
}

public partial class WaveDirector : Node
{
	[Export] public FlightPaths FlightPaths { get; set; } // This is for enemies that follow a path. 
	// Contains all generic flight paths for enemies to follow.
	[Export] public SpawnPoints SpawnPoints { get; set; } // This is for enemies that follow the guiding points 
	// or the guiding point and then the player. Contains all generic spawn points for enemies to spawn at.
	[Export] public GuidingPoints GuidingPoints { get; set; } // This is for enemies to follow initially before 
	// they start following the player or exit point. Contains both guiding points and exit points for enemies to follow.
	
	[Export] public Node2D CharactersContainer { get; set; }

	[Export] public Godot.Collections.Array<EnemyData> RegisteredEnemyData { get; set; } = new Godot.Collections.Array<EnemyData>();
	private Dictionary<string, PackedScene> _enemyScenes = new Dictionary<string, PackedScene>();

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
		
		foreach(var data in RegisteredEnemyData)
		{
			if (data == null || string.IsNullOrEmpty(data.EnemyId) || data.EnemyScene == null)
			{
				GD.PrintErr("Wave director: skipping an EnemyData entry due to missing id or scene");
				continue;
			}
			_enemyScenes[data.EnemyId] = data.EnemyScene;
		}
	}

	private PackedScene ResolveScene(string enemyId)
	{
		if (_enemyScenes.TryGetValue(enemyId, out PackedScene scene))
        {
            return scene;
        }
        GD.PrintErr($"WaveDirector: no EnemyData registered for id '{enemyId}'.");
        return null;
	}

	// Spawn popcorn enemies that follow a path
	public void SpawnPopcornPath(string enemyId, FlightPathType flightPathType, int count, float delay)
	{
		PackedScene packedScene = ResolveScene(enemyId);
		Path2D path = FlightPaths?.GetFlightPath(flightPathType);

		if (packedScene == null || path == null || path.Curve.GetBakedPoints().Length <= 0)
        {
			GD.PrintErr("Wave director: Cannot spawn popcorn swarm of path type due to no scene or (invalid) path");
            return;
        }

		for (int i = 0; i < count; i++)
        {
            float myDelay = delay * i;
 
            if (delay <= 0f)
            {
				SpawnPopcornInternal(packedScene, path.ToGlobal(path.Curve.GetPointPosition(0)), path);
            }
            else
            {
                GetTree().CreateTimer(myDelay).Timeout += () => SpawnPopcornInternal(packedScene, path.ToGlobal(path.Curve.GetPointPosition(0)), path);
            }
        }
	}

	private void AddChildToContainer(BaseCharacter character)
	{
		if (CharactersContainer != null)
		{
			CharactersContainer.AddChild(character);
		}
		else
		{
			AddChild(character);
		}
	}

	// Spawn popcorn enemies that dives at guiding/exit points instead of following a fixed path
	public void SpawnPopcornSeek(string enemyId, Vector2 spawnPos, Stack<Vector2> pointsStack, int playerIndex = -1)
	{
		PackedScene scene = ResolveScene(enemyId);

		if (scene.Instantiate() is not PopcornEnemy popcorn)
		{
			GD.PrintErr("WaveDirector: EnemyData scene root is not a PopcornEnemy.");
			return;
		}

		AddChildToContainer(popcorn);
		popcorn.GlobalPosition = spawnPos;
		popcorn.SetupSeek(pointsStack, playerIndex);
	}

	// Spawn popcorn enemies in a formation that utilized seek movement system
	public void SpawnSeekFormation(string enemyId, SpawnPointType spawnPoint, Godot.Collections.Array<int> guidingPoints, ExitPointType exitPoint, FormationShape shape, int count, float spacing = 8f, float staggerDelay = 0.25f)
	{
		PackedScene scene = ResolveScene(enemyId);
        Vector2 spawnPointPos = SpawnPoints.GetSpawnPoint(spawnPoint).GlobalPosition;
		Vector2 exitPointPos = GuidingPoints.GetExitPoint(exitPoint).GlobalPosition;
		Stack<Vector2> targetPositions = new Stack<Vector2>();
		targetPositions.Push(exitPointPos);

		int playerIndex = -1;
		for (int i = guidingPoints.Count - 1; i >= 0; i--)
		{
			if ((GuidingPointType)guidingPoints[i] == GuidingPointType.Player)
			{
				playerIndex = i;
			}
			Vector2 tempPos = GuidingPoints.GetGuidingPoint((GuidingPointType)guidingPoints[i]);
			targetPositions.Push(tempPos);
		}

		if (scene == null || count <= 0)
        {
            return;
        }

		Vector2[] offsets = GenerateFormationOffsets(shape, count, spacing);

		for (int i = 0; i < count; i++)
        {
			Vector2 thisPos = spawnPointPos + offsets[i];
            float delay = staggerDelay * i;
 
            if (delay <= 0f)
            {
				SpawnPopcornSeek(enemyId, thisPos, new Stack<Vector2>(targetPositions.Reverse()), playerIndex);
            }
            else
            {
                GetTree().CreateTimer(delay).Timeout += () => SpawnPopcornSeek(enemyId, thisPos, new Stack<Vector2>(targetPositions.Reverse()), playerIndex);
            }
        }
	}

	private void SpawnPopcornInternal(PackedScene scene, Vector2 spawnGlobalPosition, Path2D path = null)
	{
		if (scene.Instantiate() is not PopcornEnemy popcorn)
		{
			GD.PrintErr("WaveDirector: EnemyData scene root is not a PopcornEnemy.");
			return;
		}

		AddChildToContainer(popcorn);
		popcorn.GlobalPosition = spawnGlobalPosition;
		if (path != null)
		{
			popcorn.SetupPath(path);
		}
	}

	private static Vector2[] GenerateFormationOffsets(FormationShape shape, int count, float spacing)
	{
		if (count <= 0) return System.Array.Empty<Vector2>();
		var offsets = new Vector2[count];

		switch(shape)
		{
			case FormationShape.Line:
				for (int i = 0; i < count; i++)
				{
					offsets[i] = new Vector2(i * spacing, 0f);
				}
				break;
			case FormationShape.Arc:
				float arcSpan = Mathf.Pi * 0.5f;
				float radius = spacing * count / arcSpan;
				float step = count > 1 ? arcSpan / (count - 1) : 0f;
				for (int i = 0; i < count; i++)
				{
					float angle = -arcSpan * 0.5f + step * i;
					offsets[i] = new Vector2(Mathf.Sin(angle), 1f - Mathf.Cos(angle)) * radius;
				}
				break;
			case FormationShape.Grid:
				int columns = Mathf.CeilToInt(Mathf.Sqrt(count));
				for(int i = 0; i < count; i++)
				{
					int row = i / columns;
					int col = i % columns;

					offsets[i] = new Vector2(col * spacing, row * spacing);
				}
				break;
			case FormationShape.VWedge:
				for (int i = 0; i < count; i++)
                {
                    int side = i % 2 == 0 ? 1 : -1;
                    int rank = i / 2;
                    offsets[i] = new Vector2(side * rank * spacing, rank * spacing);
                }
                break;
		}
		return offsets;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
