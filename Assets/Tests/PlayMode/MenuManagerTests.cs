using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

/// <summary>
/// PlayMode tests for MenuManager.
/// Verifies menu UI behavior, scene transitions, and application controls.
/// </summary>
public class MenuManagerTests {
    private GameObject _menuObj;
    private MenuManager _menu;

    private GameObject _settingsPanel;
    private CanvasGroup _fadeGroup;

    private GameObject es;

    /// <summary>
    /// Creates a fully valid MenuManager instance with required dependencies.
    /// Prevents null reference issues during tests.
    /// </summary>
    [SetUp]
    public void Setup() {
        _menuObj = new GameObject("MenuManager");
        _menu = _menuObj.AddComponent<MenuManager>();

        // Settings panel mock
        _settingsPanel = new GameObject("SettingsPanel");
        _settingsPanel.SetActive(false);

        // Fade group mock
        GameObject fadeObj = new GameObject("Fade");
        _fadeGroup = fadeObj.AddComponent<CanvasGroup>();

        // Inject private fields via reflection
        var type = typeof(MenuManager);

        type.GetField("settingsPanel",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(_menu, _settingsPanel);

        type.GetField("fadeGroup",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(_menu, _fadeGroup);

        type.GetField("fadeDuration",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(_menu, 0.01f); // speed up tests

        // Ensure EventSystem exists (UI safety)
        if (Object.FindFirstObjectByType<EventSystem>() == null) {
            es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<InputSystemUIInputModule>();
        }
    }

    [TearDown]
    public void Teardown() {
        if (es != null) Object.DestroyImmediate(es.gameObject);
    }

    /// <summary>
    /// Ensures that the settings panel opens when settings are pressed.
    /// </summary>
    [Test]
    public void SettingsPanel_Opens() {
        _menu.OnSettingsPressed();

        Assert.IsTrue(_settingsPanel.activeSelf);
    }

    /// <summary>
    /// Ensures that the settings panel closes when requested.
    /// </summary>
    [Test]
    public void SettingsPanel_Closes() {
        _settingsPanel.SetActive(true);

        _menu.OnCloseSettingsPressed();

        Assert.IsFalse(_settingsPanel.activeSelf);
    }

    /// <summary>
    /// Ensures that NewGame starts transition state correctly.
    /// (Cannot test actual scene load in EditMode, but state change is verified.)
    /// </summary>
    /* [Test]
    public void NewGame_SetsTransitionState() {
        _menu.NewGame();

        var field = typeof(MenuManager)
            .GetField("isTransitioning",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        bool value = (bool)field.GetValue(_menu);

        Assert.IsTrue(value);
    } */

    /// <summary>
    /// Ensures ExitGame does not crash and executes without exceptions.
    /// (Editor/application quit cannot be validated in PlayMode tests.)
    /// </summary>
    // Deaktiviert in der CI: ExitGame() ruft im Editor
    // UnityEditor.EditorApplication.isPlaying = false auf und stoppt damit den
    // PlayMode -> der gesamte Testlauf bricht ab ("player was stopped", Exit 3).
    // Dieser Quit-Pfad laesst sich in einem PlayMode-Test prinzipiell nicht
    // validieren. Siehe TODO: ggf. ExitGame() testbar umbauen (Quit-Logik kapseln).
    [Ignore("ExitGame() setzt EditorApplication.isPlaying = false und bricht den PlayMode-Testlauf in der CI ab.")]
    [Test]
    public void ExitGame_DoesNotCrash() {
        Assert.DoesNotThrow(() => {
            _menu.ExitGame();
        });
    }
}