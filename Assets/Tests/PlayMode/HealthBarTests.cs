using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

/// <summary>
/// PlayMode tests for the HealthBar system.
/// Validates that the UI correctly reflects changes in player health.
/// </summary>
[TestFixture]
public class HealthBarTests {
    private GameObject _healthBarObject;
    private HealthBar _healthBar;
    private PlayerStats _stats;
    private Image _image;

    /// <summary>
    /// Sets up a fresh HealthBar, Image component, and PlayerStats instance
    /// before each test is executed.
    /// </summary>
    [SetUp]
    public void Setup() {
        _stats = new PlayerStats {
            maxHealth = 100,
            currentHealth = 100
        };

        _healthBarObject = new GameObject("HealthBar");

        _image = _healthBarObject.AddComponent<Image>();
        _image.type = Image.Type.Filled;
        _image.fillMethod = Image.FillMethod.Horizontal;
        _image.fillAmount = 1f;

        _healthBar = _healthBarObject.AddComponent<HealthBar>();

        _healthBarObject.SetActive(true);
    }

    /// <summary>
    /// Cleans up all created objects after each test
    /// to prevent interference between test cases.
    /// </summary>
    [TearDown]
    public void TearDown() {
        Object.DestroyImmediate(_healthBarObject);
        _stats = null;
    }

    /// <summary>
    /// Ensures that the health bar decreases visually
    /// when the player takes damage.
    /// </summary>
    [UnityTest]
    public IEnumerator HealthBar_Updates_When_Damage_Is_Received() {
        _healthBar.Initialize(_stats);

        float initialFill = _image.fillAmount;

        _stats.currentHealth -= 20;
        _healthBar.UpdateHealthBar();

        yield return null;

        float newFill = _image.fillAmount;

        Assert.Less(newFill, initialFill);
    }

    /// <summary>
    /// Ensures that the health bar shows zero fill when the player dies.
    /// </summary>
    [UnityTest]
    public IEnumerator HealthBar_Shows_Zero_When_Dead() {
        _healthBar.Initialize(_stats);

        _stats.currentHealth = 0;
        _healthBar.UpdateHealthBar();

        yield return null;

        float fill = _image.fillAmount;

        Assert.AreEqual(0f, fill, 0.01f);
    }

    /// <summary>
    /// Ensures that the health bar increases visually
    /// when the player is healed.
    /// </summary>
    [UnityTest]
    public IEnumerator HealthBar_Updates_On_Heal() {

        _healthBar.Initialize(_stats);

        _stats.currentHealth = 50;
        _healthBar.UpdateHealthBar();
        float afterDamage = _image.fillAmount;

        _stats.currentHealth = 70;
        _healthBar.UpdateHealthBar();

        yield return null;

        float afterHeal = _image.fillAmount;

        Assert.Greater(afterHeal, afterDamage);
    }
}