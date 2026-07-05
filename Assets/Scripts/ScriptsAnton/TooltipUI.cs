using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// The Tooltip class that handles the salvage Button
/// </summary>
public class TooltipUI : MonoBehaviour, IPointerExitHandler {

    [SerializeField] private GameObject tooltipRoot;
    [SerializeField] private Button salvageButton;
    [SerializeField] private RectMask2D mask;
    [SerializeField] private GameObject extendedCard;
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI minAmountText;
    [SerializeField] private TextMeshProUGUI maxAmountText;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float salvageVolume;

    [Header("References")]
    [SerializeField] private Inventory inventory;

    private Slot slot;

    private Vector4 padding;
    private const float hiddenExtendedCardPadding = 150f;
    private const float shownExtendedCardPadding = 0f;

    /// <summary>
    /// Sets the Listeners for the Salvage Button and the Value Slider
    /// </summary>
    private void Start() {
        padding.y = hiddenExtendedCardPadding;
        padding = mask.padding;
        slider.wholeNumbers = true;

        Visibile(false);

        salvageButton.onClick.AddListener(HandleToolip);

        slider.onValueChanged.AddListener(HandleSliderValue);
    }

    /// <summary>
    /// Updates the Left Text to the slider value
    /// </summary>
    /// <param name="value">The Value (state) of the Slider</param>
    private void HandleSliderValue(float value) {
        minAmountText.text = ((int)value).ToString();
    }

    /// <summary>
    /// Toggles the window and checks if the item has multiple Items.
    /// If the item doesn't have multiple Items it will be destroyed otherwise
    /// a new window appears and allows the Player to set a amount to be destroyed
    /// </summary>
    public void HandleToolip() {
        if (padding.y == hiddenExtendedCardPadding) {
            if (slot != null && slot.GetAmount() == 1) {
                slot.SetItem(null);
                Visibile(false);
            } else {
                float maxAmount = slot.GetAmount();
                maxAmountText.text = maxAmount.ToString();
                slider.maxValue = maxAmount;
                slider.value = 0;

                padding.y = hiddenExtendedCardPadding;
                mask.padding = padding;

                LeanTween.value(extendedCard, hiddenExtendedCardPadding, shownExtendedCardPadding, 0.25f)
                    .setEaseOutCubic()
                    .setOnUpdate(value => {
                        padding.y = value;
                        mask.padding = padding;
                    });
            }
        } else {
            slot.RemoveAmount((int)slider.value);

            audioSource.volume = salvageVolume * SoundManager.Instance.volume;
            audioSource.Play();

            Visibile(false);
        }
    }

    /// <summary>
    /// Sets the Visibility of the tooltip
    /// </summary>
    /// <param name="state">true activates the Item, false deactivates it</param>
    public void Visibile(bool state) {
        tooltipRoot.SetActive(state);

        if (state) {
            transform.position = Mouse.current.position.ReadValue();
        }
    }

    /// <summary>
    /// Sets the slot it is hovering over globally
    /// </summary>
    /// <param name="slot"></param>
    public void SetSelectedSlot(Slot slot) {
        this.slot = slot;
    }

    /// <summary>
    /// Deactivates the Salvage Button if the Object exits the buttons Area
    /// </summary>
    /// <param name="eventData">The Data context</param>
    public void OnPointerExit(PointerEventData eventData) {
        padding.y = hiddenExtendedCardPadding;
        mask.padding = padding;
        Visibile(false);
    }

    /// <summary>
    /// Removes all Listeners just in case
    /// </summary>
    private void OnDestroy() {
        salvageButton.onClick.RemoveListener(HandleToolip);
        slider.onValueChanged.RemoveListener(HandleSliderValue);
    }
}
