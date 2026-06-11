using UnityEngine;
using UnityEngine.InputSystem;

public class GasTankHealth : MonoBehaviour {
    [SerializeField] private DeathScreen deathScreen;
    [SerializeField] private DayNightCycle dayNightCycle;
    [SerializeField] private GasTankHUD tankHUD;
    [Header("Health")]
    [SerializeField] private int maxHP = 100;
    [SerializeField] private int currentHP;

    public int CurrentHP => currentHP;
    public int MaxHP => maxHP;

    void Start() {

          if (currentHP <= 0)
             currentHP = maxHP;


    }

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

    public void Heal(int amount) {
        if (currentHP <= 0)
            return;

        currentHP += amount;

        if (currentHP > maxHP)
            currentHP = maxHP;

    }

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

 
    public void ResetHP() {
        currentHP = maxHP;
    }
}