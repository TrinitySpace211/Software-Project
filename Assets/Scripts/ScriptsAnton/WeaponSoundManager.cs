using UnityEngine;

public class WeaponSoundManager : MonoBehaviour {
    public static WeaponSoundManager Instance { get; private set; }

    private const string PLAYER_PREFS_SOUND_EFFECTS_VOLUME = "SoundEffectsVolume";

    [SerializeField] private AudioClipRefsSO audioClipRefsSO;

    private float volume = 1f;

    private void Awake() {
        Instance = this;

        volume = PlayerPrefs.GetFloat(PLAYER_PREFS_SOUND_EFFECTS_VOLUME, 1f);
    }

    public void AssaultRilfe_ShootSound(Vector3 position, float volume) {
        PlaySound(audioClipRefsSO.assaultRifleSounds, position, volume);
    }

    public void Pistol_ShootSound(Vector3 position, float volume) {
        PlaySound(audioClipRefsSO.pistolSounds, position, volume);
    }

    public void Shotgun_ShootSound(Vector3 position, float volume) {
        PlaySound(audioClipRefsSO.shotgunSounds, position, volume);
    }

    public void Sniper_ShootSound(Vector3 position, float volume) {
        PlaySound(audioClipRefsSO.sniperSounds, position, volume);
    }

    public void AssaultRilfe_Reload(Vector3 position, float volume) {
        PlaySound(audioClipRefsSO.assaultRifleSounds, position, volume);
    }

    public void Pistol_Reload(Vector3 position, float volume) {
        PlaySound(audioClipRefsSO.pistolSounds, position, volume);
    }

    public void Shotgun_Reload(Vector3 position, float volume) {
        PlaySound(audioClipRefsSO.shotgunSounds, position, volume);
    }

    public void Sniper_Reload(Vector3 position, float volume) {
        PlaySound(audioClipRefsSO.sniperSounds, position, volume);
    }

    public void Melee_Swing(Vector3 position, float volume) {
        PlaySound(audioClipRefsSO.meleeSwing, position, volume);
    }

    private void PlaySound(AudioClip[] audioClips, Vector3 position, float volume = 1f) {
        PlaySound(audioClips[UnityEngine.Random.Range(0, audioClips.Length)], position, volume);
    }

    private void PlaySound(AudioClip audioClip, Vector3 position, float volumeMultiplier) {
        AudioSource.PlayClipAtPoint(audioClip, position, volumeMultiplier * volume);
    }
}
