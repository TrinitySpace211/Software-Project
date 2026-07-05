using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
///     Manages all objectives in the scene.
///     Called by DayNightCycle events to switch zombie targets at night and back to player during day.
/// </summary>
public class ObjectiveManager : MonoBehaviour {

    public static ObjectiveManager Instance { get; private set; }

    [Header("Player")][SerializeField] private Transform player;
    [Header("Objectives")] public GasTankHealth[] objectives;
    [Header("Zones")][SerializeField] private List<SpawnZone> spawnZones;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    /// <summary>
    ///     Returns a random active (non-destroyed) objective.
    ///     Returns null if all objectives are destroyed.
    /// </summary>
    public Transform GetRandomActiveObjective() {
        var active = Array.FindAll(objectives, o => o != null && o.CurrentHP > 0);
        if (active.Length == 0) return null;
        return active[Random.Range(0, active.Length)].transform;
    }

    public Transform GetNearestActiveObjective(Vector3 position) {
        Transform nearest = null;
        var minDist = float.MaxValue;

        foreach (var o in objectives) {
            if (o == null || o.CurrentHP <= 0) continue;

            var dist = Vector3.Distance(position, o.transform.position);
            if (dist < minDist) {
                minDist = dist;
                nearest = o.transform;
            }
        }

        return nearest;
    }

    /// <summary>
    ///     Called by DayNightCycle.onNightStarted.
    ///     Redirects all active zombies to attack a random objective.
    /// </summary>
    public void OnNightStarted() {
        var redirected = 0;

        foreach (SpawnZone zone in spawnZones) {
            List<ZombieAI> zombies = zone.GetZombies();
            List<SprinterController> sprinters = zone.GetSprinters();
            List<TankZombieController> tanks = zone.GetTanks();

            foreach (ZombieAI zombie in zombies) {
                zombie.SetRageEyes();
                if (zombie.IsDead()) continue;
                var objective = GetRandomActiveObjective();
                if (objective == null) break;
                zombie.SetTarget(objective, true);
                redirected++;
            }

            foreach (SprinterController sprinter in sprinters) {
                if (sprinter.isDead) continue;
                var objective = GetRandomActiveObjective();
                if (objective == null) break;
                sprinter.SetTarget(objective, true);
                redirected++;
            }

            foreach (TankZombieController tank in tanks) {
                if (tank.isDead) continue;
                var objective = GetRandomActiveObjective();
                if (objective == null) break;
                tank.SetTarget(objective, true);
                redirected++;
            }
        }

        Debug.Log($"[ObjectiveManager] Nacht: {redirected} Zombies greifen Objectives an.");
    }

    /// <summary>
    ///     Called byS DayNightCycle.onNewDayStarted.
    ///     was supposed to: Redirects all zombies back to the player.
    /// </summary>
    public void OnDayStarted() {
        // Zombies behalten ihr Ziel bis sie sterben.
        foreach (SpawnZone zone in spawnZones) {
            foreach (ZombieAI zombie in zone.GetZombies()) {
                zombie.SetDefaultEyes();
            }
        }
    }
}