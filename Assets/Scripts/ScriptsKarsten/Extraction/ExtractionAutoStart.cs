using UnityEngine;
using System.Collections;

public class ExtractionAutoStart : MonoBehaviour {
    [SerializeField] private ExtractionController extractionController;

    private void Start() {
        if (extractionController != null) {
            extractionController.StartExtraction();
        }
    }
}