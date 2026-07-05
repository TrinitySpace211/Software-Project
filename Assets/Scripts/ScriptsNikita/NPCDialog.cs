using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Handles the NPC dialog UI, function menu, tower spawning, and weapon upgrade.
/// </summary>
public class NPCDialog : MonoBehaviour, ISaveable {

    private static readonly string ID = "NPCDialog";

    [Header("Dialog Panels")]
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

    [Header("Tower Build Settings")]
    /// <summary>
    /// The tower prefab that will be spawned.
    /// </summary>
    //public GameObject towerPrefab;

    /// <summary>
    /// Tower prefabs for each upgrade level.
    /// Element 0 is the base tower, element 1 is the first upgrade, element 2 is the second upgrade.
    /// </summary>
    public GameObject[] towerPrefabsByLevel;

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

    [Header("Inventory References")]
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
    /// UI object that contains the tower build warning message.
    /// It is shown when the player cannot build a tower.
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

    [Header("Weapon Shop Panels")]
    /// <summary>
    /// Panel where the player can choose between ranged combat and close combat weapons.
    /// </summary>
    public CanvasGroup weaponTypePanel;

    /// <summary>
    /// Panel that contains all ranged weapon buttons.
    /// </summary>
    public CanvasGroup rangedWeaponPanel;

    /// <summary>
    /// Panel that contains all close combat weapon buttons.
    /// </summary>
    public CanvasGroup closeWeaponPanel;

    /// <summary>
    /// Panel that displays detailed information about the currently selected weapon.
    /// </summary>
    public CanvasGroup weaponInfoPanel;

    [Header("Weapon Shop UI")]
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
    /// Button in the weapon type panel that opens the ranged (firearm) weapons.
    /// It is greyed out (non-interactable) in the Melee-Only and Pistol+Melee
    /// game modes, since no firearms may be bought there.
    /// Optional: if not assigned, buying is still blocked in code.
    /// </summary>
    public Button rangedWeaponsButton;

    [Header("Weapon Item References")]
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

    [Header("Upgrade Panels GasTank")]
    /// <summary>
    /// Panel that contains the available upgrade options.
    /// </summary>
    public CanvasGroup upgradePanel;

    /// <summary>
    /// Panel that displays detailed information about the gas tank healing upgrade.
    /// </summary>
    public CanvasGroup upgradeInfoPanel;

    [Header("Gas Tank Healing Upgrade")]
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

    [Header("NPC References")]
    /// <summary>
    /// Reference to the NPC movement script.
    /// It is used to stop the NPC while the dialog is open.
    /// </summary>
    public NPCNavMeshWander npcWander;

    [Header("Tower Upgrade UI")]
    /// <summary>
    /// Panel that displays detailed information about the tower upgrade.
    /// </summary>
    public CanvasGroup upgradeInfoPanelTower;

    /// <summary>
    /// Amount of scrap required to upgrade one tower.
    /// </summary>
    public int towerUpgradeScrapCost = 200;

    /// <summary>
    /// Text element that displays the current scrap amount and required scrap cost for the tower upgrade.
    /// </summary>
    public TMP_Text towerUpgradeScrapText;

    /// <summary>
    /// Warning box that is shown if the tower upgrade cannot be bought.
    /// </summary>
    public GameObject towerUpgradeWarningBox;

    /// <summary>
    /// Text element inside the tower upgrade warning box.
    /// It displays why the tower upgrade cannot be bought.
    /// </summary>
    public TMP_Text towerUpgradeWarningText;

    /// <summary>
    /// Button used to buy the tower upgrade.
    /// </summary>
    public Button towerUpgradeButton;

    /// <summary>
    /// Number of tower upgrades that have already been bought.
    /// A maximum of 16 upgrades is possible because 8 towers can each be upgraded 2 times.
    /// </summary>
    private int boughtTowerUpgradeCount = 0;

    private GameObject[] builtTowers;
    private int[] towerUpgradeLevels;

    [Header("Weapon Upgrade UI")]
    /// <summary>
    /// Left-side panel where the player can choose between weapon damage and ammo upgrades.
    /// </summary>
    public CanvasGroup weaponTypeUpgradePanel;

    /// <summary>
    /// Right-side info panel that shows the currently selected weapon upgrade information.
    /// </summary>
    public CanvasGroup upgradeInfoPanelWeapon;

    /// <summary>
    /// Title text of the selected weapon upgrade.
    /// </summary>
    public TMP_Text weaponUpgradeTitleText;

    /// <summary>
    /// Description text of the selected weapon upgrade.
    /// </summary>
    public TMP_Text weaponUpgradeDescriptionText;

