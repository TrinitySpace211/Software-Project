using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;
using Image = UnityEngine.UI.Image;

public class Inventory : MonoBehaviour
{
    //Items hinzufügen
    public ItemSO woodItem;
    public ItemSO axeItem;

    public GameObject hotbarObj;
    public GameObject inventorySlotParent;
    public GameObject container;

    public Image dragIcon;

    private List<Slot> inventorySlots = new List<Slot>();
    private List<Slot> hotbarSlots = new List<Slot>();
    private List<Slot> allSlots = new List<Slot>();

    private Slot draggedSlot = null;
    private bool isDragging = false;

    private bool isPaused = false;

    private void Awake() {
        inventorySlots.AddRange(inventorySlotParent.GetComponentsInChildren<Slot>());
        hotbarSlots.AddRange(hotbarObj.GetComponentsInChildren<Slot>());

        allSlots.AddRange(inventorySlots);
        allSlots.AddRange(hotbarSlots);
    }

    private void Start() {
        container.SetActive(false);
    }

    //wichtig für die Items
    void Update()
    {
        if(Keyboard.current.bKey.wasPressedThisFrame) {
            AddItem(woodItem, 3);
        } else if(Keyboard.current.nKey.wasPressedThisFrame) {
            AddItem(axeItem, 1);
        }

        if (Keyboard.current.iKey.wasPressedThisFrame) {
            container.SetActive(!container.activeInHierarchy);
            //Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = !Cursor.visible;
            TogglePause();
        }

        StartDrag();
        UpdateDragItemPosition();
        EndDrag();
    }

    public void AddItem(ItemSO itemToAdd, int amount) {
        int remaining = amount;

        foreach(Slot slot in allSlots) {
            if(slot.HasItem() && slot.GetItem() == itemToAdd) {
                int currentAmount = slot.GetAmount();
                int maxStack = itemToAdd.maxStackSize;

                if (currentAmount < maxStack) {
                    int spaceLeft = maxStack - currentAmount;
                    int amountToAdd = Mathf.Min(spaceLeft, remaining);

                    slot.SetItem(itemToAdd, currentAmount + amountToAdd);
                    remaining -= amountToAdd;

                    if (remaining <= 0)
                        return;
                }
            }
        }

        foreach (Slot slot in allSlots) {
            if(!slot.HasItem()) {
                int amountToPlace = Mathf.Min(itemToAdd.maxStackSize, remaining);
                slot.SetItem(itemToAdd, amountToPlace);
                remaining -= amountToPlace;

                if (remaining <= 0)
                    return;
            }
        }

        if(remaining > 0) {
            Debug.Log("Inventar ist voll, konnte nicht " + remaining + " von " + itemToAdd.itemName + " hinzufügen.");
        }
    }

    private void StartDrag() {
        if (Input.GetMouseButtonDown(0)) {
            Slot hovered = GetHoveredSlot();

            if (hovered != null && hovered.HasItem()) {
                draggedSlot = hovered;
                isDragging = true;

                //zeigt das gezogene Item
                dragIcon.sprite = hovered.GetItem().icon;
                dragIcon.color = new Color(1, 1, 1, 0.5f);
                dragIcon.enabled = true;
            }
        }
    }

    private void EndDrag() {
        if (Input.GetMouseButtonUp(0) && isDragging) {
            Slot hovered = GetHoveredSlot();

            if(hovered != null) {
                HandleDrop(draggedSlot, hovered);

                dragIcon.enabled = false;
                draggedSlot = null;
                isDragging = false;
            }
        }
    }

    private Slot GetHoveredSlot() {
        foreach(Slot s in allSlots) {
            if(s.hovering)
                return s;
        }

        return null;
    }

    private void HandleDrop(Slot from, Slot to) {
        if (from == to) return;

        //Stacking der Items
        if(to.HasItem() && to.GetItem() == from.GetItem()) {
            int max = to.GetItem().maxStackSize;
            int space = max -to.GetAmount();

            if(space > 0) {
                int move = Mathf.Min(space, from.GetAmount());
                to.SetItem(to.GetItem(), to.GetAmount() + move);
                from.SetItem(from.GetItem(), from.GetAmount() - move);

                if (from.GetAmount() <= 0)
                    from.ClearSlot();

                return;
            }
        }

        //Wenn es sich um ein anderes Item handelt
        if (to.HasItem()) {
            ItemSO tempItem = to.GetItem();
            int tempAmount = to.GetAmount();

            to.SetItem(from.GetItem(), from.GetAmount());
            from.SetItem(tempItem, tempAmount);
            return;
        }

        //Wenn es ein leerer Slot ist
        to.SetItem(from.GetItem(), from.GetAmount());
        from.ClearSlot();
    }

    private void UpdateDragItemPosition() {
        if(isDragging) {
            dragIcon.transform.position = Input.mousePosition;
        }
    }
    private void TogglePause() {
        isPaused = !isPaused;

        container.SetActive(isPaused);

        Cursor.visible = isPaused;

        Time.timeScale = isPaused ? 0f : 1f;
    }
}
