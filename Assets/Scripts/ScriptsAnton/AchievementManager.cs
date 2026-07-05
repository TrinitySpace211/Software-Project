using System;
using UnityEngine;

/// <summary>
/// Manages the Achivements
/// When the triggerAchievement gets invoked, 
/// then it will spawn the Achievement Card and 
/// trigger the Initialize() of the card
/// </summary>
public class AchievementManager : MonoBehaviour {
    public delegate void TriggerAchievement(AchievementSO achievementSO);
    public static TriggerAchievement triggerAchievement;

    public GameObject achievementCard;
    public Transform achievementTransform;

    private void Awake() {
        triggerAchievement += OnAchievementTriggered;
    }

    private void OnDestroy() {
        triggerAchievement -= OnAchievementTriggered;
    }

    /// <summary>
    /// Spawns the Card Prefab and initializes the card
    /// </summary>
    /// <param name="achievementSO">The Data necessary</param>
    private void OnAchievementTriggered(AchievementSO achievementSO) {
        GameObject card = Instantiate(achievementCard, achievementTransform);
        card.GetComponent<AchievementCard>().Initialize(achievementSO);
    }
}
