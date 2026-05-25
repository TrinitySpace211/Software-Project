using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionsMenu : MonoBehaviour {
    [Header("Audio")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private TextMeshProUGUI volumeValueText;

    private const string MasterVolumeKey = "master_volume";

    private void Start() {
        LoadVolume();
    }

    public void SetMasterVolume(float value) {
        value = Mathf.Clamp01(value);

        AudioListener.volume = value;
        PlayerPrefs.SetFloat(MasterVolumeKey, value);
        PlayerPrefs.Save();

        if (volumeValueText != null) {
            string percentText = Mathf.RoundToInt(value * 100f).ToString() + "%";
            volumeValueText.text = percentText;
        }
    }

    private void LoadVolume() {
        float volume = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
        AudioListener.volume = volume;

        if (masterVolumeSlider != null)
            masterVolumeSlider.SetValueWithoutNotify(volume);

        if (volumeValueText != null) {
            string percentText = Mathf.RoundToInt(volume * 100f).ToString() + "%";
            volumeValueText.text = percentText;
        }
    }
}