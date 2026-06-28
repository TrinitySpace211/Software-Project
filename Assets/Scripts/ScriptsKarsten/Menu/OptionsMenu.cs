using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

/// <summary>
/// Options menu handling audio + input rebinding (Player InputActions).
/// Saves everything via PlayerPrefs (including Input bindings as JSON).
/// </summary>
public class OptionsMenu : MonoBehaviour {
    [Header("Audio")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private TextMeshProUGUI masterVolumeValueText;

    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private TextMeshProUGUI musicVolumeValueText;

    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private TextMeshProUGUI sfxVolumeValueText;

    [Header("Input Actions")]
    [SerializeField] private InputActionAsset inputActions;

    [Header("Move Rebind UI")]
    [SerializeField] private TextMeshProUGUI moveUpText;
    [SerializeField] private TextMeshProUGUI moveDownText;
    [SerializeField] private TextMeshProUGUI moveLeftText;
    [SerializeField] private TextMeshProUGUI moveRightText;

    [Header("Other Rebind UI")]
    [SerializeField] private TextMeshProUGUI sprintText;
    [SerializeField] private TextMeshProUGUI interactText;
    [SerializeField] private TextMeshProUGUI useText;
    [SerializeField] private TextMeshProUGUI throwText;
    [SerializeField] private TextMeshProUGUI reloadText;
    [SerializeField] private TextMeshProUGUI mapText;

    private InputAction moveAction;
    private InputAction sprintAction;
    private InputAction interactAction;
    private InputAction useAction;
    private InputAction throwAction;
    private InputAction reloadAction;
    private InputAction mapAction;


    private const string RebindKey = "rebinds";


    private void Start() {
        LoadVolumes();
        LoadRebinds();

        CacheActions();
        RefreshAllUI();
    }

    // ---------------- AUDIO ----------------

    public void SetMasterVolume(float value) {
        PlayerPrefs.SetFloat("master_volume", value);
        AudioListener.volume = value;
    }

    public void SetMusicVolume(float value) {
        PlayerPrefs.SetFloat("music_volume", value);
    }

    public void SetSfxVolume(float value) {
        PlayerPrefs.SetFloat("sfx_volume", value);
    }

    private void LoadVolumes() {
        float master = PlayerPrefs.GetFloat("master_volume", 1f);
        float music = PlayerPrefs.GetFloat("music_volume", 1f);
        float sfx = PlayerPrefs.GetFloat("sfx_volume", 1f);

        masterVolumeSlider.SetValueWithoutNotify(master);
        musicVolumeSlider.SetValueWithoutNotify(music);
        sfxVolumeSlider.SetValueWithoutNotify(sfx);

        AudioListener.volume = master;
    }

    // ---------------- INPUT ----------------

    private void CacheActions() {
        var map = inputActions.FindActionMap("Player");

        moveAction = map.FindAction("Move");
        sprintAction = map.FindAction("Sprint");
        interactAction = map.FindAction("Interact");
        useAction = map.FindAction("Use");
        throwAction = map.FindAction("Throw");
        reloadAction = map.FindAction("Reloading");
        mapAction = map.FindAction("OpenMap");

    }

    // ---------------- MOVE REBIND ----------------

    public void RebindMoveUp() => RebindComposite(moveAction, "up", moveUpText);
    public void RebindMoveDown() => RebindComposite(moveAction, "down", moveDownText);
    public void RebindMoveLeft() => RebindComposite(moveAction, "left", moveLeftText);
    public void RebindMoveRight() => RebindComposite(moveAction, "right", moveRightText);

    // ---------------- SIMPLE ACTION REBIND ----------------

    public void RebindSprint() => RebindButton(sprintAction, sprintText);
    public void RebindInteract() => RebindButton(interactAction, interactText);
    public void RebindUse() => RebindButton(useAction, useText);
    public void RebindThrow() => RebindButton(throwAction, throwText);
    public void RebindReload() => RebindButton(reloadAction, reloadText);
    public void RebindMap() => RebindButton(mapAction, mapText);


    // ---------------- CORE REBIND SYSTEM ----------------

    private void RebindButton(InputAction action, TextMeshProUGUI label) {
        action.Disable();

        action.PerformInteractiveRebinding()
            .WithControlsExcluding("Mouse")
            .OnComplete(op => {
                op.Dispose();
                action.Enable();
                SaveRebinds();
                RefreshAllUI();
            })
            .Start();
    }

    private void RebindComposite(InputAction action, string part, TextMeshProUGUI label) {
        action.Disable();

        int bindingIndex = GetCompositeBindingIndex(action, part);

        action.PerformInteractiveRebinding(bindingIndex)
            .OnComplete(op => {
                op.Dispose();
                action.Enable();
                SaveRebinds();
                RefreshAllUI();
            })
            .Start();
    }

    private int GetCompositeBindingIndex(InputAction action, string part) {
        for (int i = 0; i < action.bindings.Count; i++) {
            if (action.bindings[i].isPartOfComposite && action.bindings[i].name == part)
                return i;
        }
        return -1;
    }

    // ---------------- SAVE / LOAD ----------------

    private void SaveRebinds() {
        PlayerPrefs.SetString(RebindKey, inputActions.SaveBindingOverridesAsJson());
        PlayerPrefs.Save();
    }

    private void LoadRebinds() {
        if (PlayerPrefs.HasKey(RebindKey)) {
            inputActions.LoadBindingOverridesFromJson(PlayerPrefs.GetString(RebindKey));
        }
    }

    // ---------------- UI REFRESH ----------------

    private void RefreshAllUI() {
        RefreshMoveUI();
        RefreshActionUI();
    }

    private void RefreshMoveUI() {
        moveUpText.text = GetBinding(moveAction, "up");
        moveDownText.text = GetBinding(moveAction, "down");
        moveLeftText.text = GetBinding(moveAction, "left");
        moveRightText.text = GetBinding(moveAction, "right");
    }

    private void RefreshActionUI() {
        sprintText.text = GetBinding(sprintAction);
        interactText.text = GetBinding(interactAction);
        useText.text = GetBinding(useAction);
        throwText.text = GetBinding(throwAction);
        reloadText.text = GetBinding(reloadAction);
        mapText.text = GetBinding(mapAction);
    }


    private string GetBinding(InputAction action) {
        var b = action.bindings[0];
        return InputControlPath.ToHumanReadableString(
            b.effectivePath,
            InputControlPath.HumanReadableStringOptions.OmitDevice);
    }

    private string GetBinding(InputAction action, string part) {
        foreach (var b in action.bindings) {
            if (b.isPartOfComposite && b.name == part)
                return InputControlPath.ToHumanReadableString(
                    b.effectivePath,
                    InputControlPath.HumanReadableStringOptions.OmitDevice);
        }
        return "?";
    }

    // ---------------- RESET ----------------

    public void ResetRebinds() {
        inputActions.RemoveAllBindingOverrides();
        PlayerPrefs.DeleteKey(RebindKey);
        RefreshAllUI();
    }

    public static float MasterVolume =>
    PlayerPrefs.GetFloat("master_volume", 1f);

    public static float MusicVolume =>
        PlayerPrefs.GetFloat("music_volume", 1f);

    public static float SfxVolume =>
        PlayerPrefs.GetFloat("sfx_volume", 1f);
}