using Godot;

public interface IShootable
{
    public void Shoot(BulletData bulletData, Vector2 direction);
}