    /// <summary>
    /// Effect text of the selected weapon upgrade.
    /// </summary>
    public TMP_Text weaponUpgradeEffectText;

    /// <summary>
    /// Scrap cost text of the selected weapon upgrade.
    /// </summary>
    public TMP_Text weaponUpgradeScrapText;

    /// <summary>
    /// Warning box for weapon upgrade problems.
    /// </summary>
    public GameObject weaponUpgradeWarningBox;

    /// <summary>
    /// Warning text for weapon upgrade problems.
    /// </summary>
    public TMP_Text weaponUpgradeWarningText;

    /// <summary>
    /// Button used to buy the selected weapon upgrade.
    /// </summary>
    public Button weaponUpgradeBuyButton;

    [Header("Weapon Upgrade Values")]
    /// <summary>
    /// Scrap cost for increasing ranged weapon damage.
    /// </summary>
    public int weaponDamageUpgradeScrapCost = 120;

    /// <summary>
    /// Scrap cost for increasing ranged weapon ammo capacity.
    /// </summary>
    public int weaponAmmoUpgradeScrapCost = 100;

    /// <summary>
    /// Damage increase for the selected ranged weapon.
    /// </summary>
    public int weaponDamageIncrease = 5;

    /// <summary>
    /// Ammo capacity increase for the selected ranged weapon.
    /// </summary>
    public int weaponAmmoIncrease = 10;

    /// <summary>
    /// Stores which weapon upgrade type is currently selected.
    /// </summary>
    private WeaponUpgradeType selectedWeaponUpgradeType = WeaponUpgradeType.None;

    /// <summary>
    /// Defines the currently selected weapon upgrade type.
    /// </summary>
    private enum WeaponUpgradeType {
        None,
        Damage,
        Ammo
    }

    [Header("Weapon Shop Gun References")]
    /// <summary>
    /// GunSO reference for the shotgun.
    /// This is used to display upgraded weapon values in the shop UI.
    /// </summary>
    public GunSO shotgunGun;

    /// <summary>
    /// GunSO reference for the assault rifle.
    /// This is used to display upgraded weapon values in the shop UI.
    /// </summary>
    public GunSO assaultRifleGun;

    /// <summary>
    /// GunSO reference for the sniper rifle.
    /// This is used to display upgraded weapon values in the shop UI.
    /// </summary>
    public GunSO sniperGun;

    /// <summary>
    /// GunSO reference for the pistol.
    /// This is used to display upgraded weapon values in the shop UI.
    /// </summary>
    public GunSO pistolGun;

    [Header("Weapon Upgrade References")]
    /// <summary>
    /// Reference to the player weapon selector.
    /// This is used to get the currently equipped gun for weapon upgrades.
    /// </summary>
    public PlayerWeaponSelector playerWeaponSelector;

    private void Awake() {
        InitializeTowerState();
    }

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
        HidePanel(upgradeInfoPanelTower);

        builtTowers = new GameObject[towerSpawnPoints.Length];
        towerUpgradeLevels = new int[towerSpawnPoints.Length];

        ApplyGameModeRestrictions();

