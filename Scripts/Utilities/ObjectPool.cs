using System.Linq;
using Godot;
using Godot.Collections;

[GlobalClass]
public partial class ObjectPool : Node2D
{
	public static ObjectPool Instance { get; private set; }
	private Array<BulletData> _registeredBulletData = new Array<BulletData>();
	[Export] public Array<BulletData> RegisteredBulletData
	{
		get => _registeredBulletData;
		set => _registeredBulletData = value;
	}
	private Dictionary<string, Array<BaseBullet>> _bulletPools = new Dictionary<string, Array<BaseBullet>>();
	private Dictionary<string, PackedScene> _bulletScenes = new Dictionary<string, PackedScene>();

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
			var bulletArray = new Array<BaseBullet>();
			_bulletPools[bulletData.BulletName] = bulletArray;
			_bulletScenes[bulletData.BulletName] = bulletData.BulletScene;

			for (int i = 0; i < bulletData.InitialPoolSize; i++)
			{
				var bulletInstance = bulletData.BulletScene.Instantiate<BaseBullet>();
				bulletInstance.Reset();
				AddChild(bulletInstance);
				bulletArray.Add(bulletInstance);
			}
		}
	}

	public T SpawnBullet<T>(BulletData bulletData, Vector2 position, Vector2 direction) where T : BaseBullet
	{
		if(!_bulletPools.ContainsKey(bulletData.BulletName))
			return null;
		
		Array<BaseBullet> array = _bulletPools[bulletData.BulletName];
		BaseBullet bullet;
		if(array.Count() > 0)
		{
			bullet = array.First();
			array.RemoveAt(0);
		}
		else
		{
			bullet = _bulletScenes[bulletData.BulletName].Instantiate<BaseBullet>();
		}
		if (bullet.GetParent() == null)
		{
			AddChild(bullet);
		}
		bullet.Initialize(bulletData, position, direction);
		GD.Print("Spawned bullet: ", bulletData.BulletName, " at position: ", position);

		return bullet as T;
	}

	public void ReturnBullet(BaseBullet bullet, string bulletName)
	{
		bullet.Reset();
		if(_bulletPools.ContainsKey(bulletName))
		{
			_bulletPools[bulletName].Add(bullet);
			GD.Print("Returned bullet: ", bulletName, " to pool. Pool size: ", _bulletPools[bulletName].Count());
		}
	}
}
