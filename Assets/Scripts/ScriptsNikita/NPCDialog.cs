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
    /// Spawn points where towers can be built.
    /// Each tower is placed at the next free spawn point.
    /// </summary>
    public Transform[] towerSpawnPoints;

    /// <summary>
    /// Text element that displays how many towers have already been built.
    /// </summary>
    public TMP_Text builtTowerText;

    /// <summary>
    /// Text element inside the tower warning box.
    /// It displays why the tower cannot be built, for example not enough scrap or maximum towers reached.
    /// </summary>
    public TMP_Text towerWarningText;

    /// <summary>
    /// Number of towers that have already been built.
    /// This is used to select the next spawn point.
    /// </summary>
    private int builtTowerCount = 0;

    /// <summary>
    /// Maximum number of towers that can be built.
    /// </summary>
    private const int maxTowerCount = 8;

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

    /// <summary>
    /// Bullet script used by the tower projectile.
    /// The damage value is read from this bullet.
    /// </summary>
    public Bullet towerBullet;

    /// <summary>
    /// Text element that displays the damage value of the tower projectile.
    /// </summary>
    public TMP_Text towerDamageText;

    /// <summary>
    /// Panel where the player can choose between ranged combat and close combat weapons.
    /// </summary>
    public CanvasGroup weaponTypePanel;

    /// <summary>
    /// Panel that contains all ranged weapon buttons.
    /// </summary>
    public CanvasGroup weaponPanelRanged;

    /// <summary>
    /// Panel that contains all close combat weapon buttons.
    /// </summary>
    public CanvasGroup weaponPanelClose;

    /// <summary>
    /// Panel that displays detailed information about the currently selected weapon.
    /// </summary>
    public CanvasGroup weaponInfoPanel;

    /// <summary>
    /// Text element that displays the name of the selected weapon.
    /// </summary>
    public TMP_Text weaponTitleText;

    /// <summary>
    /// Text element that displays the description of the selected weapon.
    /// </summary>
    public TMP_Text weaponDescriptionText;

    /// <summary>
    /// Text element that displays the current scrap amount and the required scrap cost.
    /// </summary>
    public TMP_Text weaponScrapAmountText;

    /// <summary>
    /// Text element that displays the damage value of the selected weapon.
    /// </summary>
    public TMP_Text weaponDamageText;

    /// <summary>
    /// UI object that contains the warning message.
    /// It is shown when the player does not have enough scrap to buy the selected weapon.
    /// </summary>
    public GameObject weaponWarningBox;

    /// <summary>
    /// Button used to buy the currently selected weapon.
    /// It is only interactable when the player has enough scrap.
    /// </summary>
    public Button buyWeaponButton;

    /// <summary>
    /// Item definition for the shotgun.
    /// This ItemSO is added to the inventory when the player buys the shotgun.
    /// </summary>
    public ItemSO shotgunItem;

    /// <summary>
    /// Item definition for the assault rifle.
    /// This ItemSO is added to the inventory when the player buys the assault rifle.
    /// </summary>
    public ItemSO assaultRifleItem;

    /// <summary>
    /// Item definition for the sniper rifle.
    /// This ItemSO is added to the inventory when the player buys the sniper rifle.
    /// </summary>
    public ItemSO sniperItem;

    /// <summary>
    /// Item definition for the baseball bat.
    /// This ItemSO is added to the inventory when the player buys the baseball bat.
    /// </summary>
    public ItemSO baseballBatItem;

    /// <summary>
    /// Item definition for the crowbar.
    /// This ItemSO is added to the inventory when the player buys the crowbar.
    /// </summary>
    public ItemSO crowbarItem;

    /// <summary>
    /// Item definition for the sword.
    /// This ItemSO is added to the inventory when the player buys the sword.
    /// </summary>
    public ItemSO swordItem;

    /// <summary>
    /// Item definition for the knife.
    /// This ItemSO is added to the inventory when the player buys the knife.
    /// </summary>
    public ItemSO knifeItem;

    /// <summary>
    /// Item definition for the axe.
    /// This ItemSO is added to the inventory when the player buys the axe.
    /// </summary>
    public ItemSO axeItem;

    /// <summary>
    /// Item definition for the tomahawk.
    /// This ItemSO is added to the inventory when the player buys the tomahawk.
    /// </summary>
    public ItemSO tomahawkItem;

    /// <summary>
    /// Stores the scrap cost of the currently selected weapon.
    /// This amount of scrap will be removed from the inventory when the player buys the weapon.
    /// </summary>
    private int selectedWeaponCost;

    /// <summary>
    /// Stores the currently selected weapon item.
    /// This item will be added to the inventory when the player clicks the buy button.
    /// </summary>
    private ItemSO selectedWeaponItem;

    /// <summary>
    /// Panel that contains the available upgrade options.
    /// </summary>
    public CanvasGroup upgradePanel;

    /// <summary>
    /// Panel that displays detailed information about the selected upgrade.
    /// </summary>
    public CanvasGroup upgradeInfoPanel;

    /// <summary>
    /// Amount of scrap required to heal the gas tank.
    /// </summary>
    public int gasTankHealScrapCost = 40;

    /// <summary>
    /// Amount of health restored to the gas tank.
    /// </summary>
    public int gasTankHealAmount = 10;

    /// <summary>
    /// Text element that displays the current scrap amount and required scrap cost.
    /// </summary>
    public TMP_Text gasTankHealScrapText;

    /// <summary>
    /// Warning box that is shown if the player cannot buy the gas tank healing upgrade.
    /// </summary>
    public GameObject gasTankHealWarningBox;

    /// <summary>
    /// Button used to buy the gas tank healing upgrade.
    /// </summary>
    public Button gasTankHealButton;

    /// <summary>
    /// Reference to the gas tank health script.
    /// This is used to restore health when the player buys the gas tank healing upgrade.
    /// </summary>
    public GasTankHealth gasTankHealth;

    /// <summary>
    /// Text element inside the gas tank healing warning box.
    /// It displays why the gas tank healing upgrade cannot be bought.
    /// </summary>
    public TMP_Text gasTankHealWarningText;

    /// <summary>
    /// Prepares the UI panels by briefly activating them and then disabling them again.
    /// This helps avoid visual stuttering when switching between panels.
    /// </summary>
    public void Start() {
        HidePanel(dialogPanel);
        HidePanel(functionsPanel);
        HidePanel(towerInfoPanel);
        HidePanel(upgradePanel);
        HidePanel(upgradeInfoPanel);

        // Force Unity to update the canvas layout immediately
        Canvas.ForceUpdateCanvases();
    }

    /// <summary>
    /// Opens the NPC dialog panel and unlocks the cursor for UI interaction.
    /// </summary>
    public void OpenDialog() {
        // Stores that the NPC dialog is currently open.
        // Other scripts can use this value to check if the player is already talking to the NPC
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
        // Stores that the NPC dialog is currently open.
        // Other scripts can use this value to check if the player is already talking to the NPC
        IsDialogOpen = false;

        HidePanel(dialogPanel);
        HidePanel(functionsPanel);
        HidePanel(towerInfoPanel);
        HidePanel(upgradePanel);
        HidePanel(upgradeInfoPanel);

        // Unlock and show the cursor
        Cursor.lockState = CursorLockMode.None;

        if (playerInventory.container.gameObject.activeInHierarchy) {
            Cursor.visible = true;
        } else if (!playerInventory.container.gameObject.activeInHierarchy) {
            Cursor.visible = false;
        }
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

        CloseDialog();

        // Clear the currently selected UI element
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
            // If the inventory or the scrap item is missing,
            // the current scrap amount cannot be checked.
            scrapAmountText.text = "SCRAP: ? / " + scrapCost;

            if (towerDamageText != null && towerBullet != null) {
                // Show the damage value from the assigned tower bullet.
                towerDamageText.text = "" + towerBullet.damage;
            } else if (towerDamageText != null) {
                // Show unknown damage if no tower bullet was assigned.
                towerDamageText.text = "?";
            }

            if (builtTowerText != null) {
                // Show how many towers have already been built.
                builtTowerText.text = "" + builtTowerCount + " / " + maxTowerCount;
            }

            // Show the warning box because building is not possible.
            warningBox.SetActive(true);

            // Disable the build button to prevent errors.
            buildTowerButton.interactable = false;

            return;
        }

        // Get the current amount of scrap from the player's inventory.
        int currentScrap = playerInventory.GetItemAmount(scrapItem);

        // Check if another tower can still be built.
        // This is only true while the number of built towers is below the maximum tower count.
        bool canBuildMoreTowers = builtTowerCount < maxTowerCount;

        // Check if the player has enough scrap to pay the tower cost.
        bool hasEnoughScrap = currentScrap >= scrapCost;

        // The tower can only be built if the player has enough scrap
        // and the maximum tower amount has not been reached yet.
        bool canBuildTower = hasEnoughScrap && canBuildMoreTowers;

        // Show current scrap amount and required scrap cost.
        scrapAmountText.text = "SCRAP: " + currentScrap + " / " + scrapCost;

        if (towerDamageText != null && towerBullet != null) {
            // Show the damage value from the assigned tower bullet.
            towerDamageText.text = "" + towerBullet.damage;
        } else if (towerDamageText != null) {
            // Show unknown damage if no tower bullet was assigned.
            towerDamageText.text = "?";
        }

        if (builtTowerText != null) {
            // Show how many towers have already been built.
            builtTowerText.text = "" + builtTowerCount + " / " + maxTowerCount;
        }

        // Clear the warning text before creating a new warning message.
        towerWarningText.text = "";

        // Add a warning message if the player does not have enough scrap.
        if (!hasEnoughScrap) {
            towerWarningText.text += "NOT ENOUGH RESOURCES";
        }

        // Add a warning message if the maximum tower amount has been reached.
        if (!canBuildMoreTowers) {
            // If there is already another warning message,
            // add a line break before adding the next message.
            if (towerWarningText.text != "") {
                towerWarningText.text += "\n";
            }

            // Add the tower limit warning message.
            towerWarningText.text += "MAX TOWERS REACHED";
        }

        // Show the warning box only if the player does not have enough scrap.
        warningBox.SetActive(!canBuildTower);

        // Enable the build button only if the player has enough scrap
        // and the maximum tower count has not been reached.
        buildTowerButton.interactable = canBuildTower;
    }

    /// <summary>
    /// Spawns a tower at the assigned spawn point.
    /// </summary>
    public void SpawnTower() {
        if (playerInventory == null || scrapItem == null) {
            // If the inventory or the scrap item is missing,
            // the tower cannot be built correctly.
            Debug.LogError("Inventory oder Scrap Item fehlt!");
            return;
        }

        if (builtTowerCount >= maxTowerCount || builtTowerCount >= towerSpawnPoints.Length) {
            // Stop if all possible tower positions are already used.
            Debug.Log("Maximale Anzahl an Towern erreicht.");
            UpdateTowerInfo();
            return;
        }

        // Get the next free spawn point.
        Transform selectedSpawnPoint = towerSpawnPoints[builtTowerCount];

        // Check if the player has enough scrap to build the tower.
        if (!playerInventory.HasItemAmount(scrapItem, scrapCost)) {
            // Stop building if the player does not have enough scrap.
            Debug.Log("Nicht genug Scrap.");

            // Update the tower info panel so the warning and button state are correct.
            UpdateTowerInfo();

            return;
        }

        // Remove the required scrap amount from the player's inventory.
        playerInventory.RemoveItem(scrapItem, scrapCost);

        // Create a tower at the selected spawn point position and rotation.
        Instantiate(towerPrefab, selectedSpawnPoint.position, selectedSpawnPoint.rotation);

        // Increase the built tower count so the next tower uses the next spawn point.
        builtTowerCount++;

        // Update the tower info panel after the scrap amount has changed.
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

            // Show the damage value of the selected weapon
            if (selectedWeaponItem != null) {
                weaponDamageText.text = "" + selectedWeaponItem.baseDamage;
            } else {
                weaponDamageText.text = "?";
            }

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

        // Show the damage value of the selected weapon
        if (selectedWeaponItem != null) {
            weaponDamageText.text = "" + selectedWeaponItem.baseDamage;
        } else {
            weaponDamageText.text = "?";
        }

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

        selectedWeaponItem = assaultRifleItem;
        selectedWeaponCost = 3;

        UpdateWeaponInfo();
    }

    /// <summary>
    /// Selects the shotgun and displays its information in the weapon info panel.
    /// </summary>
    public void SelectShotgun() {
        ShowPanel(weaponInfoPanel);

        weaponTitleText.text = "SHOTGUN";
        weaponDescriptionText.text = "Reliable close-range weapon with high power.";

        selectedWeaponItem = shotgunItem;
        selectedWeaponCost = 1;

        UpdateWeaponInfo();
    }

    /// <summary>
    /// Selects the sniper and displays its information in the weapon info panel.
    /// </summary>
    public void SelectSniper() {
        ShowPanel(weaponInfoPanel);

        weaponTitleText.text = "SNIPER";
        weaponDescriptionText.text = "A powerful long-range weapon for precise shots against zombies.";

        selectedWeaponItem = sniperItem;
        selectedWeaponCost = 1;

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

        selectedWeaponItem = baseballBatItem;
        selectedWeaponCost = 2;

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

        selectedWeaponItem = crowbarItem;
        selectedWeaponCost = 1;

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

        selectedWeaponItem = swordItem;
        selectedWeaponCost = 1;

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

        selectedWeaponItem = knifeItem;
        selectedWeaponCost = 2;

        UpdateWeaponInfo();
    }

    /// <summary>
    /// Selects the axe and displays its information in the weapon info panel.
    /// </summary>
    public void SelectAxe() {
        ShowPanel(weaponInfoPanel);

        weaponInfoPanel.transform.SetAsLastSibling();

        weaponTitleText.text = "AXE";
        weaponDescriptionText.text = "A close combat weapon with strong damage against zombies.";

        selectedWeaponItem = axeItem;
        selectedWeaponCost = 1;

        UpdateWeaponInfo();
    }

    /// <summary>
    /// Selects the tomahawk and displays its information in the weapon info panel.
    /// </summary>
    public void SelectTomahawk() {
        ShowPanel(weaponInfoPanel);

        weaponInfoPanel.transform.SetAsLastSibling();

        weaponTitleText.text = "TOMAHAWK";
        weaponDescriptionText.text = "A compact close combat weapon with high damage. Useful for attacks against nearby zombies.";

        selectedWeaponItem = tomahawkItem;
        selectedWeaponCost = 1;

        UpdateWeaponInfo();
    }

    /// <summary>
    /// Buys the currently selected weapon if the player has enough scrap.
    /// The required scrap amount is removed from the inventory and the weapon item is added.
    /// </summary>
    public void BuySelectedWeapon() {
        if (playerInventory == null) {
            // If the player inventory is missing,
            // the weapon cannot be added and scrap cannot be removed.
            Debug.LogError("Player Inventory is missing!");
            return;
        }

        if (scrapItem == null) {
            // If the scrap item is missing,
            // the script cannot check or remove the required scrap amount.
            Debug.LogError("Scrap Item is missing!");
            return;
        }

        if (selectedWeaponItem == null) {
            // If no weapon was selected before clicking the buy button,
            // there is no item that can be added to the inventory.
            Debug.LogError("No weapon item selected!");
            return;
        }

        if (selectedWeaponCost <= 0) {
            // If the selected weapon cost is zero or negative,
            // the weapon setup is invalid.
            Debug.LogError("Selected weapon cost is invalid!");
            return;
        }

        // Check if the player has enough scrap to buy the selected weapon.
        if (!playerInventory.HasItemAmount(scrapItem, selectedWeaponCost)) {
            // Stop buying if the player does not have enough scrap.
            Debug.Log("Not enough scrap to buy weapon.");

            // Update the weapon info panel so the warning and button state are correct.
            UpdateWeaponInfo();

            return;
        }

        // Remove the required scrap amount from the player's inventory.
        bool removedScrap = playerInventory.RemoveItem(scrapItem, selectedWeaponCost);

        if (!removedScrap) {
            // If scrap could not be removed, stop the buying process.
            // This prevents the weapon from being added for free.
            Debug.LogError("Could not remove scrap from inventory.");

            // Update the weapon info panel after the failed remove attempt.
            UpdateWeaponInfo();

            return;
        }

        // Add the selected weapon item to the player's inventory.
        playerInventory.AddItem(selectedWeaponItem, 1);

        // Print the bought weapon name to the console for testing.
        Debug.Log("Bought weapon: " + selectedWeaponItem.name);

        // Update the weapon info panel after the scrap amount has changed.
        UpdateWeaponInfo();
    }

    /// <summary>
    /// Opens the upgrade panel.
    /// The main dialog panel is closed and the upgrade options are shown.
    /// </summary>
    public void OpenUpgradePanel() {
        HidePanel(dialogPanel);
        HidePanel(functionsPanel);
        HidePanel(towerInfoPanel);
        HidePanel(weaponTypePanel);
        HidePanel(weaponPanelRanged);
        HidePanel(weaponPanelClose);
        HidePanel(weaponInfoPanel);
        HidePanel(upgradeInfoPanel);

        ShowPanel(upgradePanel);
    }

    /// <summary>
    /// Goes back from the upgrade panel to the main dialog panel.
    /// </summary>
    public void BackFromUpgradePanel() {
        HidePanel(upgradePanel);
        HidePanel(upgradeInfoPanel);

        ShowPanel(dialogPanel);
    }

    /// <summary>
    /// Selects the gas tank healing upgrade and opens the upgrade info panel.
    /// </summary>
    public void SelectGasTankHealing() {
        ShowPanel(upgradePanel);
        ShowPanel(upgradeInfoPanel);

        upgradeInfoPanel.transform.SetAsLastSibling();

        UpdateGasTankHealingInfo();
    }

    /// <summary>
    /// Updates the gas tank healing upgrade info panel.
    /// It checks the current scrap amount, gas tank health state
    /// and enables or disables the heal button.
    /// </summary>
    private void UpdateGasTankHealingInfo() {
        if (playerInventory == null || scrapItem == null) {
            // If the inventory or the scrap item is missing,
            // the current scrap amount cannot be checked.
            gasTankHealScrapText.text = "SCRAP: ? / " + gasTankHealScrapCost;

            // Show the warning box because healing is not possible.
            gasTankHealWarningBox.SetActive(true);

            // Disable the heal button to prevent errors.
            gasTankHealButton.interactable = false;

            return;
        }

        if (gasTankHealth == null) {
            // If the gas tank health script is missing,
            // the gas tank cannot be healed.
            gasTankHealScrapText.text = "SCRAP: ? / " + gasTankHealScrapCost;

            // Show the warning box because healing is not possible.
            gasTankHealWarningBox.SetActive(true);

            // Disable the heal button to prevent errors.
            gasTankHealButton.interactable = false;

            return;
        }

        // Get the current amount of scrap from the player's inventory.
        int currentScrap = playerInventory.GetItemAmount(scrapItem);

        // Check if the player has enough scrap to buy the healing upgrade.
        bool hasEnoughScrap = currentScrap >= gasTankHealScrapCost;

        // Check if the gas tank is damaged and can actually be healed.
        bool gasTankNeedsHealing = gasTankHealth.CurrentHP < gasTankHealth.MaxHP;

        // The healing upgrade can only be bought if the player has enough scrap
        // and the gas tank is not already at full health.
        bool canBuyHealing = hasEnoughScrap && gasTankNeedsHealing;

        if (gasTankHealWarningText != null) {
            // Clear the warning text before creating a new warning message.
            gasTankHealWarningText.text = "";

            // Add a warning message if the player does not have enough scrap.
            if (!hasEnoughScrap) {
                gasTankHealWarningText.text += "NOT ENOUGH RESOURCES";
            }

            // Add a warning message if the gas tank is already at full health.
            if (!gasTankNeedsHealing) {
                // If there is already another warning message,
                // add a line break before adding the next message.
                if (gasTankHealWarningText.text != "") {
                    gasTankHealWarningText.text += "\n";
                }

                // Add the full health warning message.
                gasTankHealWarningText.text += "GAS-TANK ALREADY FULL";
            }
        }

        // Show current scrap amount and required scrap cost.
        gasTankHealScrapText.text = "SCRAP: " + currentScrap + " / " + gasTankHealScrapCost;

        // Show the warning box if the healing upgrade cannot be bought.
        gasTankHealWarningBox.SetActive(!canBuyHealing);

        // Enable the heal button only if the player has enough scrap
        // and the gas tank needs healing.
        gasTankHealButton.interactable = canBuyHealing;
    }

    /// <summary>
    /// Buys the gas tank healing upgrade.
    /// The required scrap amount is removed from the inventory and the gas tank health is restored.
    /// </summary>
    public void BuyGasTankHealing() {
        if (playerInventory == null || scrapItem == null) {
            // If the inventory or the scrap item is missing,
            // the healing upgrade cannot be bought correctly.
            Debug.LogError("Inventory oder Scrap Item fehlt!");
            return;
        }

        if (gasTankHealth == null) {
            // If no gas tank health script is assigned,
            // the gas tank cannot be healed.
            Debug.LogError("Gas Tank Health fehlt!");
            return;
        }

        if (!playerInventory.HasItemAmount(scrapItem, gasTankHealScrapCost)) {
            // Stop healing if the player does not have enough scrap.
            Debug.Log("Nicht genug Scrap für Gas-Tank Healing.");

            // Update the upgrade info panel so the warning and button state are correct.
            UpdateGasTankHealingInfo();

            return;
        }

        // Remove the required scrap amount from the player's inventory.
        playerInventory.RemoveItem(scrapItem, gasTankHealScrapCost);

        // Restore health to the gas tank.
        gasTankHealth.Heal(gasTankHealAmount);

        Debug.Log("Gas-Tank healed by " + gasTankHealAmount + " HP.");

        // Update the upgrade info panel after the scrap amount has changed.
        UpdateGasTankHealingInfo();

        // Clear the currently selected UI element.
        EventSystem.current.SetSelectedGameObject(null);
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


