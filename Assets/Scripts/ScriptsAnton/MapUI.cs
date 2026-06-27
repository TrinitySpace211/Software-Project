using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class MapUI : MonoBehaviour {

    [SerializeField] private GameObject map;
    [SerializeField] private AudioSource mapOpenAudioSource;
    [SerializeField] private AudioClipRefsSO audioClipRefsSO;

    public PauseMenu pauseMenu;
    private void Start() {
        PlayerInputHandler.OnMapOpenAction += PlayerInputHandler_OnMapOpenAction;
        HideMap();
    }

    private void Update() {

        if (pauseMenu.IsPaused)
            return;

        if (Keyboard.current.zKey.wasPressedThisFrame) {
            ShowHideMap();
            PauseGame();
        }
    private void PlayerInputHandler_OnMapOpenAction() {
        ShowHideMap();
        PauseGame();
    }

    private void ShowHideMap() {
        bool isActive = map.activeInHierarchy;

        if (!isActive) {
            ShowMap();
            PlayMapSound();
        } else {
            HideMap();
            PlayMapSound();
        }
    }

    private void ShowMap() {
        map.SetActive(true);
    }

    private void HideMap() {
        map.SetActive(false);
    }

    private void PlayMapSound() {
        mapOpenAudioSource.clip = audioClipRefsSO.mapOpen[UnityEngine.Random.Range(0, audioClipRefsSO.mapOpen.Length)];
        mapOpenAudioSource.volume = audioClipRefsSO.mapOpenVolume * SoundManager.Instance.volume;
        mapOpenAudioSource.Play();
    }

    private void PauseGame() {
        _ = Time.timeScale == 0f ? Time.timeScale = 1f : Time.timeScale = 0f;
    }
}
