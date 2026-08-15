using Godot;
using System.Collections.Generic;

// Object pool for enemies, mirroring ObjectPool's approach for bullets.
// Each EnemyData entry gets its own stack of pre-warmed instances keyed by EnemyId.
// Spawners (WaveDirector, etc.) should always go through Spawn<T>()/Return() rather
// than instantiating or QueueFree()-ing enemies directly.
[GlobalClass]
public partial class EnemyPool : Node2D
{
	public static EnemyPool Instance { get; private set; }

	private Godot.Collections.Array<EnemyData> _registeredEnemyData = new Godot.Collections.Array<EnemyData>();
	[Export] public Godot.Collections.Array<EnemyData> RegisteredEnemyData
	{
		get => _registeredEnemyData;
		set => _registeredEnemyData = value;
	}

	private Dictionary<string, Stack<BaseEnemy>> _enemyPools = new Dictionary<string, Stack<BaseEnemy>>();
	private Dictionary<string, PackedScene> _enemyScenes = new Dictionary<string, PackedScene>();

	public override void _Ready()
	{
		Instance = this;
		InitializeEnemyPools();
	}

	private void InitializeEnemyPools()
	{
		foreach (EnemyData enemyData in _registeredEnemyData)
		{
			if (enemyData == null || string.IsNullOrEmpty(enemyData.EnemyId) || enemyData.EnemyScene == null)
			{
				GD.PrintErr("EnemyPool: skipping an EnemyData entry due to missing id or scene");
				continue;
			}

			var enemyStack = new Stack<BaseEnemy>();
			_enemyPools[enemyData.EnemyId] = enemyStack;
			_enemyScenes[enemyData.EnemyId] = enemyData.EnemyScene;

			for (int i = 0; i < enemyData.InitialPoolSize; i++)
			{
				BaseEnemy enemyInstance = enemyData.EnemyScene.Instantiate<BaseEnemy>();
				enemyInstance.PoolId = enemyData.EnemyId;
				AddChild(enemyInstance);
				enemyInstance.ReturnToPool();
				enemyStack.Push(enemyInstance);
			}
		}
	}

	// Dispenses an enemy for the given id, re-parenting it under `parentContainer`
	// (or this pool node if none is given), positioning it, and bringing it back
	// to a fresh, alive state via BaseEnemy.ActivateFromPool().
	public T Spawn<T>(string enemyId, Node parentContainer, Vector2 globalPosition) where T : BaseEnemy
	{
		if (!_enemyPools.TryGetValue(enemyId, out Stack<BaseEnemy> enemyStack))
		{
			GD.PrintErr($"EnemyPool: no pool registered for id '{enemyId}'.");
			return null;
		}

		BaseEnemy enemy;
		if (enemyStack.Count > 0)
		{
			enemy = enemyStack.Pop();
		}
		else
		{
			enemy = _enemyScenes[enemyId].Instantiate<BaseEnemy>();
			enemy.PoolId = enemyId;
		}

		Node targetParent = parentContainer ?? this;
		if (enemy.GetParent() != targetParent)
		{
			enemy.GetParent()?.RemoveChild(enemy);
			targetParent.AddChild(enemy);
		}

		enemy.GlobalPosition = globalPosition;
		enemy.ActivateFromPool();

		if (enemy is not T typedEnemy)
		{
			GD.PrintErr($"EnemyPool: enemy for id '{enemyId}' is not of requested type {typeof(T).Name}.");
			return null;
		}

		return typedEnemy;
	}

	// Parks an enemy back in its pool (by PoolId) instead of freeing it. Enemies
	// should call BaseEnemy.Despawn() rather than this directly in most cases.
	public void Return(BaseEnemy enemy)
	{
		if (enemy == null) return;

		if (string.IsNullOrEmpty(enemy.PoolId) || !_enemyPools.TryGetValue(enemy.PoolId, out Stack<BaseEnemy> enemyStack))
		{
			enemy.QueueFree();
			return;
		}

		enemy.ReturnToPool();
		enemyStack.Push(enemy);
	}
}