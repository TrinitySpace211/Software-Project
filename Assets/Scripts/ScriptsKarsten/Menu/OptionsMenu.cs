using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

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
    [SerializeField] private TextMeshProUGUI throwText;
    [SerializeField] private TextMeshProUGUI reloadText;
    [SerializeField] private TextMeshProUGUI mapText;
    [SerializeField] private TextMeshProUGUI inventoryText;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    private InputAction moveAction;
    private InputAction sprintAction;
    private InputAction interactAction;
    private InputAction throwAction;
    private InputAction reloadAction;
    private InputAction mapAction;
    private InputAction inventoryAction;

    private const string RebindKey = "rebinds";
    private const string MasterKey = "master_volume";
    private const string MusicKey = "music_volume";
    private const string SfxKey = "sfx_volume";

    private void Awake() {
        CacheActions();
    }

    private void Start() {
        LoadVolumes();
        LoadRebinds();
        RefreshAllUI();
        ApplyVolumeSettings();
    }

    private void OnEnable() {
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);

        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(SetSfxVolume);
    }

    private void OnDisable() {
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.RemoveListener(SetMasterVolume);

        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.RemoveListener(SetMusicVolume);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.RemoveListener(SetSfxVolume);
    }

    public void SetMasterVolume(float value) {
        PlayerPrefs.SetFloat(MasterKey, value);
        PlayerPrefs.Save();

        if (masterVolumeValueText != null)
            masterVolumeValueText.text = Mathf.RoundToInt(value * 100f) + "%";

        ApplyVolumeSettings();
    }

    public void SetMusicVolume(float value) {
        PlayerPrefs.SetFloat(MusicKey, value);
        PlayerPrefs.Save();

        if (musicVolumeValueText != null)
            musicVolumeValueText.text = Mathf.RoundToInt(value * 100f) + "%";

        ApplyVolumeSettings();
    }

    public void SetSfxVolume(float value) {
        PlayerPrefs.SetFloat(SfxKey, value);
        PlayerPrefs.Save();

        if (sfxVolumeValueText != null)
            sfxVolumeValueText.text = Mathf.RoundToInt(value * 100f) + "%";

        ApplyVolumeSettings();
    }

    private void LoadVolumes() {
        float master = PlayerPrefs.GetFloat(MasterKey, 1f);
        float music = PlayerPrefs.GetFloat(MusicKey, 1f);
        float sfx = PlayerPrefs.GetFloat(SfxKey, 1f);

        if (masterVolumeSlider != null)
            masterVolumeSlider.SetValueWithoutNotify(master);

        if (musicVolumeSlider != null)
            musicVolumeSlider.SetValueWithoutNotify(music);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.SetValueWithoutNotify(sfx);

        if (masterVolumeValueText != null)
            masterVolumeValueText.text = Mathf.RoundToInt(master * 100f) + "%";

        if (musicVolumeValueText != null)
            musicVolumeValueText.text = Mathf.RoundToInt(music * 100f) + "%";

        if (sfxVolumeValueText != null)
            sfxVolumeValueText.text = Mathf.RoundToInt(sfx * 100f) + "%";
    }

    public void ApplyVolumeSettings() {
        float master = PlayerPrefs.GetFloat(MasterKey, 1f);
        float music = PlayerPrefs.GetFloat(MusicKey, 1f);
        float sfx = PlayerPrefs.GetFloat(SfxKey, 1f);

        AudioListener.volume = master;

        if (musicSource != null)
            musicSource.volume = music * master;

        if (sfxSource != null)
            sfxSource.volume = sfx * master;
    }

    private void CacheActions() {
        var map = inputActions.FindActionMap("Player", true);

        moveAction = map.FindAction("Move", true);
        sprintAction = map.FindAction("Sprint", true);
        interactAction = map.FindAction("Interact", true);
        throwAction = map.FindAction("Throw", true);
        reloadAction = map.FindAction("Reloading", true);
        mapAction = map.FindAction("OpenMap", true);
        inventoryAction = map.FindAction("OpenInventory", true);
    }

    public void RebindMoveUp() => RebindComposite(moveAction, "up", moveUpText);
    public void RebindMoveDown() => RebindComposite(moveAction, "down", moveDownText);
    public void RebindMoveLeft() => RebindComposite(moveAction, "left", moveLeftText);
    public void RebindMoveRight() => RebindComposite(moveAction, "right", moveRightText);

    public void RebindSprint() => RebindButton(sprintAction, sprintText);
    public void RebindInteract() => RebindButton(interactAction, interactText);
    public void RebindThrow() => RebindButton(throwAction, throwText);
    public void RebindReload() => RebindButton(reloadAction, reloadText);
    public void RebindMap() => RebindButton(mapAction, mapText);
    public void RebindInventory() => RebindButton(inventoryAction, inventoryText);

    private void RebindButton(InputAction action, TextMeshProUGUI label) {
        if (action == null)
            return;

        action.Disable();

        if (label != null)
            label.text = "...";

        action.PerformInteractiveRebinding()
            .WithControlsExcluding("Mouse")
            .WithCancelingThrough("<Keyboard>/escape")
            .OnComplete(op => {
                op.Dispose();
                action.Enable();
                SaveRebinds();
                RefreshAllUI();
            })
            .OnCancel(op => {
                op.Dispose();
                action.Enable();
                RefreshAllUI();
            })
            .Start();
    }

    private void RebindComposite(InputAction action, string part, TextMeshProUGUI label) {
        if (action == null)
            return;

        action.Disable();

        int bindingIndex = GetCompositeBindingIndex(action, part);
        if (bindingIndex < 0) {
            action.Enable();
            return;
        }

        if (label != null)
            label.text = "...";

        action.PerformInteractiveRebinding(bindingIndex)
            .WithCancelingThrough("<Keyboard>/escape")
            .OnComplete(op => {
                op.Dispose();
                action.Enable();
                SaveRebinds();
                RefreshAllUI();
            })
            .OnCancel(op => {
                op.Dispose();
                action.Enable();
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

    private void SaveRebinds() {
        PlayerPrefs.SetString(RebindKey, inputActions.SaveBindingOverridesAsJson());
        PlayerPrefs.Save();
    }

    private void LoadRebinds() {
        if (PlayerPrefs.HasKey(RebindKey))
            inputActions.LoadBindingOverridesFromJson(PlayerPrefs.GetString(RebindKey));
    }

    private void RefreshAllUI() {
        RefreshMoveUI();
        RefreshActionUI();
    }

    private void RefreshMoveUI() {
        if (moveUpText != null) moveUpText.text = GetBinding(moveAction, "up");
        if (moveDownText != null) moveDownText.text = GetBinding(moveAction, "down");
        if (moveLeftText != null) moveLeftText.text = GetBinding(moveAction, "left");
        if (moveRightText != null) moveRightText.text = GetBinding(moveAction, "right");
    }

    private void RefreshActionUI() {
        if (sprintText != null) sprintText.text = GetBinding(sprintAction);
        if (interactText != null) interactText.text = GetBinding(interactAction);
        if (throwText != null) throwText.text = GetBinding(throwAction);
        if (reloadText != null) reloadText.text = GetBinding(reloadAction);
        if (mapText != null) mapText.text = GetBinding(mapAction);
        if (inventoryText != null) inventoryText.text = GetBinding(inventoryAction);
    }

    private string GetBinding(InputAction action) {
        if (action == null || action.bindings.Count == 0)
            return "?";

        var b = action.bindings[0];
        return InputControlPath.ToHumanReadableString(
            b.effectivePath,
            InputControlPath.HumanReadableStringOptions.OmitDevice);
    }

    private string GetBinding(InputAction action, string part) {
        if (action == null)
            return "?";

        foreach (var b in action.bindings) {
            if (b.isPartOfComposite && b.name == part) {
                return InputControlPath.ToHumanReadableString(
                    b.effectivePath,
                    InputControlPath.HumanReadableStringOptions.OmitDevice);
            }
        }

        return "?";
    }

    public void ResetRebinds() {
        inputActions.RemoveAllBindingOverrides();
        PlayerPrefs.DeleteKey(RebindKey);
        RefreshAllUI();
    }

    public static float MasterVolume => PlayerPrefs.GetFloat(MasterKey, 1f);
    public static float MusicVolume => PlayerPrefs.GetFloat(MusicKey, 1f);
    public static float SfxVolume => PlayerPrefs.GetFloat(SfxKey, 1f);
}