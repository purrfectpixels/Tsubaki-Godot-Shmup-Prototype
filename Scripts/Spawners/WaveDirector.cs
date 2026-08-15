using Godot;
using System.Collections.Generic;

public enum FormationShape
{
    Line,   // 0
    Arc,    // 1
    Grid,   // 2
    VWedge, // 3
    Column,
    Circle
}

public partial class WaveDirector : Node
{
    [Export] public FlightPaths FlightPaths { get; set; }
    [Export] public SpawnPoints SpawnPoints { get; set; }
    [Export] public GuidingPoints GuidingPoints { get; set; }
    
    [Export] public Node2D CharactersContainer { get; set; }
    [Export] public Node2D MovingCharactersContainer { get; set; }

    public override void _Ready()
    {
        Callable.From(InitializationTest).CallDeferred();
    }

    private void InitializationTest()
    {
        int errorCount = 0;

        if (FlightPaths == null) { GD.PrintErr("FlightPaths is not assigned in WaveDirector."); errorCount++; }
        if (SpawnPoints == null) { GD.PrintErr("SpawnPoints is not assigned in WaveDirector."); errorCount++; }
        if (GuidingPoints == null) { GD.PrintErr("GuidingPoints is not assigned in WaveDirector."); errorCount++; }

        if (FlightPaths != null)
        {
            if (FlightPaths.UpperRightPath == null) { GD.PrintErr("UpperRightPath is not assigned in FlightPaths."); errorCount++; }
            if (FlightPaths.UpperLeftPath == null) { GD.PrintErr("UpperLeftPath is not assigned in FlightPaths."); errorCount++; }
            if (FlightPaths.LowerRightPath == null) { GD.PrintErr("LowerRightPath is not assigned in FlightPaths."); errorCount++; }
            if (FlightPaths.LowerLeftPath == null) { GD.PrintErr("LowerLeftPath is not assigned in FlightPaths."); errorCount++; }
        }

        if (SpawnPoints != null)
        {
            if (SpawnPoints.UpperRightSpawnpoint == null) { GD.PrintErr("UpperRightSpawnpoint is not assigned in SpawnPoints."); errorCount++; }
            if (SpawnPoints.UpperLeftSpawnpoint == null) { GD.PrintErr("UpperLeftSpawnpoint is not assigned in SpawnPoints."); errorCount++; }
            if (SpawnPoints.LowerRightSpawnpoint == null) { GD.PrintErr("LowerRightSpawnpoint is not assigned in SpawnPoints."); errorCount++; }
            if (SpawnPoints.LowerLeftSpawnpoint == null) { GD.PrintErr("LowerLeftSpawnpoint is not assigned in SpawnPoints."); errorCount++; }
        }

        if (GuidingPoints != null)
        {
            if (GuidingPoints.CenterLeftGuidingPoint == null) { GD.PrintErr("CenterLeftGuidingPoint is not assigned in GuidingPoints."); errorCount++; }
            if (GuidingPoints.CenterRightGuidingPoint == null) { GD.PrintErr("CenterRightGuidingPoint is not assigned in GuidingPoints."); errorCount++; }
            if (GuidingPoints.CenterLeftExitPoint == null) { GD.PrintErr("CenterLeftExitPoint is not assigned in GuidingPoints."); errorCount++; }
            if (GuidingPoints.CenterRightExitPoint == null) { GD.PrintErr("CenterRightExitPoint is not assigned in GuidingPoints."); errorCount++; }
        }

        if (EnemyPool.Instance == null)
        {
            GD.PrintErr("EnemyPool instance not found in scene.");
            errorCount++;
        }

        GD.Print($"WaveDirector initialization test completed with {errorCount} errors.");
    }

    private Node ResolveContainer()
	{
		Node container = CharactersContainer;
		if (container == null)
		{
			container = this;
		}
		return container;
	}


