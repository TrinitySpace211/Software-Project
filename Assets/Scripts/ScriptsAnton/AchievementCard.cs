using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementCard : MonoBehaviour {
    public AchievementSO achievement;

    [SerializeField] private TextMeshProUGUI titleDisp;
    [SerializeField] private TextMeshProUGUI descriptionDisp;
    [SerializeField] private Image spriteDisp;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private RectMask2D mask;
    [SerializeField] private RectTransform contentTransform;

    private Vector4 padding;

    public float achievementVolume = 1f;

    public float waitBeforeExit = 3f;

    private void Start() {
        padding = mask.padding;
    }

    public void Initialize(AchievementSO achievement) {
        this.achievement = achievement;
        UpdateDisplay();

        StartCoroutine(WaitAndDestroy());
    }

    private void UpdateDisplay() {
        titleDisp.text = achievement.name;
        descriptionDisp.text = achievement.description;
        spriteDisp.sprite = achievement.sprite;

        audioSource.volume = achievementVolume * SoundManager.Instance.volume;
        audioSource.Play();
    }

    private IEnumerator WaitAndDestroy() {
        const float hiddenPadding = 0f;
        const float shownPadding = -550f;

        padding.z = hiddenPadding;
        mask.padding = padding;

        LeanTween.value(gameObject, hiddenPadding, shownPadding, 0.25f)
            .setEaseOutCubic()
            .setOnUpdate(value => {
                padding.z = value;
                mask.padding = padding;
            });

        yield return new WaitForSeconds(waitBeforeExit);

        LeanTween.value(gameObject, shownPadding, hiddenPadding, 0.25f)
            .setEaseInCubic()
            .setOnUpdate(value => {
                padding.z = value;
                mask.padding = padding;
            });

        yield return new WaitForSeconds(0.25f);

        Destroy(gameObject);
    }

}
