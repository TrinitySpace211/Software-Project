using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Controls the game's options menu, including audio volume sliders and
/// interactive key/button rebinding for the Player input action map.
/// Persists both volume levels and rebind overrides using PlayerPrefs.
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
    // Reference to the project's InputActionAsset, used to look up actions and to save/load rebinds.
    [SerializeField] private InputActionAsset inputActions;

    [Header("Move Rebind UI")]
    // Labels that display the currently bound key for each part of the "Move" composite action.
    [SerializeField] private TextMeshProUGUI moveUpText;
    [SerializeField] private TextMeshProUGUI moveDownText;
    [SerializeField] private TextMeshProUGUI moveLeftText;
    [SerializeField] private TextMeshProUGUI moveRightText;

    [Header("Other Rebind UI")]
    // Labels that display the currently bound key/button for simple (non-composite) actions.
    [SerializeField] private TextMeshProUGUI sprintText;
    [SerializeField] private TextMeshProUGUI interactText;
    [SerializeField] private TextMeshProUGUI throwText;
    [SerializeField] private TextMeshProUGUI reloadText;
    [SerializeField] private TextMeshProUGUI mapText;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Rebind Popup")]
    // Popup shown while waiting for the player to press a new key, and reused to show error messages.
    [SerializeField] private GameObject rebindPopup;
    [SerializeField] private TextMeshProUGUI rebindPopupText;

    // Cached input actions, resolved once in Awake() so we don't repeatedly search the asset at runtime.
    private InputAction moveAction;
    private InputAction sprintAction;
    private InputAction interactAction;
    private InputAction throwAction;
    private InputAction reloadAction;
    private InputAction mapAction;

    // PlayerPrefs keys used for persistence.
    private const string RebindKey = "rebinds";
    private const string MasterKey = "master_volume";
    private const string MusicKey = "music_volume";
    private const string SfxKey = "sfx_volume";

    // Tracks whether the most recent rebind attempt failed due to a conflicting binding.
    // Used by OnCancel to distinguish "player pressed Escape on purpose" from
    // "the rebind was force-cancelled internally after a conflicting press".
    private bool rebindWasCanceledByConflict;

    // Handle to the currently running "show error, then close popup" coroutine,
    // so a new error can cancel/replace one that's still in progress.
    private Coroutine errorRoutine;

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

    /// <summary>
    /// Called when the master volume slider changes. Persists the value, updates the
    /// percentage label, and re-applies volume to the audio listener/sources.
    /// </summary>
    public void SetMasterVolume(float value) {
        PlayerPrefs.SetFloat(MasterKey, value);
        PlayerPrefs.Save();

        if (masterVolumeValueText != null)
            masterVolumeValueText.text = Mathf.RoundToInt(value * 100f) + "%";

        ApplyVolumeSettings();
    }

    /// <summary>
    /// Called when the music volume slider changes. Persists the value, updates the
    /// percentage label, and re-applies volume to the music source.
    /// </summary>
    public void SetMusicVolume(float value) {
        PlayerPrefs.SetFloat(MusicKey, value);
        PlayerPrefs.Save();

        if (musicVolumeValueText != null)
            musicVolumeValueText.text = Mathf.RoundToInt(value * 100f) + "%";

        ApplyVolumeSettings();
    }

    /// <summary>
    /// Called when the SFX volume slider changes. Persists the value, updates the
    /// percentage label, and re-applies volume to the SFX source.
    /// </summary>
    public void SetSfxVolume(float value) {
        PlayerPrefs.SetFloat(SfxKey, value);
        PlayerPrefs.Save();

        if (sfxVolumeValueText != null)
            sfxVolumeValueText.text = Mathf.RoundToInt(value * 100f) + "%";

        ApplyVolumeSettings();
    }

    /// <summary>
    /// Loads saved volume values (or defaults of 1.0) from PlayerPrefs and applies them
    /// to the sliders and their labels without firing the sliders' onValueChanged events.
    /// </summary>
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

    /// <summary>
    /// Reads the currently saved volume values and applies them to the AudioListener
    /// (master) and to the music/SFX AudioSources (each scaled by master volume).
    /// </summary>
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

    /// <summary>
    /// Resolves and caches all relevant InputActions from the "Player" action map.
    /// Throws if the map or any expected action is missing (via the "true" lookup flag).
    /// </summary>
    private void CacheActions() {
        if (inputActions == null)
            return;

        var map = inputActions.FindActionMap("Player", true);

        moveAction = map.FindAction("Move", true);
        sprintAction = map.FindAction("Sprint", true);
        interactAction = map.FindAction("Interact", true);
        throwAction = map.FindAction("Throw", true);
        reloadAction = map.FindAction("Reloading", true);
        mapAction = map.FindAction("OpenMap", true);
    }

    // --- Public UI hook-up methods, intended to be wired to button OnClick() events. ---
    // Each rebinds one direction of the WASD/arrow "Move" composite action.
    public void RebindMoveUp() => RebindComposite(moveAction, "up", moveUpText, "Move up");
    public void RebindMoveDown() => RebindComposite(moveAction, "down", moveDownText, "Move down");
    public void RebindMoveLeft() => RebindComposite(moveAction, "left", moveLeftText, "Move left");
    public void RebindMoveRight() => RebindComposite(moveAction, "right", moveRightText, "Move right");

    // Each rebinds a single, non-composite action.
    public void RebindSprint() => RebindButton(sprintAction, sprintText, "Sprint");
    public void RebindInteract() => RebindButton(interactAction, interactText, "Interact");
    public void RebindThrow() => RebindButton(throwAction, throwText, "Throw");
    public void RebindReload() => RebindButton(reloadAction, reloadText, "Reload");
    public void RebindMap() => RebindButton(mapAction, mapText, "Map");

    /// <summary>
    /// Starts an interactive rebind for a simple (non-composite) action's single binding
    /// (binding index 0). Shows the rebind popup while waiting for input, rejects blocked
    /// controls and controls already used elsewhere, and reverts the change on conflict.
    /// </summary>
    /// <param name="action">The action whose binding should be rebound.</param>
    /// <param name="label">UI label to briefly show "..." in while waiting for input.</param>
    /// <param name="actionName">Human-readable name shown in the rebind popup.</param>
    private void RebindButton(InputAction action, TextMeshProUGUI label, string actionName) {
        if (action == null)
            return;

        rebindWasCanceledByConflict = false;
        action.Disable();
        ShowRebindPopup(actionName);

        if (label != null)
            label.text = "...";

        int bindingIndex = action.bindings.Count > 0 ? 0 : -1;
        if (bindingIndex < 0) {
            action.Enable();
            HideRebindPopup();
            return;
        }

        var rebind = action.PerformInteractiveRebinding(bindingIndex)
            .WithControlsExcluding("<Keyboard>/i")
            .WithControlsExcluding("<Keyboard>/e")
            .WithControlsExcluding("<Keyboard>/q")
            .WithControlsExcluding("<Mouse>/leftButton")
            .WithControlsExcluding("<Mouse>/rightButton")
            .WithCancelingThrough("<Keyboard>/escape")
            .OnPotentialMatch(op => {
                var control = op.selectedControl;

                if (control != null &&
                    (IsBlockedControl(control) || IsBindingAlreadyUsed(control, action, bindingIndex))) {
                    return;
                }

                op.Complete();
            })
            .OnComplete(op => {
                var control = op.selectedControl;

                bool conflict = control != null &&
                    (IsBlockedControl(control) || IsBindingAlreadyUsed(control, action, bindingIndex));

                op.Dispose();
                action.Enable();

                if (conflict) {
                    action.RemoveBindingOverride(bindingIndex);
                    rebindWasCanceledByConflict = true;
                    ShowErrorThenClose("Button already in use", 3f);
                    return;
                }

                HideRebindPopup();
                SaveRebinds();
                RefreshAllUI();
            })
            .OnCancel(op => {
                op.Dispose();
                action.Enable();
                HideRebindPopup();

                if (rebindWasCanceledByConflict)
                    ShowErrorThenClose("Button already in use", 3f);

                RefreshAllUI();
            });

        rebind.Start();
    }

    /// <summary>
    /// Starts an interactive rebind for one part ("up"/"down"/"left"/"right") of a composite
    /// action such as "Move". Behaves the same as <see cref="RebindButton"/> but targets the
    /// specific composite binding index for the given part instead of binding index 0.
    /// </summary>
    /// <param name="action">The composite action (e.g. Move) containing the part to rebind.</param>
    /// <param name="part">The composite part name to rebind (e.g. "up", "left").</param>
    /// <param name="label">UI label to briefly show "..." in while waiting for input.</param>
    /// <param name="actionName">Human-readable name shown in the rebind popup.</param>
    private void RebindComposite(InputAction action, string part, TextMeshProUGUI label, string actionName) {
        if (action == null)
            return;

        rebindWasCanceledByConflict = false;
        action.Disable();

        int bindingIndex = GetCompositeBindingIndex(action, part);
        if (bindingIndex < 0) {
            action.Enable();
            HideRebindPopup();
            return;
        }

        ShowRebindPopup(actionName);

        if (label != null)
            label.text = "...";

        var rebind = action.PerformInteractiveRebinding(bindingIndex)
            .WithControlsExcluding("<Keyboard>/i")
            .WithControlsExcluding("<Keyboard>/e")
            .WithControlsExcluding("<Keyboard>/q")
            .WithControlsExcluding("<Mouse>/leftButton")
            .WithControlsExcluding("<Mouse>/rightButton")
            .WithCancelingThrough("<Keyboard>/escape")
            .OnPotentialMatch(op => {
                var control = op.selectedControl;

                if (control != null &&
                    (IsBlockedControl(control) || IsBindingAlreadyUsed(control, action, bindingIndex))) {
                    return;
                }

                op.Complete();
            })
            .OnComplete(op => {
                var control = op.selectedControl;

                bool conflict = control != null &&
                    (IsBlockedControl(control) || IsBindingAlreadyUsed(control, action, bindingIndex));

                op.Dispose();
                action.Enable();

                if (conflict) {
                    action.RemoveBindingOverride(bindingIndex);
                    rebindWasCanceledByConflict = true;
                    ShowErrorThenClose("Button already in use", 3f);
                    return;
                }

                HideRebindPopup();
                SaveRebinds();
                RefreshAllUI();
            })
            .OnCancel(op => {
                op.Dispose();
                action.Enable();
                HideRebindPopup();

                if (rebindWasCanceledByConflict)
                    ShowErrorThenClose("Button already in use", 3f);

                RefreshAllUI();
            });

        rebind.Start();
    }

    /// <summary>
    /// Shows the rebind popup with a prompt telling the player which action they're
    /// about to rebind (e.g. "Press a button for "Sprint"").
    /// </summary>
    private void ShowRebindPopup(string actionName) {
        if (rebindPopup != null)
            rebindPopup.SetActive(true);

        if (rebindPopupText != null)
            rebindPopupText.text = $"Press a button for \"{actionName}\"";
    }

    /// <summary>
    /// Hides the rebind popup (used both after a successful rebind and after
    /// an error message has been shown for its duration).
    /// </summary>
    private void HideRebindPopup() {
        if (rebindPopup != null)
            rebindPopup.SetActive(false);
    }

    /// <summary>
    /// Checks whether the given control is already bound to any other binding in the
    /// entire input asset (across all action maps and actions), excluding the binding
    /// currently being edited. Composite "header" bindings are skipped since they have
    /// no real control path of their own. Uses <see cref="InputControlPath.Matches"/>
    /// rather than a raw string comparison so that name-based and position-based paths
    /// referring to the same physical control are correctly treated as equal.
    /// </summary>
    /// <param name="control">The control the player just pressed.</param>
    /// <param name="currentAction">The action currently being rebound (excluded from the check).</param>
    /// <param name="currentBindingIndex">The specific binding index being edited (excluded from the check).</param>
    /// <returns>True if the control is already used elsewhere; otherwise false.</returns>
    private bool IsBindingAlreadyUsed(InputControl control, InputAction currentAction, int currentBindingIndex = -1) {
        if (control == null || inputActions == null)
            return false;

        foreach (var actionMap in inputActions.actionMaps) {
            foreach (var action in actionMap.actions) {
                for (int i = 0; i < action.bindings.Count; i++) {
                    var binding = action.bindings[i];

                    if (binding.isComposite)
                        continue;

                    if (action == currentAction && i == currentBindingIndex)
                        continue;

                    if (string.IsNullOrEmpty(binding.effectivePath))
                        continue;

                    if (InputControlPath.Matches(binding.effectivePath, control))
                        return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Displays the rebind popup with an arbitrary error message instead of the
    /// normal "press a button" prompt.
    /// </summary>
    private void ShowRebindError(string message) {
        if (rebindPopup != null)
            rebindPopup.SetActive(true);

        if (rebindPopupText != null)
            rebindPopupText.text = message;
    }

    /// <summary>
    /// Finds the binding index within a composite action (e.g. Move) that corresponds
    /// to the given part name (e.g. "up").
    /// </summary>
    /// <returns>The binding index if found; otherwise -1.</returns>
    private int GetCompositeBindingIndex(InputAction action, string part) {
        for (int i = 0; i < action.bindings.Count; i++) {
            if (action.bindings[i].isPartOfComposite && action.bindings[i].name == part)
                return i;
        }

        return -1;
    }

    /// <summary>
    /// Serializes all current binding overrides to JSON and saves them to PlayerPrefs.
    /// </summary>
    private void SaveRebinds() {
        PlayerPrefs.SetString(RebindKey, inputActions.SaveBindingOverridesAsJson());
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Loads previously saved binding overrides from PlayerPrefs, if any exist,
    /// and applies them to the input actions asset.
    /// </summary>
    private void LoadRebinds() {
        if (PlayerPrefs.HasKey(RebindKey))
            inputActions.LoadBindingOverridesFromJson(PlayerPrefs.GetString(RebindKey));
    }

    /// <summary>
    /// Refreshes every rebind label in the menu and clears the current UI selection
    /// (so a lingering button highlight doesn't remain focused after a rebind).
    /// </summary>
    private void RefreshAllUI() {
        RefreshMoveUI();
        RefreshActionUI();
        ClearUISelection();
    }

    /// <summary>
    /// Clears the EventSystem's currently selected UI element, if any.
    /// </summary>
    private void ClearUISelection() {
        if (EventSystem.current != null) {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    /// <summary>
    /// Updates the four movement labels to reflect the current bindings of the
    /// "Move" composite action.
    /// </summary>
    private void RefreshMoveUI() {
        if (moveUpText != null) moveUpText.text = GetBinding(moveAction, "up");
        if (moveDownText != null) moveDownText.text = GetBinding(moveAction, "down");
        if (moveLeftText != null) moveLeftText.text = GetBinding(moveAction, "left");
        if (moveRightText != null) moveRightText.text = GetBinding(moveAction, "right");
    }

    /// <summary>
    /// Updates the labels for all simple (non-composite) rebindable actions.
    /// </summary>
    private void RefreshActionUI() {
        if (sprintText != null) sprintText.text = GetBinding(sprintAction);
        if (interactText != null) interactText.text = GetBinding(interactAction);
        if (throwText != null) throwText.text = GetBinding(throwAction);
        if (reloadText != null) reloadText.text = GetBinding(reloadAction);
        if (mapText != null) mapText.text = GetBinding(mapAction);
    }

    /// <summary>
    /// Returns a readable string (e.g. "Space", "LMB") for a simple action's
    /// first (and only expected) binding.
    /// </summary>
    private string GetBinding(InputAction action) {
        if (action == null || action.bindings.Count == 0)
            return "?";

        var b = action.bindings[0];
        return InputControlPath.ToHumanReadableString(
            b.effectivePath,
            InputControlPath.HumanReadableStringOptions.OmitDevice);
    }

    /// <summary>
    /// Returns a readable string for the binding of a specific part of a
    /// composite action (e.g. "up" -> "W").
    /// </summary>
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

    /// <summary>
    /// Removes all binding overrides across the entire input asset, clears the saved
    /// rebind data from PlayerPrefs, and refreshes the UI to show default bindings.
    /// Intended to be wired to a "Reset to Defaults" button.
    /// </summary>
    public void ResetRebinds() {
        inputActions.RemoveAllBindingOverrides();
        PlayerPrefs.DeleteKey(RebindKey);
        RefreshAllUI();
    }

    /// <summary>
    /// Checks whether a control is one of the reserved/fixed shortcuts that should never
    /// be assignable to a rebindable action (inventory, use, quit, escape/cancel, and
    /// mouse clicks used for UI interaction). Compares against the actual InputControl
    /// objects rather than path strings, since the mouse can report the same physical
    /// click through multiple path aliases (e.g. "<Mouse>/leftButton" vs the generic
    /// "<Mouse>/press" control), which a plain string comparison would miss.
    /// </summary>
    private bool IsBlockedControl(InputControl control) {
        if (control == null)
            return false;

        if (control.device is Keyboard) {
            return control.path == "<Keyboard>/i"
                || control.path == "<Keyboard>/e"
                || control.path == "<Keyboard>/q"
                || control.path == "<Keyboard>/escape";
        }

        if (control.device is Mouse mouse) {
            return control == mouse.leftButton
                || control == mouse.rightButton
                || control == mouse.press;
        }

        return false;
    }

    /// <summary>
    /// Shows an error message in the rebind popup, then automatically hides it and
    /// refreshes the UI after the given delay. Cancels any previously running error
    /// display so overlapping calls don't stack.
    /// </summary>
    /// <param name="message">The error text to display.</param>
    /// <param name="delay">How long (in seconds) to keep the message visible.</param>
    private void ShowErrorThenClose(string message, float delay = 3f) {
        if (errorRoutine != null)
            StopCoroutine(errorRoutine);

        errorRoutine = StartCoroutine(ErrorRoutine(message, delay));
    }

    /// <summary>
    /// Coroutine backing <see cref="ShowErrorThenClose"/>: displays the message,
    /// waits, then hides the popup and refreshes the rebind labels.
    /// </summary>
    private IEnumerator ErrorRoutine(string message, float delay) {
        ShowRebindError(message);

        yield return new WaitForSeconds(delay);

        HideRebindPopup();
        RefreshAllUI();

        errorRoutine = null;
    }

    /// <summary>
    /// Static accessors so other systems (e.g. an AudioManager) can read the
    /// current volume settings without needing a reference to this component.
    /// </summary>
    public static float MasterVolume => PlayerPrefs.GetFloat(MasterKey, 1f);
    public static float MusicVolume => PlayerPrefs.GetFloat(MusicKey, 1f);
    public static float SfxVolume => PlayerPrefs.GetFloat(SfxKey, 1f);
}