    public void SpawnPathWave(PathWaveData pathWaveData)
    {
        if (pathWaveData == null)
        {
            GD.PrintErr("WaveDirector: PathWaveData is null.");
            return;
        }

        if (ResolveNode(pathWaveData.PathNode) is not Path2D path2D)
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

    private void SpawnPopcornPath(string enemyId, Path2D path, int count, float delay)
    {
        if (path == null || path.Curve == null || path.Curve.GetBakedPoints().Length == 0)
        {
            GD.PrintErr("WaveDirector: Cannot spawn path popcorn due to invalid Path2D.");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            float myDelay = delay * i;
            Vector2 startPosition = path.ToGlobal(path.Curve.GetPointPosition(0));

            if (myDelay <= 0f)
            {
                SpawnPopcornInternal(enemyId, startPosition, path);
            }
            else
            {
                GetTree().CreateTimer(myDelay).Timeout += () => 
                    SpawnPopcornInternal(enemyId, startPosition, path);
            }
        }
    }

    public void SpawnPopcornSeek(string enemyId, Node2D spawn, Vector2 offset, Stack<Node2D> pointsStack, int playerIndex = -1)
    {
        if (spawn == null) return;

        PopcornEnemy popcorn = EnemyPool.Instance?.Spawn<PopcornEnemy>(enemyId, ResolveContainer(), spawn.GlobalPosition + offset);
        popcorn?.SetupSeek(pointsStack);
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

        var guidingPoints = new List<Node2D>();
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

    private void SpawnSeekersGroup(string enemyId, Node2D spawnPoint, List<Node2D> guidingPoints, Node2D exitPoint, FormationShape shape, int count, float spacing = 8f, float staggerDelay = 0.25f)
    {
        if (count <= 0) return;

        List<Node2D> rawNodes = new List<Node2D>();
        
        foreach (var point in guidingPoints)
        {
            if (GodotObject.IsInstanceValid(point)) rawNodes.Add(point);
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

    private void SpawnRigidFormationOnPath(string enemyId, NodePath pathNode, FormationShape shape, int count, float spacing = 16f, float pathSpeed = 150f, int directionFactor = -1)
    {
        Node2D pathTarget = ResolveNode(pathNode);

        if (pathTarget is not Path2D path2D || path2D.Curve == null || path2D.Curve.GetBakedPoints().Length == 0)
        {
            GD.PrintErr("WaveDirector: Cannot spawn rigid formation due to invalid Path2D.");
            return;
        }

        Vector2[] offsets = GenerateFormationOffsets(shape, count, spacing, directionFactor);

        for (int i = 0; i < count; i++)
        {
            SpawnRigidFormationMember(enemyId, path2D, offsets[i], pathSpeed);
        }
    }

    private void SpawnRigidFormationMember(string enemyId, Path2D path, Vector2 formationOffset, float pathSpeed)
    {
        PopcornEnemy popcorn = EnemyPool.Instance?.Spawn<PopcornEnemy>(enemyId, ResolveContainer(), path.GlobalPosition);
        popcorn?.SetupPath(path, formationOffset, pathSpeed);
    }

    public void SpawnDiveAndFleeWave(DiveWaveData diveWaveData)
    {
        if (diveWaveData == null)
        {
            GD.PrintErr("WaveDirector: DiveWaveData is null.");
            return;
        }

        Node2D spawnPoint = ResolveNode(diveWaveData.SpawnPoint);
        Node2D entryPoint = ResolveNode(diveWaveData.EntryPoint);
        Node2D exitPoint = ResolveNode(diveWaveData.ExitPoint);

		Vector2[] offset = GenerateFormationOffsets(diveWaveData.Shape, diveWaveData.Count, diveWaveData.Spacing);

        if (spawnPoint == null || entryPoint == null || exitPoint == null)
        {
            GD.PrintErr($"WaveDirector: Could not resolve Spawn/Entry/Exit point for dive wave '{diveWaveData.ResourcePath}'.");
            return;
        }

        for (int i = 0; i < diveWaveData.Count; i++)
        {
			int index = i;
            float delay = diveWaveData.StaggerDelay * i;

            if (delay <= 0f)
            {
                SpawnDiveEnemy(diveWaveData.EnemyId, spawnPoint, entryPoint, exitPoint, offset[index]);
            }
            else
            {
                GetTree().CreateTimer(delay).Timeout += () =>
                    SpawnDiveEnemy(diveWaveData.EnemyId, spawnPoint, entryPoint, exitPoint, offset[index]);
            }
        }
    }

    private void SpawnDiveEnemy(string enemyId, Node2D spawnPoint, Node2D entryPoint, Node2D exitPoint, Vector2 offset)
    {
        DiveEnemy diver = EnemyPool.Instance?.Spawn<DiveEnemy>(enemyId, ResolveContainer(), spawnPoint.GlobalPosition + offset);

        diver?.SetupDiveAndFlee(entryPoint, exitPoint, offset);
    }

    private void SpawnPopcornInternal(string enemyId, Vector2 spawnGlobalPosition, Path2D path = null)
    {
        PopcornEnemy popcorn = EnemyPool.Instance?.Spawn<PopcornEnemy>(enemyId, ResolveContainer(), spawnGlobalPosition);
        if (popcorn == null) return;

        if (path != null)
        {
            popcorn.SetupPath(path);
        }
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
        if (path == null) return null;

        Node currentScene = GetTree().CurrentScene;
        if (currentScene is MainGame mainGame)
        {
            Node2D currentLevel = mainGame.GetCurrentLevel();
            return currentLevel?.GetNodeOrNull<Node2D>(path);
        }

        GD.PrintErr("ERROR: CurrentScene is not MainGame.");
        return null;
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
                    int rank = (i + 1) / 2;
                    int side = (i % 2 == 1) ? -1 : 1;
                    offsets[i] = new Vector2(-rank * spacing, side * rank * spacing * directionFactor);
                }
                break;

            case FormationShape.Line:
                for (int i = 0; i < count; i++)
                {
                    float yOffset = (i - (count - 1) / 2.0f) * spacing;
                    offsets[i] = new Vector2(0f, yOffset);
                }
                break;

            case FormationShape.Column:
                for (int i = 0; i < count; i++)
                {
                    offsets[i] = new Vector2(-i * spacing, 0f);
                }
                break;

            case FormationShape.Grid:
                int cols = Mathf.CeilToInt(Mathf.Sqrt(count));
                for (int i = 0; i < count; i++)
                {
                    int row = i / cols;
                    int col = i % cols;
                    offsets[i] = new Vector2(-row * spacing, (col - (cols - 1) / 2.0f) * spacing);
                }
                break;

            case FormationShape.Circle:
                if (count == 1) { offsets[0] = Vector2.Zero; break; }
                float radius = (spacing * count) / (2f * Mathf.Pi);
                float angleStep = Mathf.Tau / count;
                for (int i = 0; i < count; i++)
                {
                    float angle = i * angleStep;
                    offsets[i] = new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
                }
                break;

            case FormationShape.Arc:
                if (count == 1) { offsets[0] = Vector2.Zero; break; }
                float arcAngleSpan = Mathf.DegToRad(120f);
                float angleStepArc = arcAngleSpan / (count - 1);
                float startAngle = -arcAngleSpan / 2f;
                float arcRadius = (spacing * (count - 1)) / arcAngleSpan;

                for (int i = 0; i < count; i++)
                {
                    float currentAngle = startAngle + (i * angleStepArc);
                    offsets[i] = new Vector2(
                        (Mathf.Cos(currentAngle) - 1f) * arcRadius,
                        Mathf.Sin(currentAngle) * arcRadius * directionFactor
                    );
                }
                break;

            default:
                for (int i = 0; i < count; i++) offsets[i] = Vector2.Zero;
                break;
        }

        return offsets;
    }
}