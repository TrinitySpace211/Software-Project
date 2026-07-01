using System;
using UnityEngine;

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

    private void OnAchievementTriggered(AchievementSO achievementSO) {
        GameObject card = Instantiate(achievementCard, achievementTransform);
        card.GetComponent<AchievementCard>().Initialize(achievementSO);
    }
}
