using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Handles the NPC dialog UI, function menu, tower spawning, and weapon upgrade.
/// </summary>
public class NPCDialog : MonoBehaviour {

    /// <summary>
    /// The main dialog panel shown when the player interacts with the NPC.
    /// </summary>
    public CanvasGroup dialogPanel;

    /// <summary>
    /// The panel that contains additional NPC functions or options.
    /// </summary>
    public CanvasGroup functionsPanel;

    /// <summary>
    /// CanvasGroup for the tower information panel.
    /// This panel shows details about the tower.
    /// </summary>
    public CanvasGroup towerInfoPanel;

    /// <summary>
    /// The tower prefab that will be spawned.
    /// </summary>
    public GameObject towerPrefab;

    /// <summary>
    /// The position and rotation where the tower should be spawned.
    /// </summary>
    public Transform towerSpawnPoint;

    /// <summary>
    /// True while the NPC dialog is open.
    /// Can be read by other scripts, but can only be changed inside NPCDialog.
    /// </summary>
    public bool IsDialogOpen { get; private set; }

    /// <summary>
    /// Reference to the player's inventory.
    /// This is needed to check how much scrap the player currently owns
    /// and to remove scrap when the tower is built.
    /// </summary>
    public Inventory playerInventory;

    /// <summary>
    /// Item definition for scrap.
    /// Important: This must be the same Scrap ItemSO that is also used by the inventory and loot system.
    /// </summary>
    public ItemSO scrapItem;

    /// <summary>
    /// Amount of scrap required to build the tower.
    /// </summary>
    public int scrapCost = 50;

    /// <summary>
    /// Text element that displays the current scrap amount and the required scrap amount.
    /// </summary>
    public TMP_Text scrapAmountText;

    /// <summary>
    /// UI object that contains the warning message.
    /// It is shown when the player does not have enough scrap.
    /// </summary>
    public GameObject warningBox;

    /// <summary>
    /// Button used to build the tower.
    /// It is only interactable when the player has enough scrap.
    /// </summary>
    public Button buildTowerButton;

    public CanvasGroup weaponTypePanel;
    public CanvasGroup weaponPanelRanged;
    public CanvasGroup weaponPanelClose;
    public CanvasGroup weaponInfoPanel;

    public TMP_Text weaponTitleText;
    public TMP_Text weaponDescriptionText;
    public TMP_Text weaponScrapAmountText;
    public GameObject weaponWarningBox;
    public Button buyWeaponButton;

    // Stores the currently selected weapon cost.
    private int selectedWeaponCost;

    // Stores the currently selected weapon item.
    private ItemSO selectedWeaponItem;

    /// <summary>
    /// Prepares the UI panels by briefly activating them and then disabling them again.
    /// This helps avoid visual stuttering when switching between panels.
    /// </summary>
    public void start() {

        HidePanel(dialogPanel);
        HidePanel(functionsPanel);
        HidePanel(towerInfoPanel);

        // Force Unity to update the canvas layout immediately
        Canvas.ForceUpdateCanvases();

        
    }

    /// <summary>
    /// Opens the NPC dialog panel and unlocks the cursor for UI interaction.
    /// </summary>
    public void OpenDialog() {
        IsDialogOpen = true;

        ShowPanel(dialogPanel);
        HidePanel(functionsPanel);
        HidePanel(towerInfoPanel);

        // Unlock and show the cursor so the player can interact with the UI
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Clear the currently selected UI element.
        EventSystem.current.SetSelectedGameObject(null);
    }

    /// <summary>
    /// Closes both NPC UI panels and keeps the cursor visible.
    /// </summary>
    public void CloseDialog() {
        IsDialogOpen = false;

        HidePanel(dialogPanel);
        HidePanel(functionsPanel);
        HidePanel(towerInfoPanel);

        // Unlock and show the cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Clear the currently selected UI element
        EventSystem.current.SetSelectedGameObject(null);
    }

    /// <summary>
    /// Returns from the functions/upgrade panel back to the main dialog panel.
    /// </summary>
    public void Back() {
        // Hide the functions panel and show the dialog panel again
        ShowPanel(dialogPanel);
        HidePanel(functionsPanel);
        HidePanel(towerInfoPanel);

        // Clear the currently selected UI element
        EventSystem.current.SetSelectedGameObject(null);
    }

    /// <summary>
    /// Handles the upgrade option selected by the player.
    /// </summary>
    public void Upgrade() {
        Debug.Log("Upgrade gewählt");
        EventSystem.current.SetSelectedGameObject(null);
    }

    /// <summary>
    /// Handles the functions option selected by the player.
    /// </summary>
    public void Functions() {
        HidePanel(dialogPanel);
        ShowPanel(functionsPanel);
        HidePanel(towerInfoPanel);

        // Clear the currently selected UI element
        EventSystem.current.SetSelectedGameObject(null);
    }

