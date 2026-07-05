using UnityEngine;

/// <summary>
/// The necessary Data for the Achievements
/// </summary>
[CreateAssetMenu(fileName = "AchievementSO", menuName = "Achievements/New Achievement")]
public class AchievementSO : ScriptableObject {
    public new string name;
    public string description;

    public Sprite sprite;
}
