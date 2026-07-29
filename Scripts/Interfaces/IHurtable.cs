public interface IHurtable
{
    bool IsDead { get; }
    public void TakeDamage(float damage);
    public void OnHealthChanged(float health, float maxHP);
}