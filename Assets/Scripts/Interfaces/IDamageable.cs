
/// <summary>
/// Interface to determine what script can be damaged
/// </summary>
public interface IDamageable {
    void TakeDamage(int damage);
    bool IsDead();
}