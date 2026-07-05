using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Creates the Cheat GUI and creates the commands
/// </summary>
public class DebugController : MonoBehaviour {

    public static DebugController Instance { get; private set; }

    [SerializeField] private Inventory inventory;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private GasTankHealth objectiveHealth;
    [SerializeField] private AchievementSO achievementDay1;
    [SerializeField] private AchievementSO achievementDay5;
    [SerializeField] private AchievementSO achievementDay10;

    private GUIStyle customTextFieldStyle;

    private bool showConsole;
    private bool showHelp;
    private string input;
    private Vector2 scroll;

    public static DebugCommand HELP;
    //Gun Commands
    public static DebugCommand GIVE_ASSAULTRIFLE;
    public static DebugCommand GIVE_PISTOL;
    public static DebugCommand GIVE_SHOTGUN;
    public static DebugCommand GIVE_SNIPER;
    //Melee Commands
    public static DebugCommand GIVE_BASEBALL;
    public static DebugCommand GIVE_KNIFE;
    public static DebugCommand GIVE_HATCHET;
    public static DebugCommand GIVE_CROWBAR;
    public static DebugCommand GIVE_SWORD;
    public static DebugCommand GIVE_TOMAHAWK;
    //Health Pack Commands
    public static DebugCommand<int> GIVE_BANDAGE;
    public static DebugCommand<int> GIVE_SYRINGE;
    public static DebugCommand<int> GIVE_HEALTHBOTTLE;
    public static DebugCommand<int> GIVE_HEALTHPACK;
    //Grenade Command
    public static DebugCommand<int> GIVE_GRENADE;
    //Ammo Commands
    public static DebugCommand<int> GIVE_AMMO556;
    public static DebugCommand<int> GIVE_AMMO9;
    public static DebugCommand<int> GIVE_AMMO12G;
    public static DebugCommand<int> GIVE_AMMO_S;
    //Scrap Command
    public static DebugCommand<int> GIVE_SCRAP;
    //Kill/Destroy
    public static DebugCommand KILL_PLAYER;
    public static DebugCommand DESTROY_OBJECTIVE;
    //Achievement
    public static DebugCommand ACHIEVEMENT1;
    public static DebugCommand ACHIEVEMENT2;
    public static DebugCommand ACHIEVEMENT3;
    //Damage Player
    public static DebugCommand<int> DAMAGE_PLAYER;
    //Extraction
    public static DebugCommand EXTRACTION;

    public List<object> commandList;

