using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GasTankHUD : MonoBehaviour {
    [SerializeField] private GasTankHealth gasTank;

    [Header("UI")]
    [SerializeField] private Image healthFill;
    [SerializeField] private TMP_Text healthText;

    private float damageFlashTimer;

    private void Update() {
        if (gasTank == null)
            return;

        float percent = (float)gasTank.CurrentHP / gasTank.MaxHP;

        healthFill.fillAmount = Mathf.Lerp(
            healthFill.fillAmount,
            percent,
            Time.deltaTime * 8f
        );

        healthText.text =
            $"{gasTank.CurrentHP} / {gasTank.MaxHP}";

        Color targetColor;

        if (percent > 0.6f)
            targetColor = Color.green;
        else if (percent > 0.3f)
            targetColor = Color.yellow;
        else
            targetColor = Color.red;

        if (damageFlashTimer > 0f) {
            damageFlashTimer -= Time.deltaTime;

            healthFill.color = Color.white;
        } else {
            healthFill.color = Color.Lerp(
                healthFill.color,
                targetColor,
                Time.deltaTime * 10f
            );
        }

        if (percent <= 0.2f) {
            float pulse = 1f + Mathf.Sin(Time.time * 10f) * 0.03f;
            transform.localScale = Vector3.one * pulse;
        } else {
            transform.localScale =
                Vector3.Lerp(transform.localScale, Vector3.one, Time.deltaTime * 8f);
        }
    }
    public void OnTankDamaged() {
        damageFlashTimer = 0.2f;
    }
}