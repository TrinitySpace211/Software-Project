using UnityEngine;

/// <summary>
///     Verwaltet die Lebenspunkte des Spielers.
///     TODO: maxHealth später aus PlayerStats beziehen.
/// </summary>
public class PlayerHealth : MonoBehaviour {
    private readonly int maxHealth = 100; // Hardcoded fürs testen
    private int _currentHealth;
    private bool _isDead;

    private void Start() {
        _currentHealth = maxHealth;
    }

    /// <summary>
    ///     Zieht dem Spieler Schadenspunkte ab.
    ///     Wird von ZombieAI beim Angriff aufgerufen.
    /// </summary>
    /// <param name="damage">Abzuziehender Schaden.</param>
    public void TakeDamage(int damage) {
        if (_isDead) return; // kein Schaden mehr nach Tod

        _currentHealth -= damage;
        _currentHealth = Mathf.Max(0, _currentHealth);
        Debug.Log($"Player HP: {_currentHealth}/{maxHealth}");

        if (_currentHealth <= 0)
            Die();
    }

    // TODO: GameOver-Logik einbauen
    private void Die() {
        _isDead = true;
        Debug.Log("Player gestorben");
    }
}