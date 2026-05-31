using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Unit tests for OptionsMenu.
/// Validates audio volume handling, UI updates, and clamping behavior.
/// </summary>
public class OptionsMenuTests {
    private GameObject _obj;
    private OptionsMenu _menu;
    private Slider _slider;
    private TextMeshProUGUI _text;

    /// <summary>
    /// Creates UI dependencies and injects them into OptionsMenu.
    /// </summary>
    [SetUp]
    public void Setup() {
        _obj = new GameObject("OptionsMenu");
        _menu = _obj.AddComponent<OptionsMenu>();

        GameObject sliderObj = new GameObject("Slider");
        _slider = sliderObj.AddComponent<Slider>();

        GameObject textObj = new GameObject("Text");
        _text = textObj.AddComponent<TextMeshProUGUI>();

        _menu.GetType().GetField("masterVolumeSlider",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(_menu, _slider);

        _menu.GetType().GetField("volumeValueText",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(_menu, _text);
    }

    /// <summary>
    /// Ensures that SetMasterVolume correctly updates global audio volume.
    /// </summary>
    [Test]
    public void SetMasterVolume_UpdatesAudioListener() {
        _menu.SetMasterVolume(0.5f);

        Assert.AreEqual(0.5f, AudioListener.volume, 0.01f);
    }

    /// <summary>
    /// Ensures that volume percentage text is updated correctly.
    /// </summary>
    [Test]
    public void SetMasterVolume_UpdatesText() {
        _menu.SetMasterVolume(0.75f);

        Assert.IsTrue(_text.text.Contains("75"));
    }

    /// <summary>
    /// Ensures that volume values are clamped between 0 and 1.
    /// </summary>
    [Test]
    public void SetMasterVolume_ClampsValue() {
        _menu.SetMasterVolume(2f);

        Assert.LessOrEqual(AudioListener.volume, 1f);
    }
}