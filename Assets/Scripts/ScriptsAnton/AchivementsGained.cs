using System.IO;
using UnityEngine;

/// <summary>
/// Checks in the Main Menu Scene the save files 
/// if the Player has collected the Achievements 
/// and displays them in the lower right corner
/// </summary>
public class AchivementsGained : MonoBehaviour {

    [SerializeField] private GameObject achievement1;
    [SerializeField] private GameObject achievement2;
    [SerializeField] private GameObject achievement3;

    bool achievement1Gained = false;
    bool achievement2Gained = false;
    bool achievement3Gained = false;

    /// <summary>
    /// Shows Achievements that got earned from playing
    /// </summary>
    private void Start() {
        if (File.Exists(Path.Combine(Application.persistentDataPath, "save.json"))) {
            DayNightCycle.DayNightData data = (DayNightCycle.DayNightData)SaveManager.Instance.LoadDataFromSave(DayNightCycle.ID);

            achievement1Gained = data.achievement1Gained;
            achievement2Gained = data.achievement2Gained;

            if (achievement1Gained)
                achievement1.SetActive(true);
            else
                achievement1.SetActive(false);

            if (achievement2Gained)
                achievement2.SetActive(true);
            else
                achievement2.SetActive(false);
        }
        if (File.Exists(Path.Combine(Application.persistentDataPath, $"{ExtractionController.ID}.json"))) {
            ExtractionController.ExtractionContollerData data = SaveManager.Instance.LoadData<ExtractionController.ExtractionContollerData>(ExtractionController.ID);

            achievement3Gained = data.achievement3Gained;

            if (achievement3Gained) achievement3.SetActive(true);
            else achievement3.SetActive(false);
        }
    }
}
