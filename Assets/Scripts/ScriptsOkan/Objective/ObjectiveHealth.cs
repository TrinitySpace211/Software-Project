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

    private void HandleDestruction() {
        Debug.Log($"[Objective] {objectiveName} wurde zerstört!");
        OnObjectiveDestroyed?.Invoke(); // Event feuern
    }

    public void TakeDamage(int damage) {
        if (IsDestroyed()) return;
        _currentHealth -= damage;
        if (_currentHealth <= 0) {
            _currentHealth = 0;
            HandleDestruction();
        }
    }

    public bool IsDestroyed() {
        return _currentHealth <= 0;
    }
}