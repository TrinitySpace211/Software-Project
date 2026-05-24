using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class MenuManager : MonoBehaviour {
    [SerializeField] private string gameSceneName = "WorldScene";
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip musicClip;
    [SerializeField] private GameObject firstSelectedButton;

    [SerializeField] private CanvasGroup fadeGroup;
    [SerializeField] private float fadeDuration = 1f;

    private AudioSource audioSource;
    private AudioSource musicSource;
    private bool isTransitioning;

    private void Awake() {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.clip = musicClip;
        musicSource.loop = true;
        musicSource.playOnAwake = false;

        if (musicClip != null)
            musicSource.Play();

        if (fadeGroup != null) {
            fadeGroup.alpha = 0f;
            fadeGroup.blocksRaycasts = false;
        }
    }

    private void Start() {
        if (firstSelectedButton != null && EventSystem.current != null) {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        }

        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    public void NewGame() {
        if (isTransitioning) return;
        StartCoroutine(FadeAndLoadScene());
    }

    private IEnumerator FadeAndLoadScene() {
        isTransitioning = true;
        PlayClick();

        if (fadeGroup != null) {
            fadeGroup.blocksRaycasts = true;
            yield return Fade(0f, 1f);
        }

        yield return new WaitForSeconds(0.1f);
        SceneManager.LoadScene(gameSceneName);
    }

    private IEnumerator Fade(float from, float to) {
        float time = 0f;
        while (time < fadeDuration) {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / fadeDuration);
            fadeGroup.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }

        fadeGroup.alpha = to;
    }

    public void OnSettingsPressed() {
        PlayClick();
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void OnCloseSettingsPressed() {
        PlayClick();
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    public void ExitGame() {
        PlayClick();
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    private void PlayClick() {
        if (clickSound == null) return;
        audioSource.PlayOneShot(clickSound);
    }
}