    /// <summary>
    /// Initializes all the needed commands
    /// </summary>
    private void Awake() {
        Instance = this;

        HELP = new DebugCommand("help", "Shows a list of commands", "help", () => {
            showHelp = true;
        });
        //Guns
        GIVE_ASSAULTRIFLE = new DebugCommand("/give Rifle", "Puts a Assault Rifle in the Player Inventory", "/give Rifle", () => {
            inventory.AddItem(ItemType.Gun, GunType.AssaultRifle);
            inventory.AddItem(ItemType.Ammunition, AmmunitionType.Ammo556x45mm, 30);
        });
        GIVE_PISTOL = new DebugCommand("/give Pistol", "Puts a Pistol in the Player Inventory", "/give Pistol", () => {
            inventory.AddItem(ItemType.Gun, GunType.Pistol);
            inventory.AddItem(ItemType.Ammunition, AmmunitionType.Ammo9mm, 10);
        });
        GIVE_SHOTGUN = new DebugCommand("/give Shotgun", "Puts a Shotgun in the Player Inventory", "/give Shotgun", () => {
            inventory.AddItem(ItemType.Gun, GunType.Shotgun);
            inventory.AddItem(ItemType.Ammunition, AmmunitionType.Ammo12Gauge, 8);
        });
        GIVE_SNIPER = new DebugCommand("/give Sniper", "Puts a Sniper in the Player Inventory", "/give Sniper", () => {
            inventory.AddItem(ItemType.Gun, GunType.Sniper);
            inventory.AddItem(ItemType.Ammunition, AmmunitionType.AmmoSniper, 5);
        });
        //Melees
        GIVE_BASEBALL = new DebugCommand("/give Melee_B", "Puts a Baseball Bat in the Player Inventory", "/give Melee_B", () => {
            inventory.AddItem(ItemType.Melee, MeleeType.Baseball_Bat);
        });
        GIVE_CROWBAR = new DebugCommand("/give Melee_C", "Puts a Crowbar in the Player Inventory", "/give Melee_C", () => {
            inventory.AddItem(ItemType.Melee, MeleeType.Crowbar);
        });
        GIVE_HATCHET = new DebugCommand("/give Melee_H", "Puts a Hatchet in the Player Inventory", "/give Melee_H", () => {
            inventory.AddItem(ItemType.Melee, MeleeType.Hatchet);
        });
        GIVE_KNIFE = new DebugCommand("/give Melee_K", "Puts a Knife in the Player Inventory", "/give Melee_K", () => {
            inventory.AddItem(ItemType.Melee, MeleeType.Knife);
        });
        GIVE_SWORD = new DebugCommand("/give Melee_S", "Puts a Sword in the Player Inventory", "/give Melee_S", () => {
            inventory.AddItem(ItemType.Melee, MeleeType.Sword);
        });
        GIVE_TOMAHAWK = new DebugCommand("/give Melee_T", "Puts a Tomahawk in the Player Inventory", "/give Melee_T", () => {
            inventory.AddItem(ItemType.Melee, MeleeType.Tomahawk);
        });
        //Grenade
        GIVE_GRENADE = new DebugCommand<int>("/give grenade", "Puts a Grenade in the Player Inventory", "/give Grenade <amount>", (x) => {
            inventory.AddItem(ItemType.Grenade, ItemType.Grenade, x);
        });
        //Health
        GIVE_BANDAGE = new DebugCommand<int>("/give h1", "Puts a Bandage in the Player Inventory", "/give Health1 <amount>", (x) => {
            inventory.AddItem(ItemType.Consumable, HealthItemType.Bandage, x);
        });
        GIVE_SYRINGE = new DebugCommand<int>("/give h2", "Puts a Syringe in the Player Inventory", "/give Health2 <amount>", (x) => {
            inventory.AddItem(ItemType.Consumable, HealthItemType.Syringe, x);
        });
        GIVE_HEALTHBOTTLE = new DebugCommand<int>("/give h3", "Puts a Health Bottle in the Player Inventory", "/give Health3 <amount>", (x) => {
            inventory.AddItem(ItemType.Consumable, HealthItemType.HealthBottle, x);
        });
        GIVE_HEALTHPACK = new DebugCommand<int>("/give h4", "Puts a Health Pack in the Player Inventory", "/give Health4 <amount>", (x) => {
            inventory.AddItem(ItemType.Consumable, HealthItemType.HealthPack, x);
        });
        //Ammo
        GIVE_AMMO556 = new DebugCommand<int>("/give 556", "Puts a 5.56x45mm Ammo Box in the Player Inventory", "/give 556 <amount>", (x) => {
            inventory.AddItem(ItemType.Ammunition, AmmunitionType.Ammo556x45mm, x);
        });
        GIVE_AMMO9 = new DebugCommand<int>("/give 9", "Puts a 9mm Ammo Box in the Player Inventory", "/give 9", (x) => {
            inventory.AddItem(ItemType.Ammunition, AmmunitionType.Ammo9mm, x);
        });
        GIVE_AMMO12G = new DebugCommand<int>("/give 12G", "Puts 12 Gauge Bullets Ammo Box in the Player Inventory", "/give 12G <amount>", (x) => {
            inventory.AddItem(ItemType.Ammunition, AmmunitionType.Ammo12Gauge, x);
        });
        GIVE_AMMO_S = new DebugCommand<int>("/give SA", "Puts Sniper Bullets in the Player Inventory", "/give SA <amount>", (x) => {
            inventory.AddItem(ItemType.Ammunition, AmmunitionType.AmmoSniper, x);
        });
        //Scrap
        GIVE_SCRAP = new DebugCommand<int>("/give scrap", "Puts Scrap in the Player Inventory", "/give scrap <amount>", (x) => {
            inventory.AddItem(ItemType.Scrap, ItemType.Scrap, x);
        });
        //Kill/Destroy
        KILL_PLAYER = new DebugCommand("/kill", "Kills the Player", "/kill", () => {
            playerHealth.TakeDamage(999);
        });
        DESTROY_OBJECTIVE = new DebugCommand("/destroy", "destroys the Objective", "/destroy", () => {
            objectiveHealth.TakeDamage(999);
        });
        //Achievements
        ACHIEVEMENT1 = new DebugCommand("/achievement1", "gives the achievement for surviving Day 1", "/achievement1", () => {
            AchievementManager.triggerAchievement?.Invoke(achievementDay1);
        });
        ACHIEVEMENT2 = new DebugCommand("/achievement2", "gives the achievement for surviving Day 5", "/achievement2", () => {
            AchievementManager.triggerAchievement?.Invoke(achievementDay5);
        });
        ACHIEVEMENT3 = new DebugCommand("/achievement3", "gives the achievement for surviving Day 10", "/achievement3", () => {
            AchievementManager.triggerAchievement?.Invoke(achievementDay10);
        });
        //Damage Player
        DAMAGE_PLAYER = new DebugCommand<int>("/damage", "Kills the Player", "/damage <amount>", (x) => {
            playerHealth.TakeDamage(x);
        });
        //Extraction
        EXTRACTION = new DebugCommand("/extraction", "Starts the extraction scene", "/extraction", () => {
            Loader.Load(Loader.Scene.ExtractionScene);
        });

        commandList = new List<object> {
            HELP,
            GIVE_ASSAULTRIFLE,
            GIVE_PISTOL,
            GIVE_SHOTGUN,
            GIVE_SNIPER,
            GIVE_GRENADE,
            GIVE_BASEBALL,
            GIVE_CROWBAR,
            GIVE_HATCHET,
            GIVE_SWORD,
            GIVE_TOMAHAWK,
            GIVE_BANDAGE,
            GIVE_SYRINGE,
            GIVE_HEALTHBOTTLE,
            GIVE_HEALTHPACK,
            GIVE_AMMO556,
            GIVE_AMMO9,
            GIVE_AMMO12G,
            GIVE_AMMO_S,
            GIVE_SCRAP,
            KILL_PLAYER,
            DESTROY_OBJECTIVE,
            ACHIEVEMENT1,
            ACHIEVEMENT2,
            ACHIEVEMENT3,
            DAMAGE_PLAYER,
            EXTRACTION
        };
    }