        // Force Unity to update the canvas layout immediately
        Canvas.ForceUpdateCanvases();
    }

    /// <summary>
    /// Returns whether firearms may be bought in the current game mode.
    /// The shop only sells assault rifle, shotgun and sniper, which are all
    /// forbidden in Melee-Only and Pistol+Melee (the pistol is not sold here).
    /// </summary>
    private bool IsRangedShopAllowed() {
        return GameMode.Selected != GameModeType.MeleeOnly
            && GameMode.Selected != GameModeType.PistolMelee;
    }

    /// <summary>
    /// Applies the current game mode to the shop UI. In weapon-restricted modes
    /// the ranged weapons button is greyed out so the player can see it is disabled.
    /// </summary>
    private void ApplyGameModeRestrictions() {
        if (rangedWeaponsButton != null) {
            rangedWeaponsButton.interactable = IsRangedShopAllowed();
        }
    }

    /// <summary>
    /// Updates visible NPC dialog info panels while the dialog is open.
    /// This keeps costs, warnings and button states up to date when the player inventory,
    /// selected weapon or available upgrades change during the dialog.
    /// </summary>
    private void Update() {

        // Do not update any NPC dialog UI while the dialog is closed
        if (!IsDialogOpen)
            return;

        // Update the tower build info only while the tower info panel is visible.
        if (towerInfoPanel != null && towerInfoPanel.alpha > 0f)
            UpdateTowerInfo();

        // Update the gas tank healing info only while the gas tank upgrade info panel is visible.
        if (upgradeInfoPanel != null && upgradeInfoPanel.alpha > 0f)
            UpdateGasTankHealingInfo();

        // Update the tower upgrade info only while the tower upgrade info panel is visible.
        if (upgradeInfoPanelTower != null && upgradeInfoPanelTower.alpha > 0f)
            UpdateTowerUpgradeInfo();

        // Update the weapon upgrade info only while the weapon upgrade info panel is visible.
        if (upgradeInfoPanelWeapon != null && upgradeInfoPanelWeapon.alpha > 0f)
            UpdateWeaponUpgradeInfo();

        // Update the weapon info only while the weapon info panel is visible.
        if (weaponInfoPanel != null && weaponInfoPanel.alpha > 0f)
            UpdateWeaponInfo();
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

        if (npcWander != null) {
            npcWander.StopMovement();
        }

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

        // Hide all left-side panels.
        HidePanel(dialogPanel);
        HidePanel(functionsPanel);
        HidePanel(weaponTypePanel);
        HidePanel(rangedWeaponPanel);
        HidePanel(closeWeaponPanel);
        HidePanel(upgradePanel);
        HidePanel(weaponTypeUpgradePanel);

        // Hide all right-side info panels.
        HidePanel(towerInfoPanel);
        HidePanel(weaponInfoPanel);
        HidePanel(upgradeInfoPanel);
        HidePanel(upgradeInfoPanelTower);
        HidePanel(upgradeInfoPanelWeapon);

        // Unlock and show the cursor
        Cursor.lockState = CursorLockMode.None;

        // Keep the cursor visible only if the inventory is currently open.
        if (playerInventory.container.gameObject.activeInHierarchy) {
            Cursor.visible = true;
        } else if (!playerInventory.container.gameObject.activeInHierarchy) {
            Cursor.visible = false;
        }

        if (npcWander != null) {
            npcWander.ResumeMovement();
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
        GameObject newTower = Instantiate(towerPrefabsByLevel[0], selectedSpawnPoint.position, selectedSpawnPoint.rotation);

        builtTowers[builtTowerCount] = newTower;
        towerUpgradeLevels[builtTowerCount] = 0;

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
        HidePanel(rangedWeaponPanel);
        HidePanel(closeWeaponPanel);
        HidePanel(weaponInfoPanel);

        ShowPanel(weaponTypePanel);
    }

    public void OpenRangedWeapons() {
        // Spielmodus: In Melee-Only / Pistol+Melee sind keine Schusswaffen kaufbar.
        if (!IsRangedShopAllowed()) {
            return;
        }

        HidePanel(weaponTypePanel);
        HidePanel(weaponInfoPanel);

        ShowPanel(rangedWeaponPanel);
    }

    public void OpenCloseCombatWeapons() {
        HidePanel(weaponTypePanel);
        HidePanel(weaponInfoPanel);

        ShowPanel(closeWeaponPanel);
    }

    public void BackToFunctionsFromWeaponTypes() {
        HidePanel(weaponTypePanel);
        HidePanel(rangedWeaponPanel);
        HidePanel(closeWeaponPanel);
        HidePanel(weaponInfoPanel);

        ShowPanel(functionsPanel);
    }

    public void BackToWeaponTypes() {
        HidePanel(rangedWeaponPanel);
        HidePanel(closeWeaponPanel);
        HidePanel(weaponInfoPanel);

        ShowPanel(weaponTypePanel);
    }

    // WEG
    public void OpenWeapons() {
        HidePanel(functionsPanel);
        HidePanel(towerInfoPanel);

        ShowPanel(rangedWeaponPanel);
        HidePanel(weaponInfoPanel);
    }

    // WEG
    public void BackToFunctionsFromWeapons() {
        HidePanel(rangedWeaponPanel);
        HidePanel(weaponInfoPanel);

        ShowPanel(functionsPanel);
    }

    /// <summary>
    /// Returns the GunSO that belongs to the selected weapon item.
    /// Only ranged weapons have a matching GunSO.
    /// </summary>
    private GunSO GetGunForSelectedWeaponItem() {
        if (selectedWeaponItem == shotgunItem)
            return shotgunGun;

        if (selectedWeaponItem == assaultRifleItem)
            return assaultRifleGun;

        if (selectedWeaponItem == sniperItem)
            return sniperGun;

        return null;
    }

    private void UpdateWeaponInfo() {
        // If the inventory or the scrap item was not assigned,
        // show an unknown scrap amount and disable buying.
        if (playerInventory == null || scrapItem == null) {
            weaponScrapAmountText.text = "SCRAP: ? / " + selectedWeaponCost;

            // Show the current damage value of the selected weapon.
            // Ranged weapons use their GunSO value so upgrades are displayed correctly.
            // Melee weapons still use the base damage from the ItemSO.
            if (selectedWeaponItem == null) {
                weaponDamageText.text = "?";
            } else {
                GunSO selectedGun = GetGunForSelectedWeaponItem();

                if (selectedGun != null) {
                    weaponDamageText.text = "" + selectedGun.GetDamage();
                } else {
                    weaponDamageText.text = "" + selectedWeaponItem.baseDamage;
                }
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

        // Show the current damage value of the selected weapon.
        // Ranged weapons use their GunSO value so upgrades are displayed correctly.
        // Melee weapons still use the base damage from the ItemSO.
        if (selectedWeaponItem == null) {
            weaponDamageText.text = "?";
        } else {
            GunSO selectedGun = GetGunForSelectedWeaponItem();

            if (selectedGun != null) {
                weaponDamageText.text = "" + selectedGun.GetDamage();
            } else {
                weaponDamageText.text = "" + selectedWeaponItem.baseDamage;
            }
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
        selectedWeaponCost = 10;

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
        selectedWeaponCost = 25;

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
        selectedWeaponCost = 30;

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
        selectedWeaponCost = 12;

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
        selectedWeaponCost = 5;

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
        selectedWeaponCost = 18;

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
        selectedWeaponCost = 15;

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
        selectedWeaponCost = 8;

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
        selectedWeaponCost = 25;

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
        HidePanel(rangedWeaponPanel);
        HidePanel(closeWeaponPanel);
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
        HidePanel(upgradeInfoPanelTower);
        HidePanel(upgradeInfoPanelWeapon);
        HidePanel(weaponTypeUpgradePanel);

        ShowPanel(dialogPanel);
    }

    /// <summary>
    /// Selects the gas tank healing upgrade and opens the upgrade info panel.
    /// </summary>
    public void SelectGasTankHealing() {
        HidePanel(upgradeInfoPanelTower);
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
    /// Selects the tower upgrade and opens the tower upgrade info panel.
    /// </summary>
    public void SelectTowerUpgrade() {
        ShowPanel(upgradePanel);
        HidePanel(upgradeInfoPanel);
        ShowPanel(upgradeInfoPanelTower);

        upgradeInfoPanelTower.transform.SetAsLastSibling();

        UpdateTowerUpgradeInfo();
    }

    /// <summary>
    /// Checks if at least one built tower can still be upgraded.
    /// </summary>
    private bool HasUpgradeableTower() {
        if (builtTowers == null || towerUpgradeLevels == null || towerPrefabsByLevel == null)
            return false;

        int maxUpgradeLevel = towerPrefabsByLevel.Length - 1;

        for (int i = 0; i < builtTowers.Length; i++) {
            if (builtTowers[i] != null && towerUpgradeLevels[i] < maxUpgradeLevel) {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Updates the tower upgrade info panel.
    /// It checks the current scrap amount and if at least one tower exists.
    /// </summary>
    private void UpdateTowerUpgradeInfo() {
        if (playerInventory == null || scrapItem == null) {
            // If the inventory or the scrap item is missing,
            // the current scrap amount cannot be checked.
            towerUpgradeScrapText.text = "SCRAP: ? / " + towerUpgradeScrapCost;

            // Show the warning box because upgrading is not possible.
            towerUpgradeWarningBox.SetActive(true);

            // Disable the upgrade button to prevent errors.
            towerUpgradeButton.interactable = false;

            return;
        }

        // Get the current amount of scrap from the player's inventory.
        int currentScrap = playerInventory.GetItemAmount(scrapItem);

        // Check if the player has enough scrap to buy the tower upgrade.
        bool hasEnoughScrap = currentScrap >= towerUpgradeScrapCost;

        // Check if at least one tower has already been built.
        bool hasBuiltTower = builtTowerCount > 0;

        // Check whether there is a tower that can be updated
        bool hasUpgradeableTower = HasUpgradeableTower();

        // The tower upgrade can only be bought if the player has enough scrap
        // and at least one tower exists and tower can be upgraded.
        bool canBuyTowerUpgrade = hasEnoughScrap && hasBuiltTower && hasUpgradeableTower;

        // Show current scrap amount and required scrap cost.
        towerUpgradeScrapText.text = "SCRAP: " + currentScrap + " / " + towerUpgradeScrapCost;

        if (towerUpgradeWarningText != null) {
            // Clear the warning text before creating a new warning message.
            towerUpgradeWarningText.text = "";

            // Add a warning message if the player does not have enough scrap.
            if (!hasEnoughScrap) {
                towerUpgradeWarningText.text += "NOT ENOUGH RESOURCES";
            }

            // Add a warning message if no tower exists yet.
            if (!hasBuiltTower) {
                if (towerUpgradeWarningText.text != "") {
                    towerUpgradeWarningText.text += "\n";
                }

                towerUpgradeWarningText.text += "NO TOWER BUILT";
            }

            // Add a warning message if all possible tower upgrades have already been bought.
            if (hasBuiltTower && !hasUpgradeableTower) {
                if (towerUpgradeWarningText.text != "") {
                    towerUpgradeWarningText.text += "\n";
                }

                towerUpgradeWarningText.text += "ALL TOWERS UPGRADED";
            }
        }

        // Show the warning box if the tower upgrade cannot be bought.
        towerUpgradeWarningBox.SetActive(!canBuyTowerUpgrade);

        // Enable the upgrade button only if the player has enough scrap
        // and at least one tower exists.
        towerUpgradeButton.interactable = canBuyTowerUpgrade;
    }

    /// <summary>
    /// Buys a tower upgrade.
    /// A random built tower that has not reached the maximum upgrade level is replaced by the next tower prefab level.
    /// </summary>
    public void BuyTowerUpgrade() {
        if (playerInventory == null || scrapItem == null) {
            Debug.LogError("Inventory oder Scrap Item fehlt!");
            return;
        }

        if (towerPrefabsByLevel == null || towerPrefabsByLevel.Length < 3) {
            Debug.LogError("Tower Prefabs By Level ist nicht korrekt eingerichtet!");
            return;
        }

        if (builtTowers == null || towerUpgradeLevels == null) {
            Debug.LogError("Tower arrays wurden nicht initialisiert!");
            return;
        }

        if (builtTowerCount <= 0) {
            Debug.Log("Es wurde noch kein Tower gebaut.");
            UpdateTowerUpgradeInfo();
            return;
        }

        if (!playerInventory.HasItemAmount(scrapItem, towerUpgradeScrapCost)) {
            Debug.Log("Nicht genug Scrap für Tower Upgrade.");
            UpdateTowerUpgradeInfo();
            return;
        }

        if (!HasUpgradeableTower()) {
            UpdateTowerUpgradeInfo();
            return;
        }

        // Find all built towers that can still be upgraded.
        int[] upgradeableTowerIndexes = new int[builtTowers.Length];
        int upgradeableTowerCount = 0;

        for (int i = 0; i < builtTowers.Length; i++) {
            if (builtTowers[i] != null && towerUpgradeLevels[i] < towerPrefabsByLevel.Length - 1) {
                upgradeableTowerIndexes[upgradeableTowerCount] = i;
                upgradeableTowerCount++;
            }
        }

        if (upgradeableTowerCount <= 0) {
            Debug.Log("Alle Tower sind bereits vollständig geupgradet.");
            UpdateTowerUpgradeInfo();
            return;
        }

        // Select one upgradeable tower randomly.
        int randomListIndex = UnityEngine.Random.Range(0, upgradeableTowerCount);
        int selectedTowerIndex = upgradeableTowerIndexes[randomListIndex];

        // Calculate the next upgrade level.
        int nextUpgradeLevel = towerUpgradeLevels[selectedTowerIndex] + 1;

        if (towerPrefabsByLevel[nextUpgradeLevel] == null) {
            Debug.LogError("Tower Prefab fehlt für Upgrade Level: " + nextUpgradeLevel);
            return;
        }

        if (towerSpawnPoints == null ||
            selectedTowerIndex >= towerSpawnPoints.Length ||
            towerSpawnPoints[selectedTowerIndex] == null) {
            Debug.LogError("Tower SpawnPoint fehlt für Tower Index: " + selectedTowerIndex);
            return;
        }

        // Remove the required scrap amount from the player's inventory.
        playerInventory.RemoveItem(scrapItem, towerUpgradeScrapCost);

        // Use the original spawn point position and rotation instead of the current tower rotation.
        // This prevents the upgraded tower from inheriting the current aiming rotation of the old tower.
        Vector3 towerPosition = towerSpawnPoints[selectedTowerIndex].position;
        Quaternion towerRotation = towerSpawnPoints[selectedTowerIndex].rotation;

        // Destroy the old tower.
        Destroy(builtTowers[selectedTowerIndex]);

        // Create the upgraded tower at the original spawn point position and rotation.
        GameObject upgradedTower = Instantiate(
            towerPrefabsByLevel[nextUpgradeLevel],
            towerPosition,
            towerRotation
        );

        // Store the new upgraded tower and its upgrade level.
        builtTowers[selectedTowerIndex] = upgradedTower;
        towerUpgradeLevels[selectedTowerIndex] = nextUpgradeLevel;

        // Count the bought upgrade.
        boughtTowerUpgradeCount++;

        //Debug.Log("Tower at index " + selectedTowerIndex + " upgraded to level " + nextUpgradeLevel + ".");

        // Update the tower upgrade info panel after the scrap amount and upgrade state have changed.
        UpdateTowerUpgradeInfo();

        // Update the normal tower info panel as well, because tower values may have changed.
        UpdateTowerInfo();

        // Clear the currently selected UI element.
        EventSystem.current.SetSelectedGameObject(null);
    }

    // <summary>
    /// Opens the weapon upgrade panel from the main upgrade panel.
    /// </summary>
    public void OpenWeaponTypeUpgradePanel() {
        // Hide the current upgrade panel.
        HidePanel(upgradePanel);

        // Hide all right-side info panels.
        HidePanel(upgradeInfoPanel);
        HidePanel(upgradeInfoPanelTower);
        HidePanel(upgradeInfoPanelWeapon);

        // Show the weapon upgrade selection panel.
        ShowPanel(weaponTypeUpgradePanel);

        // No weapon upgrade type is selected at the beginning.
        selectedWeaponUpgradeType = WeaponUpgradeType.None;

        // Clear the currently selected UI element.
        EventSystem.current.SetSelectedGameObject(null);
    }

    /// <summary>
    /// Returns from the weapon upgrade panel back to the main upgrade panel.
    /// </summary>
    public void BackFromWeaponUpgradePanel() {
        // Hide the weapon upgrade panels.
        HidePanel(weaponTypeUpgradePanel);
        HidePanel(upgradeInfoPanelWeapon);

        // Show the main upgrade panel again.
        ShowPanel(upgradePanel);

        // Clear the selected upgrade type.
        selectedWeaponUpgradeType = WeaponUpgradeType.None;

        // Clear the currently selected UI element.
        EventSystem.current.SetSelectedGameObject(null);
    }

    //// <summary>
    /// Selects the damage upgrade for the currently equipped ranged weapon.
    /// </summary>
    public void SelectWeaponDamageUpgrade() {
        selectedWeaponUpgradeType = WeaponUpgradeType.Damage;

        ShowPanel(upgradeInfoPanelWeapon);

        if (weaponUpgradeTitleText != null)
            weaponUpgradeTitleText.text = "DAMAGE UPGRADE";

        if (weaponUpgradeDescriptionText != null)
            weaponUpgradeDescriptionText.text = "Increases the damage of the currently equipped ranged weapon.";

        if (weaponUpgradeEffectText != null)
            weaponUpgradeEffectText.text = "DAMAGE";

        UpdateWeaponUpgradeInfo();

        EventSystem.current.SetSelectedGameObject(null);
    }

    /// <summary>
    /// Selects the ammo upgrade for the currently equipped ranged weapon.
    /// </summary>
    public void SelectWeaponAmmoUpgrade() {
        selectedWeaponUpgradeType = WeaponUpgradeType.Ammo;

        ShowPanel(upgradeInfoPanelWeapon);

        if (weaponUpgradeTitleText != null)
            weaponUpgradeTitleText.text = "AMMO UPGRADE";

        if (weaponUpgradeDescriptionText != null)
            weaponUpgradeDescriptionText.text = "Increases the maximum ammo capacity of the currently equipped ranged weapon.";

        if (weaponUpgradeEffectText != null)
            weaponUpgradeEffectText.text = "MAX AMMO";

        UpdateWeaponUpgradeInfo();

        EventSystem.current.SetSelectedGameObject(null);
    }

    /// <summary>
    /// Returns from the weapon upgrade panel back to the main upgrade panel.
    /// </summary>
    public void BackFromWeaponTypeUpgradePanel() {
        // Hide the weapon upgrade selection panel.
        HidePanel(weaponTypeUpgradePanel);

        // Hide the weapon upgrade info panel on the right side.
        HidePanel(upgradeInfoPanelWeapon);

        // Show the main upgrade panel again.
        ShowPanel(upgradePanel);

        // Clear the selected weapon upgrade type.
        selectedWeaponUpgradeType = WeaponUpgradeType.None;

        // Clear the currently selected UI element.
        EventSystem.current.SetSelectedGameObject(null);
    }

    /// <summary>
    /// Updates the weapon upgrade info panel depending on the selected upgrade type,
    /// the currently equipped gun, the available scrap and the upgrade limit.
    /// </summary>
    private void UpdateWeaponUpgradeInfo() {
        if (playerInventory == null || scrapItem == null)
            return;

        GunSO currentGun = GetCurrentSelectedGun();

        int currentScrap = playerInventory.GetItemAmount(scrapItem);
        int requiredScrap = GetSelectedWeaponUpgradeCost();

        if (weaponUpgradeScrapText != null) {
            weaponUpgradeScrapText.text = "SCRAP: " + currentScrap + " / " + requiredScrap;
        }

        bool hasSelectedUpgrade = selectedWeaponUpgradeType != WeaponUpgradeType.None;
        bool hasSelectedGun = currentGun != null;
        bool hasEnoughScrap = currentScrap >= requiredScrap;
        bool canUpgradeSelectedType = true;

        if (hasSelectedGun && hasSelectedUpgrade) {
            if (selectedWeaponUpgradeType == WeaponUpgradeType.Damage) {
                canUpgradeSelectedType = currentGun.CanUpgradeDamage();
            } else if (selectedWeaponUpgradeType == WeaponUpgradeType.Ammo) {
                canUpgradeSelectedType = currentGun.CanUpgradeMaxAmmo();
            }
        }

        string warningMessage = "";

        if (!hasSelectedUpgrade) {
            warningMessage += "NO UPGRADE SELECTED";
        }

        if (!hasSelectedGun) {
            if (warningMessage.Length > 0)
                warningMessage += "\n";

            warningMessage += "    NO RANGED WEAPON SELECTED";
        }

        if (!canUpgradeSelectedType) {
            if (warningMessage.Length > 0)
                warningMessage += "\n";

            warningMessage += "UPGRADE ALREADY MAXED";
        }

        if (!hasEnoughScrap) {
            if (warningMessage.Length > 0)
                warningMessage += "\n";

            warningMessage += "NOT ENOUGH RESOURCES";
        }

        bool canBuyUpgrade = hasSelectedUpgrade && hasSelectedGun && hasEnoughScrap && canUpgradeSelectedType;

        if (weaponUpgradeWarningText != null)
            weaponUpgradeWarningText.text = warningMessage;

        if (weaponUpgradeWarningBox != null)
            weaponUpgradeWarningBox.SetActive(!canBuyUpgrade);

        if (weaponUpgradeBuyButton != null)
            weaponUpgradeBuyButton.interactable = canBuyUpgrade;
    }

    /// <summary>
    /// Returns the scrap cost of the currently selected weapon upgrade.
    /// </summary>
    private int GetSelectedWeaponUpgradeCost() {
        switch (selectedWeaponUpgradeType) {
            case WeaponUpgradeType.Damage:
                return weaponDamageUpgradeScrapCost;

            case WeaponUpgradeType.Ammo:
                return weaponAmmoUpgradeScrapCost;

            default:
                return 0;
        }
    }

    /// <summary>
    /// Returns the currently equipped gun of the player.
    /// </summary>
    private GunSO GetCurrentSelectedGun() {
        if (playerWeaponSelector == null)
            return null;

        return playerWeaponSelector.activeGun;
    }

    /// <summary>
    /// Buys the currently selected weapon upgrade and applies it to the currently equipped gun.
    /// </summary>
    public void BuySelectedWeaponUpgrade() {
        if (playerInventory == null || scrapItem == null)
            return;

        GunSO currentGun = GetCurrentSelectedGun();

        if (currentGun == null) {
            UpdateWeaponUpgradeInfo();
            return;
        }

        if (selectedWeaponUpgradeType == WeaponUpgradeType.None) {
            UpdateWeaponUpgradeInfo();
            return;
        }

        if (selectedWeaponUpgradeType == WeaponUpgradeType.Damage && !currentGun.CanUpgradeDamage()) {
            UpdateWeaponUpgradeInfo();
            return;
        }

        if (selectedWeaponUpgradeType == WeaponUpgradeType.Ammo && !currentGun.CanUpgradeMaxAmmo()) {
            UpdateWeaponUpgradeInfo();
            return;
        }

        int requiredScrap = GetSelectedWeaponUpgradeCost();

        if (!playerInventory.HasItemAmount(scrapItem, requiredScrap)) {
            UpdateWeaponUpgradeInfo();
            return;
        }

        bool removedScrap = playerInventory.RemoveItem(scrapItem, requiredScrap);

        if (!removedScrap) {
            UpdateWeaponUpgradeInfo();
            return;
        }

        if (selectedWeaponUpgradeType == WeaponUpgradeType.Damage) {
            currentGun.UpgradeDamage(weaponDamageIncrease);
            //Debug.Log("Damage upgrade bought for: " + currentGun.gunName);
        } else if (selectedWeaponUpgradeType == WeaponUpgradeType.Ammo) {
            currentGun.UpgradeMaxAmmo(weaponAmmoIncrease);
            //Debug.Log("Ammo upgrade bought for: " + currentGun.gunName);
        }

        UpdateWeaponUpgradeInfo();

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

    private void InitializeTowerState() {
        if (towerSpawnPoints == null) {
            builtTowers = Array.Empty<GameObject>();
            towerUpgradeLevels = Array.Empty<int>();
            builtTowerCount = 0;
            boughtTowerUpgradeCount = 0;
            return;
        }

        builtTowers = new GameObject[towerSpawnPoints.Length];
        towerUpgradeLevels = new int[towerSpawnPoints.Length];
        builtTowerCount = 0;
        boughtTowerUpgradeCount = 0;
    }

    private void RebuildTowersFromSaveData() {
        if (builtTowers == null) {
            builtTowers = Array.Empty<GameObject>();
        }

        for (int i = 0; i < builtTowers.Length; i++) {
            if (builtTowers[i] != null) {
                Destroy(builtTowers[i]);
                builtTowers[i] = null;
            }
        }

        if (towerSpawnPoints == null || towerPrefabsByLevel == null || towerUpgradeLevels == null) {
            return;
        }

        int maxTowerIndex = Mathf.Min(builtTowerCount, towerSpawnPoints.Length);

        for (int i = 0; i < maxTowerIndex; i++) {
            if (towerSpawnPoints[i] == null) {
                continue;
            }

            int level = Mathf.Clamp(i < towerUpgradeLevels.Length ? towerUpgradeLevels[i] : 0, 0, towerPrefabsByLevel.Length - 1);

            if (towerPrefabsByLevel[level] == null) {
                continue;
            }

            GameObject tower = Instantiate(
                towerPrefabsByLevel[level],
                towerSpawnPoints[i].position,
                towerSpawnPoints[i].rotation
            );

            builtTowers[i] = tower;
        }
    }

    public string GetSaveID() => ID;
    public object Save() {
        int upgradeCount = 0;

        if (towerUpgradeLevels != null) {
            for (int i = 0; i < towerUpgradeLevels.Length; i++) {
                upgradeCount += towerUpgradeLevels[i];
            }
        }

        return new NPCDialogData {
            builtTowerCount = builtTowerCount,
            boughtTowerUpgradeCount = upgradeCount,
            towerUpgradeLevels = towerUpgradeLevels != null ? (int[])towerUpgradeLevels.Clone() : Array.Empty<int>()
        };
    }

    public void Load(object data) {
        NPCDialogData dialogData = (NPCDialogData)data;

        InitializeTowerState();

        builtTowerCount = dialogData.builtTowerCount;
        boughtTowerUpgradeCount = dialogData.boughtTowerUpgradeCount;

        if (dialogData.towerUpgradeLevels != null) {
            towerUpgradeLevels = (int[])dialogData.towerUpgradeLevels.Clone();
        } else {
            towerUpgradeLevels = Array.Empty<int>();
        }

        if (towerSpawnPoints != null && towerUpgradeLevels.Length < towerSpawnPoints.Length) {
            int[] resizedLevels = new int[towerSpawnPoints.Length];
            Array.Copy(towerUpgradeLevels, resizedLevels, towerUpgradeLevels.Length);
            towerUpgradeLevels = resizedLevels;
        }

        if (builtTowerCount < 0) {
            builtTowerCount = 0;
        }

        if (towerSpawnPoints != null && builtTowerCount > towerSpawnPoints.Length) {
            builtTowerCount = towerSpawnPoints.Length;
        }

        RebuildTowersFromSaveData();
        UpdateTowerInfo();
        UpdateTowerUpgradeInfo();
    }

    [Serializable]
    public class NPCDialogData {
        public int builtTowerCount;
        public int boughtTowerUpgradeCount;
        public int[] towerUpgradeLevels;
    }

    private void OnEnable() {
        if (SaveManager.Instance != null)
            SaveManager.Instance.Register(this);
    }

    private void OnDisable() {
        if (SaveManager.Instance != null)
            SaveManager.Instance.Unregister(this);
    }
}


