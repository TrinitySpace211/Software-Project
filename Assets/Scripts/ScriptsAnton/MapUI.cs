using UnityEngine;

/// <summary>
/// Map so that the player has some Orientation
/// </summary>
public class MapUI : MonoBehaviour {

    [SerializeField] private GameObject map;
    [SerializeField] private AudioSource mapOpenAudioSource;
    [SerializeField] private AudioClipRefsSO audioClipRefsSO;

    public PauseMenu pauseMenu;

    private bool mapOpen = false;

    /// <summary>
    /// Hides the Map at the Start
    /// </summary>
    private void Start() {
        PlayerInputHandler.OnMapOpenAction += PlayerInputHandler_OnMapOpenAction;
        HideMap();
    }

    /// <summary>
    /// When the Map Input get triggered then it will toggle the Map
    /// </summary>
    private void PlayerInputHandler_OnMapOpenAction() {
        if (pauseMenu.IsPaused || DebugController.Instance.GetConsoleVisibility())
            return;

        ShowHideMap();
        PauseGame();
    }

    /// <summary>
    /// Toggles the Map and plays the Sound Effect
    /// </summary>
    private void ShowHideMap() {
        bool isActive = map.activeInHierarchy;

        if (!isActive) {
            ShowMap();
            PlayMapSound();
        } else {
            HideMap();
            PlayMapSound();
        }
    }

    /// <summary>
    /// Shows the Map
    /// </summary>
    private void ShowMap() {
        map.SetActive(true);
        mapOpen = true;
    }

    /// <summary>
    /// Hide the Map
    /// </summary>
    private void HideMap() {
        map.SetActive(false);
        mapOpen = false;
    }

    /// <summary>
    /// Plays the Sound Effect
    /// </summary>
    private void PlayMapSound() {
        mapOpenAudioSource.clip = audioClipRefsSO.mapOpen[UnityEngine.Random.Range(0, audioClipRefsSO.mapOpen.Length)];
        mapOpenAudioSource.volume = audioClipRefsSO.mapOpenVolume * SoundManager.Instance.volume;
        mapOpenAudioSource.Play();
    }

    /// <summary>
    /// Pauses the Game while the Map is open and resumes it when it is closed
    /// </summary>
    private void PauseGame() {
        _ = !mapOpen ? Time.timeScale = 1f : Time.timeScale = 0f;
    }
}
