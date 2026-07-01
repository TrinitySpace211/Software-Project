using UnityEngine;

public class WaveManager : MonoBehaviour {
    [Header("Spawn Zones")] [SerializeField]
    private SpawnZone[] spawnZones;

    [Header("Day 1 Early Action")] [SerializeField]
    private int day1MinZombies = 1;

    [SerializeField] private int day1MaxZombies = 3;

    private WaveDifficultyConfig _config;
    private int _currentDay;

    private void Awake() {
        LeanTween.init(800);
        _config = DifficultyPresets.Get(GameDifficulty.Selected);
    }

    [ContextMenu("Test Spawn")]
    public void OnGameStart() {
        _currentDay = 0;
        SpawnEarlyAction();
        SpawnAllZones();
    }

    public void OnNewDay() {
        _currentDay++;
        SpawnAllZones();
    }

    private void SpawnEarlyAction() {
        var earlyCount = Random.Range(day1MinZombies, day1MaxZombies + 1);
        var zone = spawnZones[Random.Range(0, spawnZones.Length)];
        zone.zombieCount = 0;
        zone.sprinterCount = 0;
        zone.tankCount = 0;

        // Spielmodus beruecksichtigen: In den "Only"-Modi spawnt auch die
        // Tag-1-Early-Action den passenden Zombie-Typ.
        switch (GameMode.Selected) {
            case GameModeType.OnlySprinter:
                zone.sprinterCount = earlyCount;
                break;
            case GameModeType.OnlyTanks:
                zone.tankCount = earlyCount;
                break;
            default:
                zone.zombieCount = earlyCount;
                break;
        }

        zone.SpawnWave();
    }

    private void SpawnAllZones() {
        var total = Mathf.Min(
            _config.baseZombieCount + _currentDay * _config.zombiesPerDay,
            _config.maxZombiesTotal
        );

        var totalSprinters = _currentDay >= _config.sprinterStartDay
            ? Mathf.Min(_currentDay - (_config.sprinterStartDay - 1), _config.maxSprinters)
            : 0;

        var totalTanks = _currentDay >= _config.tankStartDay
            ? Mathf.Min(_currentDay - (_config.tankStartDay - 1), _config.maxTanksTotal)
            : 0;

        ApplyGameMode(ref total, ref totalSprinters, ref totalTanks);

        var perZone = total / spawnZones.Length;
        var remainder = total % spawnZones.Length;
        var sprintersPerZone = totalSprinters / spawnZones.Length;
        var tanksPerZone = totalTanks / spawnZones.Length;

        for (var i = 0; i < spawnZones.Length; i++) {
            spawnZones[i].zombieCount = perZone + (i == 0 ? remainder : 0);
            spawnZones[i].sprinterCount =
                sprintersPerZone + (i == 0 ? totalSprinters % spawnZones.Length : 0);
            spawnZones[i].tankCount =
                tanksPerZone + (i == 0 ? totalTanks % spawnZones.Length : 0);
            spawnZones[i].SpawnWave();
        }
    }

    /// <summary>
    ///     Wendet den gewaehlten Spielmodus auf die Zombie-Zusammensetzung an.
    ///     In den "Only"-Modi wird die normale Zombie-Anzahl komplett in den
    ///     jeweiligen Typ umgeleitet, im Easy Mode bleiben nur normale Zombies.
    ///     Die Gesamtzahl skaliert weiterhin ueber die gewaehlte Schwierigkeit.
    /// </summary>
    private void ApplyGameMode(ref int normals, ref int sprinters, ref int tanks) {
        switch (GameMode.Selected) {
            case GameModeType.OnlySprinter:
                sprinters = normals;
                normals = 0;
                tanks = 0;
                break;
            case GameModeType.OnlyTanks:
                tanks = normals;
                normals = 0;
                sprinters = 0;
                break;
            case GameModeType.EasyMode:
                sprinters = 0;
                tanks = 0;
                break;
        }
    }

    private void ClearAllZombies() {
        foreach (var zone in spawnZones) zone.ClearZombies();
    }

    // --- Vorbereitet für ISaveable ---

    public string GetSaveID() {
        return "WaveManager";
    }

    public object Save() {
        return new WaveSaveData {
            currentWave = _currentDay,
            difficulty = GameDifficulty.Selected
        };
    }

    public void Load(object data) {
        if (data is WaveSaveData save) {
            _currentDay = save.currentWave;
            GameDifficulty.Selected = save.difficulty;
            _config = DifficultyPresets.Get(GameDifficulty.Selected);
        }
    }
}