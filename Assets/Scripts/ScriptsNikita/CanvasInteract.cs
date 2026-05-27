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






    /*public float speed = 3f;
    public float amount = 0.15f;

    private Vector3 startScale;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        float scale = 1f + Mathf.Sin(Time.time * speed) * amount;
        transform.localScale = startScale * scale;
    }
    */
}
