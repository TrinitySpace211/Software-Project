using UnityEngine;

/// <summary>
/// Handles player health, damage and death logic.
/// </summary>
public class PlayerHealth : MonoBehaviour {

    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private BaseStats baseStats;
    [SerializeField] private DeathScreen deathScreen;
    [SerializeField] private DayNightCycle dayNightCycle;

    private bool isDead;
    private HealthBar healthBar;
    private PlayerAnimation playerAnimation;
    private PlayerIK playerIK;
    private PlayerGunSelector playerGunSelector;
    private Animator animator;

    /// <summary>
    /// Gets the player stats reference.
    /// </summary>
    public PlayerStats stats => playerStats;

    private void Start() {
        if (baseStats == null) {
            enabled = false;
            return;
        }

        if (animator == null)
            animator = GetComponent<Animator>();

        if (playerAnimation == null) {
            playerAnimation = GetComponent<PlayerAnimation>();
        }

        if (playerIK == null) {
            playerIK = GetComponent<PlayerIK>();
        }

        if (playerGunSelector == null) {
            playerGunSelector = GetComponent<PlayerGunSelector>();
        }

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

        //Debug.Log($"Player HP: {playerStats.currentHealth}/{playerStats.maxHealth}");

        if (healthBar != null)
            healthBar.UpdateHealthBar();

        playerAnimation.SetHitTrigger();

        if (playerStats.currentHealth <= 0)
            Die();
    }

    private void Die() {
        isDead = true;

        if (playerIK.GetHasWeapon()) {
            playerGunSelector.DieWithWeapon();
            playerAnimation.SetDyingWithWeaponTrigger();
        } else {
            playerAnimation.SetDyingTrigger();
        }

        deathScreen.ShowDeathScreen(dayNightCycle.SurvivedNights);
    }

    public bool GetIsDead() {
        return isDead;
    }
}