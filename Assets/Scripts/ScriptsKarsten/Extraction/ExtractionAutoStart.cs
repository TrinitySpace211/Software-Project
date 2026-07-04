using UnityEngine;
using System.Collections;

/// <summary>
/// Automatically triggers the extraction sequence when the scene starts.
/// </summary>
public class ExtractionAutoStart : MonoBehaviour {
    [SerializeField] private ExtractionController extractionController;

    /// <summary>
    /// Starts the extraction process on scene start if a controller is assigned.
    /// </summary>
    private void Start() {
        if (extractionController != null) {
            extractionController.StartExtraction();
        }
    }
}