    private void Start() {
        customTextFieldStyle = new GUIStyle();
    }

    /// <summary>
    /// if the console is shown and the Player presses enter 
    /// the command text gets Handled by the HandleCommandText() Function
    /// </summary>
    private void PlayerInputHandler_OnReturnAction() {
        if (showConsole) {
            HandleCommandText();
            input = "";

            showConsole = false;
        }
    }

    /// <summary>
    /// If the Player presses the Cheat Console Button it will be toggled
    /// </summary>
    private void PlayerInputHandler_OnToggleDebugAction() {
        showConsole = !showConsole;
    }

    /// <summary>
    /// The GUI gets drawn
    /// </summary>
    private void OnGUI() {
        if (!showConsole) { return; }

        float y = 0f;
        customTextFieldStyle.fontSize = 24;
        customTextFieldStyle.fontStyle = FontStyle.Bold;
        customTextFieldStyle.normal.textColor = Color.green;
        customTextFieldStyle.alignment = TextAnchor.MiddleLeft;

        if (showHelp) {
            GUI.Box(new Rect(0, y, Screen.width, 300), "");

            Rect viewport = new Rect(0, 0, Screen.width - 30, 40 * commandList.Count);
            scroll = GUI.BeginScrollView(new Rect(0, y + 5f, Screen.width, 280), scroll, viewport);

            for (int i = 0; i < commandList.Count; i++) {
                DebugCommandBase command = commandList[i] as DebugCommandBase;
                string label = $"{command.commandFormat} - {command.commandDescription}";

                Rect labelRect = new Rect(5, 25 * i, viewport.width - 100, 40);

                GUI.Label(labelRect, label, customTextFieldStyle);
            }
            GUI.EndScrollView();

            y += 300;
        }

        GUI.Box(new Rect(0, y, Screen.width, 50), "");
        GUI.backgroundColor = new Color(0, 0, 0, 0);
        input = GUI.TextField(new Rect(10f, y + 5f, Screen.width - 20f, 40f), input, customTextFieldStyle);
    }

    /// <summary>
    /// Executes what was found in the input
    /// </summary>
    private void HandleCommandText() {
        string[] properties = input.Split(' ');

        for (int i = 0; i < commandList.Count; i++) {
            DebugCommandBase commandBase = commandList[i] as DebugCommandBase;

            if (input.Contains(commandBase.commandId)) {
                if (commandList[i] as DebugCommand != null) {
                    (commandList[i] as DebugCommand).Invoke();
                } else if (commandList[i] as DebugCommand<int> != null && properties.Length == 3) {
                    (commandList[i] as DebugCommand<int>).Invoke(int.Parse(properties[2]));
                }
            }
        }
    }

    /// <summary>
    /// Getter for the console visibility
    /// </summary>
    /// <returns></returns>
    public bool GetConsoleVisibility() {
        return showConsole;
    }

    /// <summary>
    /// Subscribes all the Events
    /// </summary>
    private void OnEnable() {
        PlayerInputHandler.OnToggleDebugAction += PlayerInputHandler_OnToggleDebugAction;
        PlayerInputHandler.OnReturnAction += PlayerInputHandler_OnReturnAction;
    }

    /// <summary>
    /// Unsubscribes all the Events
    /// </summary>
    private void OnDisable() {
        PlayerInputHandler.OnToggleDebugAction -= PlayerInputHandler_OnToggleDebugAction;
        PlayerInputHandler.OnReturnAction -= PlayerInputHandler_OnReturnAction;
    }
}
