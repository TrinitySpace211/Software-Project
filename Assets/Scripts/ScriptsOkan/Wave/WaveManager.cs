using System.IO;
using UnityEngine;

public class WaveManager : MonoBehaviour, ISaveable {
    public static readonly string ID = "WaveManager";

    [Header("Spawn Zones")]
    [SerializeField]
    private SpawnZone[] spawnZones;

    [Header("Day 1 Early Action")]
    [SerializeField]
    private int day1MinZombies = 1;

    [SerializeField] private int day1MaxZombies = 3;

    private WaveDifficultyConfig _config;
    private int _currentDay;

    private void Awake() {
        LeanTween.init(800);
        _config = DifficultyPresets.Get(GameDifficulty.Selected);
    }

    //[ContextMenu("Test Spawn")]
    private void Start() {
        _config = DifficultyPresets.Get(GameDifficulty.Selected);

        if (File.Exists(Path.Combine(Application.persistentDataPath, "save.json"))) {
            WaveSaveData data = (WaveSaveData)SaveManager.Instance.LoadDataFromSave(ID);

            if (data.currentWave == 0) {
                _currentDay = 0;
                SpawnEarlyAction();
            }
        } else {
            _currentDay = 0;
            SpawnEarlyAction();
        }
    }

    public void OnNewDay() {
        _currentDay++;
        SpawnAllZones();
    }

    private void SpawnEarlyAction() {
        if (spawnZones == null || spawnZones.Length == 0) {
            return;
        }

        var selectedZoneCount = Mathf.Min(2, spawnZones.Length);
        var availableIndices = new System.Collections.Generic.List<int>();
        for (var i = 0; i < spawnZones.Length; i++) {
            availableIndices.Add(i);
        }

        for (var i = 0; i < selectedZoneCount; i++) {
            var randomIndex = Random.Range(0, availableIndices.Count);
            var zoneIndex = availableIndices[randomIndex];
            availableIndices.RemoveAt(randomIndex);

            var zone = spawnZones[zoneIndex];
            var earlyCount = Random.Range(day1MinZombies, day1MaxZombies + 1);

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
    }

    private void SpawnAllZones() {
        if (spawnZones == null || spawnZones.Length == 0) {
            Debug.LogWarning("WaveManager has no spawn zones assigned.");
            return;
        }

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

        var zoneCount = spawnZones.Length;
        var perZone = total / zoneCount;
        var remainder = total % zoneCount;
        var sprintersPerZone = totalSprinters / zoneCount;
        var sprinterRemainder = totalSprinters % zoneCount;
        var tanksPerZone = totalTanks / zoneCount;
        var tankRemainder = totalTanks % zoneCount;

        for (var i = 0; i < zoneCount; i++) {
            spawnZones[i].zombieCount = perZone + (i < remainder ? 1 : 0);
            spawnZones[i].sprinterCount = sprintersPerZone + (i < sprinterRemainder ? 1 : 0);
            spawnZones[i].tankCount = tanksPerZone + (i < tankRemainder ? 1 : 0);
            //spawnZones[i].ClearZombies();
            spawnZones[i].SpawnWave();
        }

        //Debug.Log($"Wave {(_currentDay + 1)} spawned: normals={total}, sprinters={totalSprinters}, tanks={totalTanks}");
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

    public string GetSaveID() => ID;

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

    private void OnEnable() {
        if (SaveManager.Instance != null)
            SaveManager.Instance.Register(this);
    }

    private void OnDisable() {
        if (SaveManager.Instance != null)
            SaveManager.Instance.Unregister(this);
    }
}