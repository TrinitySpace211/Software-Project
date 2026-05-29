using UnityEngine;

public class CanvasInteract : MonoBehaviour
{

    public float floatSpeed = 2f;
    public float floatHeight = 0.08f;

  

    private Vector3 startPos;

    void Start() {
        startPos = transform.localPosition;
    }

    void Update() {
        // Floating
        float y = Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.localPosition = startPos + new Vector3(0, y, 0);
    }
}
