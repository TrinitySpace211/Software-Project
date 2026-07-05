using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Static class which Load the target scene after the Loading Screen
/// </summary>
public static class Loader {

    /// <summary>
    /// All the Scenes as enums
    /// </summary>
    public enum Scene {
        MainMenu,
        MainScene,
        TutorialScene,
        ExtractionScene,
        LoadingScene
    }

    private static Scene targetScene;

    /// <summary>
    /// static Load function to be used everywhere
    /// </summary>
    /// <param name="targetScene">loads the targetScene after the Loading Screen called the LoaderCallback</param>
    public static void Load(Scene targetScene) {
        Loader.targetScene = targetScene;

        SceneManager.LoadScene(Scene.LoadingScene.ToString());
    }

    /// <summary>
    /// Loads the target Scene
    /// </summary>
    public static void LoaderCallback() {
        SceneManager.LoadScene(targetScene.ToString());
    }
}
