using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Options menu handling audio settings and saving/loading them via PlayerPrefs.
/// Supports separate control for Master, Music and SFX volume.
/// </summary>
public class OptionsMenu : MonoBehaviour {
    [Header("Master Audio")]
    /// Slider for controlling master volume (0–1 range).
    [SerializeField] private Slider masterVolumeSlider;

    /// Text that displays the current master volume as a percentage.
    [SerializeField] private TextMeshProUGUI masterVolumeValueText;

    [Header("Music Audio")]
    /// Slider for controlling music volume (0–1 range).
    [SerializeField] private Slider musicVolumeSlider;

    /// Text that displays the current music volume as a percentage.
    [SerializeField] private TextMeshProUGUI musicVolumeValueText;

    [Header("SFX Audio")]
    /// Slider for controlling sound effects volume (0–1 range).
    [SerializeField] private Slider sfxVolumeSlider;

    /// Text that displays the current SFX volume as a percentage.
    [SerializeField] private TextMeshProUGUI sfxVolumeValueText;

    /// Key used for saving/loading master volume from PlayerPrefs.
    private const string MasterVolumeKey = "master_volume";

    /// Key used for saving/loading music volume from PlayerPrefs.
    private const string MusicVolumeKey = "music_volume";

    /// Key used for saving/loading SFX volume from PlayerPrefs.
    private const string SfxVolumeKey = "sfx_volume";

    /// <summary>
    /// Loads saved volume settings when the menu is opened.
    /// </summary>
    private void Start() {
        LoadVolumes();
        masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxVolumeSlider.onValueChanged.AddListener(SetSfxVolume);
    }


    /// <summary>
    /// Sets the master volume, updates UI, and saves it.
    /// </summary>
    /// <param name="value">Volume value between 0 and 1.</param>
    public void SetMasterVolume(float value) {
        value = Mathf.Clamp01(value);

        PlayerPrefs.SetFloat(MasterVolumeKey, value);
        PlayerPrefs.Save();

        UpdateText(masterVolumeValueText, value);
        AudioListener.volume = value;
    }

    /// <summary>
    /// Sets the music volume, updates UI, and saves it.
    /// </summary>
    /// <param name="value">Volume value between 0 and 1.</param>
    public void SetMusicVolume(float value) {
        value = Mathf.Clamp01(value);

        PlayerPrefs.SetFloat(MusicVolumeKey, value);
        PlayerPrefs.Save();

        UpdateText(musicVolumeValueText, value);
        AudioListener.volume = value;


        ApplyToAllAudio();
    }

    /// <summary>
    /// Sets the sound effects volume, updates UI, and saves it.
    /// </summary>
    /// <param name="value">Volume value between 0 and 1.</param>
    public void SetSfxVolume(float value) {
        value = Mathf.Clamp01(value);

        PlayerPrefs.SetFloat(SfxVolumeKey, value);
        PlayerPrefs.Save();

        UpdateText(sfxVolumeValueText, value);
        AudioListener.volume = value;


        ApplyToAllAudio();
    }

    private void ApplyToAllAudio() {
        AudioListener.volume = MasterVolume;
    }

    /// <summary>
    /// Loads all saved volume settings and applies them to the UI.
    /// </summary>
    private void LoadVolumes() {
        float masterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
        float musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
        float sfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, 1f);

        if (masterVolumeSlider != null)
            masterVolumeSlider.SetValueWithoutNotify(masterVolume);

        if (musicVolumeSlider != null)
            musicVolumeSlider.SetValueWithoutNotify(musicVolume);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.SetValueWithoutNotify(sfxVolume);

        UpdateText(masterVolumeValueText, masterVolume);
        UpdateText(musicVolumeValueText, musicVolume);
        UpdateText(sfxVolumeValueText, sfxVolume);

        AudioListener.volume = masterVolume;
    }

    /// <summary>
    /// Updates a volume text field with the current percentage value.
    /// </summary>
    private void UpdateText(TextMeshProUGUI text, float value) {
        if (text != null)
            text.text = Mathf.RoundToInt(value * 100f) + "%";
    }

    /// <summary>
    /// Returns the currently saved master volume.
    /// </summary>
    public static float MasterVolume =>
        PlayerPrefs.GetFloat(MasterVolumeKey, 1f);

    /// <summary>
    /// Returns the currently saved music volume.
    /// </summary>
    public static float MusicVolume =>
        PlayerPrefs.GetFloat(MusicVolumeKey, 1f);

    /// <summary>
    /// Returns the currently saved sound effects volume.
    /// </summary>
    public static float SfxVolume =>
        PlayerPrefs.GetFloat(SfxVolumeKey, 1f);
}