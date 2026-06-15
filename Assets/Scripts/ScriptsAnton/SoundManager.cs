using System;
using UnityEngine;

/// <summary>
/// The Weapon Sound Manager has all the sound effects 
/// and music gathered here, so that SFX and Music Volume can be tweek easily
/// It uses the AudioClipRefs Scriptable Object, so that all Sounds are in one place and can be accessed easily
/// </summary>
public class SoundManager : MonoBehaviour {
    public static SoundManager Instance { get; private set; }

    private const string PLAYER_PREFS_SOUND_EFFECTS_VOLUME = "SoundEffectsVolume";

    [SerializeField] private AudioClipRefsSO audioClipRefsSO;

    [Header("Sound References")]
    [SerializeField] private AudioSource backgroundMusic;
    [SerializeField] private AudioSource bulletImpactAudioSource;
    [SerializeField] private AudioSource footstepsAudioSource;

    private float volume = 1f;

    private void Awake() {
        Instance = this;

        volume = PlayerPrefs.GetFloat(PLAYER_PREFS_SOUND_EFFECTS_VOLUME, 1f);
    }

    private void Start() {
        PlayerHealth.OnTakeDamage += Player_OnTakeDamage;
        PlayerHealth.OnDeath += Player_OnDeath;
        ZombieAI.OnTakeDamage += Enemy_OnTakeDamage;
    }

    /// <summary>
    /// Event that triggers every time if any Zombie is hurt
    /// </summary>
    /// <param name="position">Postion of the Zombie</param>
    private void Enemy_OnTakeDamage(Vector3 position) {
        Zombie_Hurt(position, audioClipRefsSO.playerHurtVolume);
    }

    /// <summary>
    /// Event that triggers every time the Player is hurt
    /// </summary>
    /// <param name="position">Position of the Player</param>
    private void Player_OnTakeDamage(Vector3 position) {
        Player_Hurt(position, audioClipRefsSO.playerHurtVolume);
    }

    private void Player_OnDeath(Vector3 position) {
        Player_Death(position, audioClipRefsSO.playerDeathVolume);
    }

    /// <summary>
    /// Plays the Shoot Sound Effects for the Assault Rifle
    /// </summary>
    /// <param name="position">The position of the Impact</param>
    /// <param name="volume">Volume of the Sound Effects</param>
    public void AssaultRilfe_ShootSound(Vector3 position, float volume) {
        PlaySound(audioClipRefsSO.assaultRifleSounds, position, volume);
    }

    /// <summary>
    /// Plays the Shoot Sound Effects for the Pistol
    /// </summary>
    /// <param name="position">The position of the Impact</param>
    /// <param name="volume">Volume of the Sound Effects</param>
    public void Pistol_ShootSound(Vector3 position, float volume) {
        PlaySound(audioClipRefsSO.pistolSounds, position, volume);
    }

    /// <summary>
    /// Plays the Shoot Sound Effects for the Shotgun
    /// </summary>
    /// <param name="position">The position of the Impact</param>
    /// <param name="volume">Volume of the Sound Effects</param>
    public void Shotgun_ShootSound(Vector3 position, float volume) {
        PlaySound(audioClipRefsSO.shotgunSounds, position, volume);
    }

    /// <summary>
    /// Plays the Shoot Sound Effects for the Sniper
    /// </summary>
    /// <param name="position">The position of the Impact</param>
    /// <param name="volume">Volume of the Sound Effects</param>
    public void Sniper_ShootSound(Vector3 position, float volume) {
        PlaySound(audioClipRefsSO.sniperSounds, position, volume);
    }

    /// <summary>
    /// Plays the Reload Sound Effects for the Assault Rifle
    /// </summary>
    /// <param name="position">The position of the Impact</param>
    /// <param name="volume">Volume of the Sound Effects</param>s
    public void AssaultRilfe_Reload(Vector3 position, float volume) {
        PlaySound(audioClipRefsSO.assaultRifleSounds, position, volume);
    }

    /// <summary>
    /// Plays the Reload Sound Effects for the Pistol
    /// </summary>
    /// <param name="position">The position of the Impact</param>
    /// <param name="volume">Volume of the Sound Effects</param>
    public void Pistol_Reload(Vector3 position, float volume) {
        PlaySound(audioClipRefsSO.pistolSounds, position, volume);
    }

    /// <summary>
    /// Plays the Reload Sound Effects for the Shotgun
    /// </summary>
    /// <param name="position">The position of the Impact</param>
    /// <param name="volume">Volume of the Sound Effects</param>
    public void Shotgun_Reload(Vector3 position, float volume) {
        PlaySound(audioClipRefsSO.shotgunSounds, position, volume);
    }

    /// <summary>
    /// Plays the Reload Sound Effects for the Sniper
    /// </summary>
    /// <param name="position">The position of the Impact</param>
    /// <param name="volume">Volume of the Sound Effects</param>
    public void Sniper_Reload(Vector3 position, float volume) {
        PlaySound(audioClipRefsSO.sniperSounds, position, volume);
    }

    /// <summary>
    /// Plays the Melee Swing Sound Effects for all Melees
    /// </summary>
    /// <param name="position">The position of the Impact</param>
    /// <param name="volume">Volume of the Sound Effects</param>
    public void Melee_Swing(Vector3 position, float volume) {
        PlaySound(audioClipRefsSO.meleeSwing, position, volume);
    }

    /// <summary>
    /// Plays this Sound Effect if the hp of a Zombie gets reduced
    /// </summary>
    /// <param name="position">The position of the Impact</param>
    /// <param name="volume">Volume of the Sound Effects</param>
    public void Zombie_Hurt(Vector3 position, float volume) {
        PlaySound(audioClipRefsSO.zombieHurt, position, volume);
    }

    /// <summary>
    /// Plays this Sound Effect if the hp of a Player gets reduced
    /// </summary>
    /// <param name="position">The position of the Impact</param>
    /// <param name="volume">Volume of the Sound Effects</param>
    public void Player_Hurt(Vector3 position, float volume) {
        PlaySound(audioClipRefsSO.playerHurt, position, volume);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="position"></param>
    /// <param name="volume"></param>
    public void Player_Death(Vector3 position, float volume) {
        PlaySound(audioClipRefsSO.playerDeath, position, volume);
    }

    /// <summary>
    /// Plays all the Sounds that it is given
    /// </summary>
    /// <param name="audioClips">The Sound Effects it should play</param>
    /// <param name="position">The Position it should play at</param>
    /// <param name="volume">The Volume of the Sound Effects</param>
    private void PlaySound(AudioClip[] audioClips, Vector3 position, float volume = 1f) {
        PlaySound(audioClips[UnityEngine.Random.Range(0, audioClips.Length)], position, volume);
    }

    /// <summary>
    /// Plays the Sound Effect at a given position if the global volume is changed, 
    /// then all Sound Effects will change accordingly
    /// </summary>
    /// <param name="audioClip">The Sound Effect it should play</param>
    /// <param name="position">The Position it should play at</param>
    /// <param name="volumeMultiplier">The Volume of the Sound Effect</param>
    private void PlaySound(AudioClip audioClip, Vector3 position, float volumeMultiplier) {
        AudioSource.PlayClipAtPoint(audioClip, position, volumeMultiplier * volume);
    }
}
