using UnityEngine;

/// <summary>
///     Central script for the wave spawn system.
///     Called by DayNightCycle on new day and by the Start button on game start.
/// </summary>
public class WaveManager : MonoBehaviour {
    [Header("Spawn Zones")]
    [SerializeField]
    private SpawnZone[] spawnZones;

    [Header("Wave Scaling")]
    [SerializeField]
    private int baseZombieCount = 15;

    [SerializeField] private int zombiesPerDay = 5;
    [SerializeField] private int maxZombiesTotal = 60;

    [Header("Tank (ab Welle 6)")]
    [SerializeField] private int maxTanksTotal = 4;

    private int _currentDay;

    private void Awake() {
        LeanTween.init(800);
    }

    /// <summary>
    ///     Called by the Start button. Resets the day counter and spawns the first wave.
    /// </summary>
    [ContextMenu("Test Spawn")]
    public void OnGameStart() {
        _currentDay = 0;
        SpawnAllZones();
    }

    /// <summary>
    ///     Called by DayNightCycle when a new day begins. Increments day and spawns next wave.
    /// </summary>
    public void OnNewDay() {
        _currentDay++;
        SpawnAllZones();
    }

    /// <summary>
    ///     Distributes the total zombie count evenly across all spawn zones.
    /// </summary>
    private void SpawnAllZones() {
        var total = Mathf.Min(
            baseZombieCount + _currentDay * zombiesPerDay,
            maxZombiesTotal
        );

        // Ab Tag 2 (Welle 3): 1 Sprinter, danach +1 pro Welle bis max 8
        var totalSprinters = _currentDay >= 2
            ? Mathf.Min(_currentDay - 1, 8)
            : 0;

        // Ab Tag 5 (Welle 6): 1 Tank, danach +1 pro Welle bis max maxTanksTotal.
        // (Welle = Tag + 1, vgl. Sprinter-Kommentar "Tag 2 = Welle 3".)
        var totalTanks = _currentDay >= 5
            ? Mathf.Min(_currentDay - 4, maxTanksTotal)
            : 0;

        var perZone = total / spawnZones.Length;
        var remainder = total % spawnZones.Length;
        var sprintersPerZone = totalSprinters / spawnZones.Length;
        var tanksPerZone = totalTanks / spawnZones.Length;

        for (var i = 0; i < spawnZones.Length; i++) {
            spawnZones[i].zombieCount = perZone + (i == 0 ? remainder : 0);
            spawnZones[i].sprinterCount =
                sprintersPerZone + (i == 0 ? totalSprinters % spawnZones.Length : 0); // ← Rest auch verteilen
            spawnZones[i].tankCount =
                tanksPerZone + (i == 0 ? totalTanks % spawnZones.Length : 0);
            spawnZones[i].SpawnWave();
        }
    }

    private void ClearAllZombies() {
        foreach (SpawnZone zone in spawnZones) {
            zone.ClearZombies();
        }
    }
}