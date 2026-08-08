using System.Linq;
using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Concurrent;

[GlobalClass]
public partial class ObjectPool : Node2D
{
	public static ObjectPool Instance { get; private set; }
	private Godot.Collections.Array<BulletData> _registeredBulletData = new Godot.Collections.Array<BulletData>();
	[Export] public Godot.Collections.Array<BulletData> RegisteredBulletData
	{
		get => _registeredBulletData;
		set => _registeredBulletData = value;
	}
	private Dictionary<string, Stack<BaseBullet>> _bulletPools = new Dictionary<string, Stack<BaseBullet>>();
	private Dictionary<string, PackedScene> _bulletScenes = new Dictionary<string, PackedScene>();
	private List<BaseBullet> _activeBullets = new List<BaseBullet>();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Instance = this;
		InitializeBulletPools();
	}

	private void InitializeBulletPools()
	{
		foreach (var bulletData in _registeredBulletData)
		{
			var bulletStack = new Stack<BaseBullet>();
			_bulletPools[bulletData.BulletName] = bulletStack;
			_bulletScenes[bulletData.BulletName] = bulletData.BulletScene;

			for (int i = 0; i < bulletData.InitialPoolSize; i++)
			{
				var bulletInstance = bulletData.BulletScene.Instantiate<BaseBullet>();
				bulletInstance.Reset();
				AddChild(bulletInstance);
				bulletStack.Push(bulletInstance);
			}
		}
	}

	public T SpawnBullet<T>(BulletData bulletData, Vector2 position, Vector2 direction, Team team = Team.Player) where T : BaseBullet
	{
		if(!_bulletPools.ContainsKey(bulletData.BulletName))
			return null;
		
		Stack<BaseBullet> bulletStack = _bulletPools[bulletData.BulletName];
		BaseBullet bullet;
		if(bulletStack.Count > 0)
		{
			bullet = bulletStack.Pop();
		}
		else
		{
			bullet = _bulletScenes[bulletData.BulletName].Instantiate<BaseBullet>();
		}
		if (bullet.GetParent() == null)
		{
			AddChild(bullet);
		}
		bullet.Initialize(bulletData, position, direction, team);
		_activeBullets.Add(bullet);

		return bullet as T;
	}

	public void ReturnBullet(BaseBullet bullet, string bulletName)
	{
		bullet.Reset();
		if(_bulletPools.ContainsKey(bulletName))
		{
			_activeBullets.Remove(bullet);
			_bulletPools[bulletName].Push(bullet);
		}
	}

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;
		for(int i = _activeBullets.Count - 1; i >= 0; i--)
		{
			_activeBullets[i].ManualUpdate(dt);
		}
    }

}
