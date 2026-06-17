using UnityEngine;
using UnityEngine.AI;

/// <summary>
///     Verwaltet die aktuellen HP des Zombies zur Laufzeit.
/// </summary>
public class EnemyHealth : MonoBehaviour {
    private const string IS_DEAD = "isDead";
    public EnemyStatsSO stats;

    private Animator _animator;
    private int _currentHealth;

    private void Start() {
        _animator = GetComponent<Animator>();
        // Startwert aus SO laden
        _currentHealth = stats != null ? stats.maxHealth : 100;
    }

    /// <summary>Zieht dem Zombie Schadenspunkte ab.</summary>
    /// <param name="damage">Abzuziehender Schaden.</param>
    public void TakeDamage(int damage) {
        _currentHealth -= damage;
        if (_currentHealth <= 0)
            Die();
    }

    private void Die() {
        ZombieLootDrop lootDrop = GetComponent<ZombieLootDrop>();
        if (lootDrop == null) {
            lootDrop = gameObject.AddComponent<ZombieLootDrop>();
        }

        lootDrop.GiveLootToCurrentPlayer();

        if (_animator != null)
            _animator.SetBool(IS_DEAD, true);

        // NavMeshAgent deaktivieren damit der Zombie stehen bleibt
        var agent = GetComponent<NavMeshAgent>();
        if (agent != null)
            agent.enabled = false;

        // Destroy(gameObject, 3f); // TODO: nach Death-Animation einkommentieren
    }
}
