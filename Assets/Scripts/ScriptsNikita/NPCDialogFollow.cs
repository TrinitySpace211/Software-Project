using UnityEngine;

public class NPCDialogFollow : MonoBehaviour
{
    public Transform npc;
    public Vector3 offset = new Vector3(120f, 80f, 0f);

    private RectTransform rectTransform;

    void Awake() {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update() {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(npc.position);
        rectTransform.position = screenPos + offset;
    }
}
