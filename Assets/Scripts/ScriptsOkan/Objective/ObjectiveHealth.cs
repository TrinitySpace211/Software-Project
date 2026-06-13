using System;
using UnityEngine;

/// <summary>
///     Placeholder health component for objectives that zombies attack at night.
/// </summary>
public class ObjectiveHealth : MonoBehaviour {
    [Header("Objective Settings")] public string objectiveName = "Objective";

    public int maxHealth = 500;

    private int _currentHealth;

    private void Start() {
        _currentHealth = maxHealth;
    }

    public static event Action OnObjectiveDestroyed;

    public void TakeDamage(int damage) {
        if (IsDestroyed()) return;

        _currentHealth -= damage;
        Debug.Log($"[Objective] {objectiveName}: {_currentHealth}/{maxHealth} HP");

        if (_currentHealth <= 0) {
            _currentHealth = 0;
            OnObjectiveDestroyed();
        }
    }

    private void OnObjectiveDestroyed() {
        Debug.Log($"[Objective] {objectiveName} wurde zerstört!");
        // TODO: Game Over Logik / Event
    }

    public bool IsDestroyed() {
        return _currentHealth <= 0;
    }
}