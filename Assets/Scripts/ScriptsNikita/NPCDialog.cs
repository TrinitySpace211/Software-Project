using UnityEngine;

public class NPCDialog : MonoBehaviour {
    public GameObject dialogPanel;

    public void OpenDialog() {
        dialogPanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseDialog() {
        dialogPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Upgrade() {
        Debug.Log("Upgrade gewählt");
    }

    public void Functions() {
        Debug.Log("Funktionen gewählt");
    }
}
