using UnityEngine;

/// <summary>
/// Triggers the LoaderCallback Function when the Loading Screen starts
/// </summary>
public class LoaderCallback : MonoBehaviour {

    private bool isFirstUpdate = true;

    private void Update() {
        if (isFirstUpdate) {
            isFirstUpdate = false;

            Loader.LoaderCallback();
        }
    }
}
