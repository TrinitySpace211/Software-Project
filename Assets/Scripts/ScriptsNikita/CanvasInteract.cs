using UnityEngine;

/// <summary>
/// Makes the canvas object gently float up and down.
/// </summary>
public class CanvasInteract : MonoBehaviour
{
    /// <summary>
    /// Controls how fast the object floats.
    /// </summary>
    public float floatSpeed = 2f;

    /// <summary>
    /// Controls how high the object floats.
    /// </summary>
    public float floatHeight = 0.08f;
  

    private Vector3 startPos;


    /// <summary>
    /// Saves the starting local position of the object.
    /// </summary>
    void Start() {
        startPos = transform.localPosition;
    }

    /// <summary>
    /// Updates the object's position to create a floating animation.
    /// </summary>
    void Update() {
        // Floating
        float y = Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.localPosition = startPos + new Vector3(0, y, 0);
    }
}
