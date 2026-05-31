using UnityEngine;

/// <summary>
/// Handles player health, damage and death logic.
/// </summary>
public class PlayerHealth : MonoBehaviour {

    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private BaseStats baseStats;


    [SerializeField] private Animator animator;

// (added some code into scripts so the healthbar display the zombie damage properly)


    private bool isDead;
    private HealthBar healthBar;

    /// <summary>
    /// Gets the player stats reference.
    /// </summary>
    public PlayerStats Stats => playerStats;

    private void Start() {
        if (baseStats == null) {
            enabled = false;
            return;
        }



        if (animator == null)
            animator = GetComponent<Animator>();



        if (animator == null)
            animator = GetComponent<Animator>();


        playerStats = new PlayerStats {
            maxHealth = baseStats.health,
            currentHealth = baseStats.health,
            armor = baseStats.armor
        };

        healthBar = Object.FindFirstObjectByType<HealthBar>();
        if (healthBar != null)
            healthBar.Initialize(playerStats);
    }

    /// <summary>
    /// Applies damage to the player.
    /// </summary>
    public void TakeDamage(float damage) {
        if (isDead) return;

        if (playerStats == null) {
            return;
        }

        float finalDamage = Mathf.Max(0, damage - playerStats.armor);

        playerStats.currentHealth -= finalDamage;
        playerStats.currentHealth = Mathf.Clamp(playerStats.currentHealth, 0, playerStats.maxHealth);

        if (healthBar != null)
            healthBar.UpdateHealthBar();



        if (animator != null)
            animator.SetTrigger("GetHit");


 // (added some code into scripts so the healthbar display the zombie damage properly)


        if (animator != null)
            animator.SetTrigger("GetHit");


        if (playerStats.currentHealth <= 0)
            Die();
    }

    private void Die() {
        isDead = true;
        Debug.Log("Player died");
    }
}