using UnityEngine;

/// <summary>
/// Makes a loot chest available again on each new day.
/// </summary>
[RequireComponent(typeof(LootChest), typeof(LootChestItemDrop))]
public class DailyLootChestReset : MonoBehaviour
{
    private const float NewDayDisplayTime = 5f;

    private DayNightCycle dayNightCycle;
    private LootChest lootChest;
    private LootChestItemDrop lootDrop;
    private float previousTimeOfDay;
    private bool hasPreviousTime;

    private void Awake()
    {
        // Gets both loot scripts from the same chest.
        lootChest = GetComponent<LootChest>();
        lootDrop = GetComponent<LootChestItemDrop>();
    }

    private void OnEnable()
    {
        FindDayNightCycle();
    }

    private void Update()
    {
        // Keeps searching if the day cycle was loaded later.
        if (dayNightCycle == null)
        {
            FindDayNightCycle();
            return;
        }

        CheckVisibleDayChange();
    }

    private void FindDayNightCycle()
    {
        DayNightCycle foundCycle = FindFirstObjectByType<DayNightCycle>();
        if (foundCycle == null)
        {
            return;
        }

        dayNightCycle = foundCycle;
        previousTimeOfDay = dayNightCycle.TimeOfDay;
        hasPreviousTime = true;
    }

    private void CheckVisibleDayChange()
    {
        float currentTime = dayNightCycle.TimeOfDay;

        if (!hasPreviousTime)
        {
            previousTimeOfDay = currentTime;
            hasPreviousTime = true;
            return;
        }

        // From 05:00 onward, the game display shows the next day.
        bool reachedNextDay = previousTimeOfDay < NewDayDisplayTime
            && currentTime >= NewDayDisplayTime;

        if (reachedNextDay)
        {
            ResetChestForNewDay();
        }

        previousTimeOfDay = currentTime;
    }

    private void ResetChestForNewDay()
    {
        // The chest receives new loot and glows yellow again.
        lootDrop.ResetForNewDay();
        lootChest.ResetForNewDay();
    }

    private void OnDisable()
    {
        dayNightCycle = null;
        hasPreviousTime = false;
    }
}
