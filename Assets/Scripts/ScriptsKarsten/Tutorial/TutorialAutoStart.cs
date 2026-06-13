using UnityEngine;
using System.Collections;

/// <summary>
/// Automatically triggers the tutorial sequence when the scene starts.
/// </summary>
public class TutorialAutoStart : MonoBehaviour {
    [SerializeField] private TutorialManager tutorialManager;

    /// <summary>
    /// Starts the tutorial process on scene start if a controller is assigned.
    /// </summary>
    private void Start() {
        if (tutorialManager != null) {
            tutorialManager.StartTutorial();
        }
    }
}