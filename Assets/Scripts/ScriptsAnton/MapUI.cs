using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class MapUI : MonoBehaviour {

    [SerializeField] private GameObject map;
    [SerializeField] private AudioSource mapOpenAudioSource;
    [SerializeField] private AudioClipRefsSO audioClipRefsSO;

    public PauseMenu pauseMenu;

    private bool mapOpen = false;

    private void Start() {
        PlayerInputHandler.OnMapOpenAction += PlayerInputHandler_OnMapOpenAction;
        HideMap();
    }

    private void Update() {
        if (pauseMenu.IsPaused)
            return;
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
        mapOpen = true;
    }

    private void HideMap() {
        map.SetActive(false);
        mapOpen = false;
    }

    private void PlayMapSound() {
        mapOpenAudioSource.clip = audioClipRefsSO.mapOpen[UnityEngine.Random.Range(0, audioClipRefsSO.mapOpen.Length)];
        mapOpenAudioSource.volume = audioClipRefsSO.mapOpenVolume * SoundManager.Instance.volume;
        mapOpenAudioSource.Play();
    }

    private void PauseGame() {
        _ = !mapOpen ? Time.timeScale = 1f : Time.timeScale = 0f;
    }
}
