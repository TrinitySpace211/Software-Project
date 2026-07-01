using UnityEngine;

[CreateAssetMenu(fileName = "AchievementSO", menuName = "Achievements/New Achievement")]
public class AchievementSO : ScriptableObject {
    public new string name;
    public string description;

    public Sprite sprite;
}
