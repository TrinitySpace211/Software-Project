using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// PlayMode tests for the GasTankHealth system.
/// Validates health logic such as damage, healing, clamping, and reset behavior.
/// </summary>
public class GasTankHealthTests {
    private GameObject gameObject;
    private GasTankHealth gasTank;

    /// <summary>
    /// Creates a fresh GasTankHealth instance before each test.
    /// </summary>
    [SetUp]
    public void Setup() {
        gameObject = new GameObject();
        gasTank = gameObject.AddComponent<GasTankHealth>();

        gasTank.ResetHP();
    }

    /// <summary>
    /// Cleans up the created GameObject after each test to prevent memory leaks.
    /// </summary>
    [TearDown]
    public void TearDown() {
        Object.DestroyImmediate(gameObject);
    }

    /// <summary>
    /// Ensures that taking damage correctly reduces the current health.
    /// </summary>
    [UnityTest]
    public IEnumerator TakeDamage_ReducesHealth() {
        gasTank.TakeDamage(25);

        Assert.AreEqual(75, gasTank.CurrentHP);

        yield return null;
    }

    /// <summary>
    /// Ensures that health does not drop below zero when receiving high damage.
    /// </summary>
    [UnityTest]
    public IEnumerator TakeDamage_ClampsHealthToZero() {
        gasTank.TakeDamage(500);

        Assert.AreEqual(0, gasTank.CurrentHP);

        yield return null;
    }

    /// <summary>
    /// Ensures that healing correctly increases current health.
    /// </summary>
    [UnityTest]
    public IEnumerator Heal_IncreasesHealth() {
        gasTank.TakeDamage(50);

        gasTank.Heal(20);

        Assert.AreEqual(70, gasTank.CurrentHP);

        yield return null;
    }

    /// <summary>
    /// Ensures that healing does not exceed the maximum health value.
    /// </summary>
    [UnityTest]
    public IEnumerator Heal_DoesNotExceedMaxHealth() {
        gasTank.Heal(100);

        Assert.AreEqual(gasTank.MaxHP, gasTank.CurrentHP);

        yield return null;
    }

    /// <summary>
    /// Ensures that healing has no effect when the gas tank is destroyed (HP = 0).
    /// </summary>
    [UnityTest]
    public IEnumerator Heal_DoesNothingWhenDestroyed() {
        gasTank.TakeDamage(100);

        gasTank.Heal(50);

        Assert.AreEqual(0, gasTank.CurrentHP);

        yield return null;
    }

    /// <summary>
    /// Ensures that ResetHP restores the gas tank to full health.
    /// </summary>
    [UnityTest]
    public IEnumerator ResetHP_RestoresMaxHealth() {
        gasTank.TakeDamage(60);

        gasTank.ResetHP();

        Assert.AreEqual(gasTank.MaxHP, gasTank.CurrentHP);

        yield return null;
    }

    /// <summary>
    /// Ensures that multiple damage calls accumulate correctly over time.
    /// </summary>
    [UnityTest]
    public IEnumerator MultipleDamageCalls_AccumulateCorrectly() {
        gasTank.TakeDamage(20);
        gasTank.TakeDamage(15);
        gasTank.TakeDamage(10);

        Assert.AreEqual(55, gasTank.CurrentHP);

        yield return null;
    }
}