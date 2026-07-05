using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles gas tank health, damage processing,
/// destruction behavior, and save/load integration.
/// </summary>
public class GasTankHealth : MonoBehaviour, ISaveable {
    private static readonly string ID = "Objective";

    [SerializeField] private DeathScreen deathScreen;
    [SerializeField] private DayNightCycle dayNightCycle;
    [SerializeField] private GasTankHUD tankHUD;
    [SerializeField] private AudioSource generatorAudioSource;
    [SerializeField] private ParticleSystem explosion;

    [Header("Health")]
    [SerializeField] private int maxHP = 100;

    [Header("Generator")]
    [SerializeField] private float generatorVolume = 1f;

    private int currentHP;

    /// <summary>
    /// Gets the current health value.
    /// </summary>
    public int CurrentHP => currentHP;

    /// <summary>
    /// Gets the maximum health value.
    /// </summary>
    public int MaxHP => maxHP;

    public static event Action<Vector3> OnObjectiveDestroyed;

    /// <summary>
    /// Initializes the gas tank with full health.
    /// </summary>
    private void Awake() {
        currentHP = maxHP;
    }

    /// <summary>
    /// Starts generator audio when night begins.
    /// </summary>
    private void DayNightCycle_OnSunsetStarted() {
        generatorAudioSource.volume = generatorVolume * SoundManager.Instance.volume;
        generatorAudioSource.Play();
    }

    /// <summary>
    /// Stops generator audio when day begins.
    /// </summary>
    private void DayNightCycle_OnSunriseStarted() {
        generatorAudioSource.Stop();
    }

    /// <summary>
    /// Applies damage and triggers destruction if health reaches zero.
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

        if (currentHP <= 0) {
            OnDestroyed();
            OnObjectiveDestroyed?.Invoke(transform.position);
        }
    }

    /// <summary>
    /// Restores health up to the maximum value.
    /// </summary>
    public void Heal(int amount) {
        if (currentHP <= 0)
            return;

        currentHP += amount;

        if (currentHP > maxHP)
            currentHP = maxHP;
    }

    /// <summary>
    /// Executes destruction logic including effects,
    /// disabling the player, and triggering the death screen.
    /// </summary>
    private void OnDestroyed() {
        Debug.Log("Gas Tank destroyed!");

        if (generatorAudioSource != null) {
            generatorAudioSource.Stop();
        }

        if (explosion != null) {
            Instantiate(explosion, transform.position + new Vector3(0, 2, 0), Quaternion.identity);
        }

        if (deathScreen != null && dayNightCycle != null) {
            deathScreen.ShowDeathScreen(dayNightCycle.SurvivedNights);
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) {
            player.SetActive(false);
        }

        gameObject.SetActive(false);
    }

    /// <summary>
    /// Resets the gas tank to full health.
    /// </summary>
    public void ResetHP() {
        currentHP = maxHP;
    }

    #region Save/Load
    /// <summary>
    /// Returns the unique save identifier.
    /// </summary>
    public string GetSaveID() => ID;

    /// <summary>
    /// Creates a save data object containing current health.
    /// </summary>
    public object Save() {
        return new ObjectiveData {
            health = currentHP
        };
    }

    /// <summary>
    /// Restores health from saved data.
    /// </summary>
    public void Load(object data) {
        ObjectiveData objectiveData = (ObjectiveData)data;
        currentHP = objectiveData.health;
    }

    /// <summary>
    /// Serializable container for saved gas tank data.
    /// </summary>
    [Serializable]
    public class ObjectiveData {
        public int health;
    }
    #endregion

    /// <summary>
    /// Subscribes to events and registers for saving.
    /// </summary>
    private void OnEnable() {
        DayNightCycle.OnSunsetStarted += DayNightCycle_OnSunsetStarted;
        DayNightCycle.OnSunriseStarted += DayNightCycle_OnSunriseStarted;

        if (SaveManager.Instance != null)
            SaveManager.Instance.Register(this);
    }

    /// <summary>
    /// Unsubscribes from events and unregisters from saving.
    /// </summary>
    private void OnDisable() {
        DayNightCycle.OnSunsetStarted -= DayNightCycle_OnSunsetStarted;
        DayNightCycle.OnSunriseStarted -= DayNightCycle_OnSunriseStarted;

        if (SaveManager.Instance != null)
            SaveManager.Instance.Unregister(this);
    }
}