    /// <summary>
    /// Opens the tower information panel.
    /// The functions panel stays visible and the tower info panel is shown next to it.
    /// After opening the panel, the displayed scrap amount and build button state are updated.
    /// </summary>
    public void OpenTowerInfo() {
        ShowPanel(functionsPanel);
        ShowPanel(towerInfoPanel);

        towerInfoPanel.transform.SetAsLastSibling();

        UpdateTowerInfo();
    }

    /// <summary>
    /// Updates the tower information panel.
    /// Shows the current scrap amount, checks if the player has enough scrap,
    /// enables or disables the build button and shows or hides the warning box.
    /// </summary>
    private void UpdateTowerInfo() {
        if (playerInventory == null || scrapItem == null) {
            scrapAmountText.text = "SCRAP: ? / " + scrapCost;
            warningBox.SetActive(true);
            buildTowerButton.interactable = false;
            return;
        }

        int currentScrap = playerInventory.GetItemAmount(scrapItem);
        bool hasEnoughScrap = currentScrap >= scrapCost;

        scrapAmountText.text = "SCRAP: " + currentScrap + " / " + scrapCost;

        warningBox.SetActive(!hasEnoughScrap);
        buildTowerButton.interactable = hasEnoughScrap;
    }

    /// <summary>
    /// Spawns a tower at the assigned spawn point.
    /// </summary>
    public void SpawnTower() {
        if (playerInventory == null || scrapItem == null) {
            Debug.LogError("Inventory oder Scrap Item fehlt!");
            return;
        }

        if (!playerInventory.HasItemAmount(scrapItem, scrapCost)) {
            Debug.Log("Nicht genug Scrap.");
            UpdateTowerInfo();
            return;
        }

        playerInventory.RemoveItem(scrapItem, scrapCost);

        // Create a tower at the spawn point position and rotation
        Instantiate(towerPrefab, towerSpawnPoint.position, towerSpawnPoint.rotation);

        UpdateTowerInfo();

        // Clear the currently selected UI element
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void OpenWeaponTypes() {
        HidePanel(functionsPanel);
        HidePanel(towerInfoPanel);
        HidePanel(weaponPanelRanged);
        HidePanel(weaponPanelClose);
        HidePanel(weaponInfoPanel);

        ShowPanel(weaponTypePanel);
    }

    public void OpenRangedWeapons() {
        HidePanel(weaponTypePanel);
        HidePanel(weaponInfoPanel);

        ShowPanel(weaponPanelRanged);
    }

    public void OpenCloseCombatWeapons() {
        HidePanel(weaponTypePanel);
        HidePanel(weaponInfoPanel);

        ShowPanel(weaponPanelClose);
    }

    public void BackToFunctionsFromWeaponTypes() {
        HidePanel(weaponTypePanel);
        HidePanel(weaponPanelRanged);
        HidePanel(weaponPanelClose);
        HidePanel(weaponInfoPanel);

        ShowPanel(functionsPanel);
    }

    public void BackToWeaponTypes() {
        HidePanel(weaponPanelRanged);
        HidePanel(weaponPanelClose);
        HidePanel(weaponInfoPanel);

        ShowPanel(weaponTypePanel);
    }

    // WEG
    public void OpenWeapons() {
        HidePanel(functionsPanel);
        HidePanel(towerInfoPanel);

        ShowPanel(weaponPanelRanged);
        HidePanel(weaponInfoPanel);
    }

    // WEG
    public void BackToFunctionsFromWeapons() {
        HidePanel(weaponPanelRanged);
        HidePanel(weaponInfoPanel);

        ShowPanel(functionsPanel);
    }

    /// <summary>
    /// Updates the weapon information panel.
    /// Shows the current scrap amount, checks if the player has enough scrap,
    /// enables or disables the buy button and shows or hides the warning box.
    /// </summary>
    private void UpdateWeaponInfo() {
        // If the inventory or the scrap item was not assigned,
        // show an unknown scrap amount and disable buying.
        if (playerInventory == null || scrapItem == null) {
            weaponScrapAmountText.text = "SCRAP: ? / " + selectedWeaponCost;
            weaponWarningBox.SetActive(true);
            buyWeaponButton.interactable = false;
            return;
        }

        // Get the current amount of scrap from the player's inventory.
        int currentScrap = playerInventory.GetItemAmount(scrapItem);

        // Check if the player has enough scrap to buy the selected weapon.
        bool hasEnoughScrap = currentScrap >= selectedWeaponCost;

        // Update the scrap amount text in the weapon information panel.
        weaponScrapAmountText.text = "SCRAP: " + currentScrap + " / " + selectedWeaponCost;

        // Show the warning box only if the player does not have enough scrap.
        weaponWarningBox.SetActive(!hasEnoughScrap);

        // The buy button can only be clicked if enough scrap is available.
        buyWeaponButton.interactable = hasEnoughScrap;
    }

    /// <summary>
    /// Selects the assault rifle and displays its information in the weapon info panel.
    /// </summary>
    public void SelectAssaultRifle() {
        ShowPanel(weaponInfoPanel);

        weaponTitleText.text = "ASSAULT RIFLE";
        weaponDescriptionText.text = "A fast automatic weapon with good damage and high fire rate.";

        selectedWeaponCost = 300;

        UpdateWeaponInfo();
    }

    /// <summary>
    /// Selects the shotgun and displays its information in the weapon info panel.
    /// </summary>
    public void SelectShotgun() {
        ShowPanel(weaponInfoPanel);

        weaponTitleText.text = "SHOTGUN";
        weaponDescriptionText.text = "Reliable close-range weapon with high stopping power.";

        selectedWeaponCost = 200;

        UpdateWeaponInfo();
    }

    /// <summary>
    /// Selects the sniper and displays its information in the weapon info panel.
    /// </summary>
    public void SelectSniper() {
        ShowPanel(weaponInfoPanel);

        weaponTitleText.text = "SNIPER";
        weaponDescriptionText.text = "A powerful long-range weapon for precise shots against zombies.";

        selectedWeaponCost = 250;

        UpdateWeaponInfo();
    }

    /// <summary>
    /// Selects the baseball bat and displays its information in the weapon info panel.
    /// </summary>
    public void SelectBaseballBat() {
        ShowPanel(weaponInfoPanel);

        weaponInfoPanel.transform.SetAsLastSibling();

        weaponTitleText.text = "BASEBALL BAT";
        weaponDescriptionText.text = "A simple close combat weapon. Useful for basic defense against nearby zombies.";

        selectedWeaponCost = 75;

        UpdateWeaponInfo();
    }

    /// <summary>
    /// Selects the crowbar and displays its information in the weapon info panel.
    /// </summary>
    public void SelectCrowbar() {
        ShowPanel(weaponInfoPanel);

        weaponInfoPanel.transform.SetAsLastSibling();

        weaponTitleText.text = "CROWBAR";
        weaponDescriptionText.text = "A solid close combat weapon with good impact damage and practical survival use.";

        selectedWeaponCost = 100;

        UpdateWeaponInfo();
    }

    /// <summary>
    /// Selects the sword and displays its information in the weapon info panel.
    /// </summary>
    public void SelectSword() {
        ShowPanel(weaponInfoPanel);

        weaponInfoPanel.transform.SetAsLastSibling();

        weaponTitleText.text = "SWORD";
        weaponDescriptionText.text = "A sharp close combat weapon with high damage and good reach.";

        selectedWeaponCost = 180;

        UpdateWeaponInfo();
    }

    /// <summary>
    /// Selects the knife and displays its information in the weapon info panel.
    /// </summary>
    public void SelectKnife() {
        ShowPanel(weaponInfoPanel);

        weaponInfoPanel.transform.SetAsLastSibling();

        weaponTitleText.text = "KNIFE";
        weaponDescriptionText.text = "A light close combat weapon. Fast, cheap and useful in emergency situations.";

        selectedWeaponCost = 50;

        UpdateWeaponInfo();
    }

    /// <summary>
    /// Selects the axe and displays its information in the weapon info panel.
    /// </summary>
    public void SelectAxe() {
        ShowPanel(weaponInfoPanel);

        weaponInfoPanel.transform.SetAsLastSibling();

        weaponTitleText.text = "AXE";
        weaponDescriptionText.text = "A heavy close combat weapon with strong damage against zombies.";

        selectedWeaponCost = 150;

        UpdateWeaponInfo();
    }

    /// <summary>
    /// Selects the iron axe and displays its information in the weapon info panel.
    /// </summary>
    public void SelectIronAxe() {
        ShowPanel(weaponInfoPanel);

        weaponInfoPanel.transform.SetAsLastSibling();

        weaponTitleText.text = "IRON AXE";
        weaponDescriptionText.text = "A reinforced axe with very high close combat damage and durability.";

        selectedWeaponCost = 220;

        UpdateWeaponInfo();
    }

    /// <summary>
    /// Makes a panel visible and allows the player to interact with it.
    /// This is used instead of SetActive(true), so the UI does not need to be rebuilt every time.
    /// </summary>
    private void ShowPanel(CanvasGroup panel) {
        // If no panel was assigned in the Inspector, stop the function.
        if (panel == null)
            return;

        // Makes the panel visible.
        panel.alpha = 1f;

        // Allows buttons and other UI elements inside the panel to be clicked.
        panel.interactable = true;

        // Allows the panel to receive mouse / UI raycasts.
        panel.blocksRaycasts = true;
    }

    /// <summary>
    /// Makes a panel invisible and disables interaction with it.
    /// The GameObject stays active, but the player cannot see or click it.
    /// </summary>
    private void HidePanel(CanvasGroup panel) {
        // If no panel was assigned in the Inspector, stop the function.
        if (panel == null)
            return;

        // Makes the panel invisible.
        panel.alpha = 0f;

        // Prevents buttons and other UI elements inside the panel from being clicked.
        panel.interactable = false;

        // Prevents the invisible panel from blocking clicks on other UI elements.
        panel.blocksRaycasts = false;
    }
}


