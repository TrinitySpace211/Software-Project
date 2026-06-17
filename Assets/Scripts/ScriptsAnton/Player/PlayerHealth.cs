using System;
using UnityEngine;

/// <summary>
/// Handles player health, damage and death logic.
/// </summary>
public class PlayerHealth : MonoBehaviour {

    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private BaseStats baseStats;
    [SerializeField] private DeathScreen deathScreen;
    [SerializeField] private DayNightCycle dayNightCycle;

    private bool isDead = false;
    private HealthBar healthBar;
    private PlayerAnimation playerAnimation;
    private PlayerIK playerIK;
    private PlayerWeaponSelector playerGunSelector;
    private Animator animator;

    /// <summary>
    /// Gets the player stats reference.
    /// </summary>
    public PlayerStats stats => playerStats;

    public static Action<Vector3> OnTakeDamage;
    public static Action<Vector3> OnDeath;

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
            playerGunSelector = GetComponent<PlayerWeaponSelector>();
        }

        playerStats = new PlayerStats {
            maxHealth = baseStats.health,
            currentHealth = baseStats.health,
            armor = baseStats.armor
        };

        healthBar = FindFirstObjectByType<HealthBar>();
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

        OnTakeDamage?.Invoke(transform.position);

        LeanTween.value(gameObject, playerStats.currentHealth, playerStats.currentHealth - finalDamage, 0.25f)
        .setOnUpdate((float hp) => {
            playerStats.currentHealth = hp;
        });
        playerStats.currentHealth = Mathf.Clamp(playerStats.currentHealth, 0, playerStats.maxHealth);

        //Debug.Log($"Player HP: {playerStats.currentHealth}/{playerStats.maxHealth}");

        if (healthBar != null)
            healthBar.UpdateHealthBar();

        playerAnimation.SetHitTrigger();

        if (playerStats.currentHealth <= 0)
            Die();
    }

    /// <summary>
    /// The Player Death Animation starts and the Weapon 
    /// inverse Kinematics are cleared so nothing weird happens
    /// </summary>
    private void Die() {
        isDead = true;

        if (playerIK.GetHasWeapon()) {
            playerGunSelector.ClearSetupCurrentWeapon();
            playerAnimation.SetDyingWithWeaponTrigger();
        } else {
            playerAnimation.SetDyingTrigger();
        }

        OnDeath?.Invoke(transform.position);
        deathScreen.ShowDeathScreen(dayNightCycle.SurvivedNights);
    }

    /// <summary>
    /// Heals the Player by a certain amount
    /// </summary>
    /// <param name="health">The amount to heal</param>
    public void HealPlayerHealth(float health) {
        float startHp = playerStats.currentHealth;
        float targetHp = Mathf.Min(playerStats.currentHealth + health, playerStats.maxHealth);

        LeanTween.value(gameObject, startHp, targetHp, 0.25f)
        .setOnUpdate((float hp) => {
            playerStats.currentHealth = hp;
        });

        //Debug.Log($"Healed by: {health}");
    }

    /// <summary>
    /// Getter to Check if the Player is Dead
    /// </summary>
    /// <returns>true if the Player is Dead, false otherwise</returns>
    public bool GetIsDead() {
        return isDead;
    }
}