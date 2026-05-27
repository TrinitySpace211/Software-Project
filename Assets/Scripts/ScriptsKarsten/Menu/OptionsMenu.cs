using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Options menu handling basic audio settings and saving/loading them via PlayerPrefs.
/// </summary>
public class OptionsMenu : MonoBehaviour {
    /// Slider for controlling master volume (0–1 range).
    [Header("Audio")]
    [SerializeField] private Slider masterVolumeSlider;

    /// Text that displays the current volume as a percentage.
    [SerializeField] private TextMeshProUGUI volumeValueText;

    /// Key used for saving/loading volume from PlayerPrefs.
    private const string MasterVolumeKey = "master_volume";

    /// <summary>
    /// Loads saved volume settings when the menu is opened.
    /// </summary>
    private void Start() {
        LoadVolume();
    }

    /// <summary>
    /// Sets the master volume, applies it globally, updates UI, and saves it.
    /// </summary>
    /// <param name="value">Volume value between 0 and 1.</param>
    public void SetMasterVolume(float value) {
        value = Mathf.Clamp01(value);

        AudioListener.volume = value;

        PlayerPrefs.SetFloat(MasterVolumeKey, value);
        PlayerPrefs.Save();

        if (volumeValueText != null) {
            string percentText = Mathf.RoundToInt(value * 100f) + "%";
            volumeValueText.text = percentText;
        }
    }

    /// <summary>
    /// Loads the saved master volume and applies it to audio and UI elements.
    /// </summary>
    private void LoadVolume() {
        float volume = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);

        AudioListener.volume = volume;

        if (masterVolumeSlider != null)
            masterVolumeSlider.SetValueWithoutNotify(volume);

        if (volumeValueText != null) {
            string percentText = Mathf.RoundToInt(volume * 100f) + "%";
            volumeValueText.text = percentText;
        }
    }
}