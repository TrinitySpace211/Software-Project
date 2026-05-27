using UnityEngine;

public class NPCInteractable : MonoBehaviour
{
    public NPCDialog npcDialog;
    public void Interact() {
        Debug.Log("Interact");
        npcDialog.OpenDialog();
    }
}
