using UnityEngine;
using UnityEngine.EventSystems;

public class NPCDialog : MonoBehaviour {
    public GameObject dialogPanel;
    public GameObject functionsPanel;

    public GameObject towerPrefab;
    public Transform towerSpawnPoint;

    public  void start() {

        // Panels kurz aktivieren und dann wieder deaktivieren, um "Ruckler" beim Wechseln der Panels zu vermeiden.
        dialogPanel.SetActive(true);
        functionsPanel.SetActive(true);

        Canvas.ForceUpdateCanvases();

        dialogPanel.SetActive(false);
        functionsPanel.SetActive(false);
    }

    public void OpenDialog() {
        dialogPanel.SetActive(true);
        functionsPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void CloseDialog() {
        dialogPanel.SetActive(false);
        functionsPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void Back() {
        functionsPanel.SetActive(false);
        dialogPanel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void Upgrade() {
        Debug.Log("Upgrade gewählt");
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void Functions() {
        dialogPanel.SetActive(false);
        functionsPanel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void SpawnTower() {
        Debug.Log("Tower ausgewählt");

        Instantiate(towerPrefab, towerSpawnPoint.position, towerSpawnPoint.rotation);

        EventSystem.current.SetSelectedGameObject(null);
    }
}
