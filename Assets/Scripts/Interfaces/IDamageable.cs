public interface IDamageable
{
    void TakeDamage(float damage);
    void TakeDamageFromEnemy(float damage, string shooterName);
}