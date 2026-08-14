using Godot;
using GodotPlugins.Game;
using System;
using System.Collections.Generic;
using System.Linq;

public enum FormationShape
{
    Line, // 0
    Arc, // 1
    Grid, // 2
    VWedge, // 3
	Column,
	Circle
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
	[Export] public Node2D MovingCharactersContainer { get; set; }

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

	public void SpawnPathWave(PathWaveData pathWaveData)
	{
		if (pathWaveData == null)
		{
			GD.PrintErr("WaveDirector: PathWaveData is null.");
			return;
		}

		Node2D pathNode = ResolveNode(pathWaveData.PathNode);
		if (pathNode is not Path2D path2D)
		{
			GD.PrintErr($"WaveDirector: Could not resolve Path2D at '{pathWaveData.PathNode}'.");
			return;
		}

		SpawnPopcornPath(
			pathWaveData.EnemyId, 
			path2D, 
			pathWaveData.Count, 
			pathWaveData.Delay
		);
	}

	// Spawn popcorn enemies that follow a path
	private void SpawnPopcornPath(string enemyId, Path2D path, int count, float delay)
	{
		PackedScene packedScene = ResolveScene(enemyId);

		if (packedScene == null || path == null || path.Curve == null || path.Curve.GetBakedPoints().Length == 0)
		{
			GD.PrintErr("WaveDirector: Cannot spawn path popcorn due to missing scene or invalid Path2D.");
			return;
		}

		for (int i = 0; i < count; i++)
		{
			float myDelay = delay * i;
			Vector2 startPosition = path.ToGlobal(path.Curve.GetPointPosition(0));

			if (myDelay <= 0f)
			{
				SpawnPopcornInternal(packedScene, startPosition, path);
			}
			else
			{
				GetTree().CreateTimer(myDelay).Timeout += () => 
					SpawnPopcornInternal(packedScene, startPosition, path);
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
	public void SpawnPopcornSeek(string enemyId, Node2D spawn, Vector2 offset, Stack<Node2D> pointsStack, int playerIndex = -1)
	{
		PackedScene scene = ResolveScene(enemyId);

		if (scene.Instantiate() is not PopcornEnemy popcorn)
		{
			GD.PrintErr("WaveDirector: EnemyData scene root is not a PopcornEnemy.");
			return;
		}

		AddChildToContainer(popcorn);
		popcorn.GlobalPosition = spawn.GlobalPosition + offset;
		popcorn.SetupSeek(pointsStack);
	}

	private Stack<Node2D> CreateNodeStack(List<Node2D> nodesInOrder)
	{
		Stack<Node2D> stack = new Stack<Node2D>();
        for (int i = nodesInOrder.Count - 1; i >= 0; i--)
        {
            if (GodotObject.IsInstanceValid(nodesInOrder[i]))
            {
                stack.Push(nodesInOrder[i]);
            }
        }
        return stack;
	}

	private Node2D ResolveNode(NodePath path)
	{
		Node currentScene = GetTree().CurrentScene;

		Node2D currentLevel = null;

		if (currentScene is MainGame mainGame)
		{
			currentLevel = mainGame.GetCurrentLevel();
		}
		else
		{
			GD.PrintErr("ERROR: DO NOT ASSIGN ANYTHING ELSE EXCEPT MAIN GAME AS CURRENT SCENE!");
		}

		if (currentLevel == null)
		{
			GD.PrintErr("Cannot resolve node of path: ", path);
			return null;
		}

		Node2D newNode = currentLevel.GetNode<Node2D>(path);

		return newNode;
	}

	public void SpawnSeekers(WaveData waveData)
	{
		if (waveData == null)
		{
			GD.PrintErr("WaveDirector: WaveData is null.");
			return;
		}

		Node2D spawnPoint = ResolveNode(waveData.spawnPoint);
		Node2D exitPoint = ResolveNode(waveData.exitPoint);

		if (spawnPoint == null || exitPoint == null)
		{
			GD.PrintErr($"WaveDirector: Could not resolve Spawn or Exit point for wave '{waveData.ResourcePath}'.");
			return;
		}
		var guidingPoints = new Godot.Collections.Array<Node2D>();
		if (waveData.guidingPoints != null)
		{
			foreach (NodePath path in waveData.guidingPoints)
			{
				Node2D pointNode = ResolveNode(path);
				if (pointNode != null)
				{
					guidingPoints.Add(pointNode);
				}
			}
		}
		SpawnSeekersGroup(waveData.enemyId, spawnPoint, guidingPoints, exitPoint, waveData.formationShape, waveData.count, waveData.spacing, waveData.staggerDelay);
	}

	// Spawn popcorn enemies in a group that utilized seek movement system
	private void SpawnSeekersGroup(string enemyId, Node2D spawnPoint, Godot.Collections.Array<Node2D> guidingPoints, Node2D exitPoint, FormationShape shape, int count, float spacing = 8f, float staggerDelay = 0.25f)
	{
		PackedScene scene = ResolveScene(enemyId);
        if (scene == null || count <= 0) return;

        List<Node2D> rawNodes = new List<Node2D>();
        
        for (int i = 0; i < guidingPoints.Count; i++)
        {
            if (GodotObject.IsInstanceValid(guidingPoints[i]))
            {
                rawNodes.Add(guidingPoints[i]);
            }
        }

        if (GodotObject.IsInstanceValid(exitPoint))
        {
            rawNodes.Add(exitPoint);
        }

		Vector2[] offsets = GenerateFormationOffsets(shape, count, spacing);

        for (int i = 0; i < count; i++)
        {
            float delay = staggerDelay * i;
            Vector2 offset = offsets[i];

            if (delay <= 0f)
            {
                Stack<Node2D> nodeStack = CreateNodeStack(rawNodes);
                SpawnPopcornSeek(enemyId, spawnPoint, offset, nodeStack);
            }
            else
            {
                GetTree().CreateTimer(delay).Timeout += () => 
                {
                    Stack<Node2D> nodeStack = CreateNodeStack(rawNodes);
                    SpawnPopcornSeek(enemyId, spawnPoint, offset, nodeStack);
                };
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

	public void SpawnRigidFormationWave(RigidFormationWaveData formationData)
	{
		if (formationData == null)
		{
			GD.PrintErr("WaveDirector: RigidFormationData is null.");
			return;
		}

		SpawnRigidFormationOnPath(
			formationData.EnemyId,
			formationData.PathNode,
			formationData.Shape,
			formationData.Count,
			formationData.Spacing,
			formationData.PathSpeed,
			formationData.DirectionFactor
		);
	}

	// Spawns enemies attached to a rigid formation moving along a live Path2D.
	// Every member is locked to the SAME path progress and speed via PathLockedMovementComponent,
	// each holding its own formation offset, so the group travels and turns together as a single
	// rigid shape. Each member still keeps its own PathPercentShootAttack, so they independently
	// fire at the player (via their ShootTriggerPercents) while marching in formation.
	private void SpawnRigidFormationOnPath(string enemyId, NodePath pathNode, FormationShape shape, int count, float spacing = 16f, float pathSpeed = 150f, int directionFactor = -1)
	{
		PackedScene packedScene = ResolveScene(enemyId);
		Node2D pathTarget = ResolveNode(pathNode);
 
		if (packedScene == null || pathTarget is not Path2D path2D || path2D.Curve == null || path2D.Curve.GetBakedPoints().Length == 0)
		{
			GD.PrintErr("WaveDirector: Cannot spawn rigid formation due to missing scene or invalid Path2D.");
			return;
		}
 
		Vector2[] offsets = GenerateFormationOffsets(shape, count, spacing, directionFactor);
 
		// No stagger delay: the formation must all start moving in the same frame, otherwise
		// members would sit at different points along the path and the shape would fall apart.
		for (int i = 0; i < count; i++)
		{
			SpawnRigidFormationMember(packedScene, path2D, offsets[i], pathSpeed);
		}
	}
 
	private void SpawnRigidFormationMember(PackedScene scene, Path2D path, Vector2 formationOffset, float pathSpeed)
	{
		if (scene.Instantiate() is not PopcornEnemy popcorn)
		{
			GD.PrintErr("WaveDirector: EnemyData scene root is not a PopcornEnemy.");
			return;
		}
 
		AddChildToContainer(popcorn);
		popcorn.SetupPath(path, formationOffset, pathSpeed);
	}

	private Vector2[] GenerateFormationOffsets(FormationShape shape, int count, float spacing, int directionFactor = -1)
	{
		Vector2[] offsets = new Vector2[count];
		if (count <= 0) return offsets;

		switch (shape)
		{
			case FormationShape.VWedge:
				offsets[0] = Vector2.Zero;

				for (int i = 1; i < count; i++)
				{
					int rank = (i + 1) / 2;           // Step distance behind apex
					int side = (i % 2 == 1) ? -1 : 1; // Lateral spreading (-Y / +Y)

					// -X ALWAYS trails behind the apex along the curve's direction of travel.
					float xOffset = -rank * spacing; 
					// Apply directionFactor to Y if you need to mirror left/right wings.
					float yOffset = side * rank * spacing * directionFactor; 

					offsets[i] = new Vector2(xOffset, yOffset);
				}
				break;

			case FormationShape.Line: // Horizontal wing line perpendicular to flight direction
				for (int i = 0; i < count; i++)
				{
					// Center the line across Y axis
					float yOffset = (i - (count - 1) / 2.0f) * spacing;
					offsets[i] = new Vector2(0f, yOffset);
				}
				break;

			case FormationShape.Column: // Single file trail behind leader
				for (int i = 0; i < count; i++)
				{
					offsets[i] = new Vector2(-i * spacing, 0f);
				}
				break;

			case FormationShape.Grid:
				int cols = Mathf.CeilToInt(Mathf.Sqrt(count));
				for (int i = 0; i < count; i++)
				{
					int row = i / cols; // Depth (-X)
					int col = i % cols; // Lateral (-Y to +Y)

					float xOffset = -row * spacing;
					float yOffset = (col - (cols - 1) / 2.0f) * spacing;

					offsets[i] = new Vector2(xOffset, yOffset);
				}
				break;

			case FormationShape.Circle:
				if (count == 1)
				{
					offsets[0] = Vector2.Zero;
					break;
				}

				// Calculate radius dynamically to fit spacing
				float radius = (spacing * count) / (2f * Mathf.Pi);
				float angleStep = Mathf.Tau / count;

				for (int i = 0; i < count; i++)
				{
					float angle = i * angleStep;
					// Cos -> X (front/back), Sin -> Y (left/right)
					offsets[i] = new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
				}
				break;
			case FormationShape.Arc:
				if (count == 1)
					{
						offsets[0] = Vector2.Zero;
						break;
					}

					float arcAngleSpan = Mathf.DegToRad(120f); 
					float angleStepArc = arcAngleSpan / (count - 1);
					float startAngle = -arcAngleSpan / 2f; 
					float arcRadius = (spacing * (count - 1)) / arcAngleSpan;

					for (int i = 0; i < count; i++)
					{
						float currentAngle = startAngle + (i * angleStepArc);
						
						// (Mathf.Cos - 1f) is negative, ensuring wingmen bow backwards along -X.
						float xOffset = (Mathf.Cos(currentAngle) - 1f) * arcRadius; 
						float yOffset = Mathf.Sin(currentAngle) * arcRadius * directionFactor;

						offsets[i] = new Vector2(xOffset, yOffset);
					}
					break;

						default:
							// Fallback stack on top of origin
							for (int i = 0; i < count; i++)
							{
								offsets[i] = Vector2.Zero;
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
