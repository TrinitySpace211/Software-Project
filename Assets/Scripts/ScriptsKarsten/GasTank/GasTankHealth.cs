using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Manages the gas tank's health and destruction state.
/// </summary>
public class GasTankHealth : MonoBehaviour, ISaveable {
    private static readonly string ID = "Objective";

    [SerializeField] private DeathScreen deathScreen;
    [SerializeField] private DayNightCycle dayNightCycle;
    [SerializeField] private GasTankHUD tankHUD;

    [Header("Health")]
    [SerializeField] private int maxHP = 100;

    private int currentHP;

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
    private void Awake() {
        currentHP = maxHP;
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

    #region Save/Load
    public string GetSaveID() => ID;

    public object Save() {
        return new ObjectiveData {
            health = currentHP
        };
    }

    public void Load(object data) {
        ObjectiveData objectiveData = (ObjectiveData)data;
        currentHP = objectiveData.health;
    }

    [Serializable]
    public class ObjectiveData {
        public int health;
    }
    #endregion

    private void OnEnable() {
        SaveManager.Instance.Register(this);
    }

    private void OnDisable() {
        if (SaveManager.Instance != null)
            SaveManager.Instance.Unregister(this);
    }
}