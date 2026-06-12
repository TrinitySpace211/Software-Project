using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Manages the gas tank's health and destruction state.
/// </summary>
public class GasTankHealth : MonoBehaviour {
    [SerializeField] private DeathScreen deathScreen;
    [SerializeField] private DayNightCycle dayNightCycle;
    [SerializeField] private GasTankHUD tankHUD;

    [Header("Health")]
    [SerializeField] private int maxHP = 100;
    [SerializeField] private int currentHP;

    /// <summary>
    /// Current health value of the gas tank.
    /// </summary>
    public int CurrentHP => currentHP;

    /// <summary>
    /// Maximum health value of the gas tank.
    /// </summary>
    public int MaxHP => maxHP;

    /// <summary>
    /// Initializes the health value.
    /// </summary>
    void Start() {
        if (currentHP <= 0)
            currentHP = maxHP;
    }

    /// <summary>
    /// Handles debug health input.
    /// </summary>
    void Update() {
        if (Keyboard.current.tKey.wasPressedThisFrame) {
            TakeDamage(10);
        }

        if (Keyboard.current.hKey.wasPressedThisFrame) {
            Heal(10);
        }

        if (Keyboard.current.rKey.wasPressedThisFrame) {
            ResetHP();
        }
    }

    /// <summary>
    /// Applies damage to the gas tank.
    /// </summary>
    public void TakeDamage(int damage) {
        if (currentHP <= 0)
            return;

        currentHP -= damage;

        if (currentHP < 0)
            currentHP = 0;

        if (tankHUD != null) {
            tankHUD.OnTankDamaged();
        }

        if (currentHP == 0) {
            OnDestroyed();
        }
    }

    /// <summary>
    /// Restores health to the gas tank.
    /// </summary>
    public void Heal(int amount) {
        if (currentHP <= 0)
            return;

        currentHP += amount;

        if (currentHP > maxHP)
            currentHP = maxHP;
    }

    /// <summary>
    /// Handles the gas tank destruction sequence.
    /// </summary>
    private void OnDestroyed() {
        Debug.Log("Gas Tank destroyed!");

        Time.timeScale = 0f;

        if (deathScreen != null && dayNightCycle != null) {
            deathScreen.ShowDeathScreen(dayNightCycle.SurvivedNights);
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) {
            player.SetActive(false);
        }
    }

    /// <summary>
    /// Restores the gas tank to full health.
    /// </summary>
    public void ResetHP() {
        currentHP = maxHP;
    }
}