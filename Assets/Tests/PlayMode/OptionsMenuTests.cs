using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Reflection;

/// <summary>
/// NUnit tests for OptionsMenu.
/// These tests only verify audio and text behavior.
/// </summary>
public class OptionsMenuTests {
    private GameObject _obj;
    private OptionsMenu _menu;

    private Slider _masterSlider;
    private TextMeshProUGUI _masterText;

    [SetUp]
    public void Setup() {
        PlayerPrefs.DeleteAll();
        AudioListener.volume = 1f;

        _obj = new GameObject("OptionsMenu");
        _menu = _obj.AddComponent<OptionsMenu>();

        var sliderObj = new GameObject("MasterSlider");
        _masterSlider = sliderObj.AddComponent<Slider>();

        var textObj = new GameObject("MasterText");
        _masterText = textObj.AddComponent<TextMeshProUGUI>();

        typeof(OptionsMenu)
            .GetField("masterVolumeSlider", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(_menu, _masterSlider);

        typeof(OptionsMenu)
            .GetField("masterVolumeValueText", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(_menu, _masterText);
    }

    [TearDown]
    public void TearDown() {
        PlayerPrefs.DeleteAll();

        if (_obj != null) Object.DestroyImmediate(_obj);
        if (_masterSlider != null) Object.DestroyImmediate(_masterSlider.gameObject);
        if (_masterText != null) Object.DestroyImmediate(_masterText.gameObject);
    }

    [Test]
    public void SetMasterVolume_UpdatesPlayerPrefs() {
        _menu.SetMasterVolume(0.5f);

        Assert.AreEqual(0.5f, PlayerPrefs.GetFloat("master_volume"), 0.001f);
    }

    [Test]
    public void SetMasterVolume_UpdatesAudioListener() {
        _menu.SetMasterVolume(0.5f);

        Assert.AreEqual(0.5f, AudioListener.volume, 0.001f);
    }

    [Test]
    public void SetMasterVolume_UpdatesText() {
        _menu.SetMasterVolume(0.75f);

        Assert.AreEqual("75%", _masterText.text);
    }

    [Test]
    public void LoadVolumes_SetsSliderAndTextFromPrefs() {
        PlayerPrefs.SetFloat("master_volume", 0.33f);

        typeof(OptionsMenu)
            .GetMethod("LoadVolumes", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(_menu, null);

        Assert.AreEqual(0.33f, _masterSlider.value, 0.001f);
        Assert.AreEqual("33%", _masterText.text);
    }

    [Test]
    public void SetMasterVolume_DoesNotClampInput() {
        _menu.SetMasterVolume(2f);

        Assert.AreEqual(2f, PlayerPrefs.GetFloat("master_volume"), 0.001f);
    }
}