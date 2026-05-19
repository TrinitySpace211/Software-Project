using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

[TestFixture]
public class HealthBarTests {

    private GameObject _healthBarObject;
    private HealthBar _healthBar;
    private PlayerStats _stats;
    private Image _image;

    [SetUp]
    public void Setup() {

        // --- STATS ---
        BaseStats baseStats = ScriptableObject.CreateInstance<BaseStats>();
        baseStats.health = 100;
        baseStats.armor = 10;

        StatsMediator mediator = new StatsMediator();
        _stats = new PlayerStats(mediator, baseStats);

        // --- UI OBJECT ---
        _healthBarObject = new GameObject("HealthBar");

        _image = _healthBarObject.AddComponent<Image>();
        _image.type = Image.Type.Filled;
        _image.fillMethod = Image.FillMethod.Horizontal;
        _image.fillAmount = 1f;

        _healthBar = _healthBarObject.AddComponent<HealthBar>();
    }

    [UnityTest]
    public IEnumerator HealthBar_Updates_When_Damage_Is_Received() {

        // Arrange
        _healthBar.Initialize(_stats);

        float initialFill = _image.fillAmount;

        // Act
        _stats.ChangeHealth(-20);
        _healthBar.UpdateHealthBar();

        yield return null;

        float newFill = _image.fillAmount;

        // Assert
        Assert.Less(newFill, initialFill, "HealthBar sollte bei Schaden kleiner werden");
    }

    [UnityTest]
    public IEnumerator HealthBar_Shows_Zero_When_Dead() {

        // Arrange
        _healthBar.Initialize(_stats);

        // Act
        _stats.ChangeHealth(-999);
        _healthBar.UpdateHealthBar();

        yield return null;

        float fill = _image.fillAmount;

        // Assert
        Assert.AreEqual(0f, fill, 0.01f, "HealthBar muss 0 sein wenn Spieler tot ist");
    }

    [UnityTest]
    public IEnumerator HealthBar_Updates_On_Heal() {

        // Arrange
        _healthBar.Initialize(_stats);

        _stats.ChangeHealth(-50);
        _healthBar.UpdateHealthBar();

        float afterDamage = _image.fillAmount;

        // Act
        _stats.ChangeHealth(20);
        _healthBar.UpdateHealthBar();

        yield return null;

        float afterHeal = _image.fillAmount;

        // Assert
        Assert.Greater(afterHeal, afterDamage, "HealthBar sollte bei Heilung steigen");
    }

    [TearDown]
    public void TearDown() {

        if (_healthBarObject != null) {
            Object.DestroyImmediate(_healthBarObject);
        }

        _stats = null;
        _healthBar = null;
        _image = null;
    }
}