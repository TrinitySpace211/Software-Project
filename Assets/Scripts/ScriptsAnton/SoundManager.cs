using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// The Weapon Sound Manager has all the sound effects 
/// and music gathered here, so that SFX and Music Volume can be tweek easily
/// It uses the AudioClipRefs Scriptable Object, so that all Sounds are in one place and can be accessed easily
/// </summary>
public class SoundManager : MonoBehaviour {
    public static SoundManager Instance { get; private set; }

    [SerializeField] private AudioClipRefsSO audioClipRefsSO;

    [Header("Sound References")]
    [SerializeField] private AudioSource bulletImpactAudioSource;
    [SerializeField] private AudioSource footstepsAudioSource;
    [SerializeField] private AudioSource bulletShootAudioSource;

    private AudioSource audioSource;

    public float volume { get; private set; } = 1f;

    private readonly float shotgunReloadDelay = 0.25f;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable() {
        Player.OnHeal += Player_OnHeal;
        PlayerHealth.OnTakeDamage += Player_OnTakeDamage;
        Player.OnReload += Player_OnReload;
        PlayerHealth.OnDeath += Player_OnDeath;
        ZombieAI.OnTakeDamage += Enemy_OnTakeDamage;
        Player.OnGrenadeThrow += Player_OnGrenadeThrow;
        DayNightCycle.OnSunsetStarted += DayNightCycle_OnSunsetStarted;
        GasTankHealth.OnObjectiveDestroyed += GasTankHealth_OnObjectiveDestroyed;
    }

    private void OnDestroy() {
        Player.OnHeal -= Player_OnHeal;
        PlayerHealth.OnTakeDamage -= Player_OnTakeDamage;
        Player.OnReload -= Player_OnReload;
        PlayerHealth.OnDeath -= Player_OnDeath;
        ZombieAI.OnTakeDamage -= Enemy_OnTakeDamage;
        Player.OnGrenadeThrow -= Player_OnGrenadeThrow;
        DayNightCycle.OnSunsetStarted -= DayNightCycle_OnSunsetStarted;
    }

    /// <summary>
    /// Event that triggers every time if any Zombie is hurt
    /// </summary>
    /// <param name="position">Postion of the Zombie</param>
    private void Enemy_OnTakeDamage(Vector3 position) {
        PlaySound(audioClipRefsSO.zombieHurt, position, audioClipRefsSO.zombieHurtVolume);
    }

    /// <summary>
    /// Event that triggers every time the Player is hurt
    /// </summary>
    /// <param name="position">Position of the Player</param>
    private void Player_OnTakeDamage(Vector3 position) {
        PlaySound(audioClipRefsSO.playerHurt, position, audioClipRefsSO.playerHurtVolume);
    }

    /// <summary>
    /// Event that triggers once the Player is Dead
    /// </summary>
    /// <param name="position">The Position of the Player</param>
    private void Player_OnDeath(Vector3 position) {
        PlaySound(audioClipRefsSO.playerDeath, position, audioClipRefsSO.playerDeathVolume);
    }

    /// <summary>
    /// Event that triggers every time the Player is Reloading.
    /// Plays the Reload Sound Effect for the corresponding Weapon
    /// </summary>
    /// <param name="position">The Position of the Player</param>
    private void Player_OnReload(Vector3 position, GunSO gun) {
        switch (gun.type) {
            case GunType.AssaultRifle:
                AssaultRilfe_ReloadSound(position, audioClipRefsSO.reloadSoundVolume);
                break;
            case GunType.Pistol:
                Pistol_ReloadSound(position, audioClipRefsSO.reloadSoundVolume);
                break;
            case GunType.Shotgun:
                Shotgun_ReloadSound(position, audioClipRefsSO.reloadSoundVolume, gun);
                break;
            case GunType.Sniper:
                Sniper_ReloadSound(position, audioClipRefsSO.reloadSoundVolume);
                break;
        }
    }

    /// <summary>
    /// Plays the Pin Sound of the Grenade when it gets thrown
    /// </summary>
    /// <param name="position">The position where the Sound should happen</param>
    private void Player_OnGrenadeThrow(Vector3 position) {
        PlaySound(audioClipRefsSO.grenadePin, position, audioClipRefsSO.grenadePinVolume);
    }

    /// <summary>
    /// Plays the Explosion Sound for the Grenade at a specific Position
    /// </summary>
    /// <param name="position">The position where the Sound should happen</param>
    public void Grenade_ExplosionSound(Vector3 position) {
        PlaySound(audioClipRefsSO.explosionSounds, position, audioClipRefsSO.explosionSoundsVolume);
    }

    /// <summary>
    /// Plays the Heal Sound for the Health Kits at a specific Position
    /// </summary>
    /// <param name="position">The position where the Sound should happen</param>
    private void Player_OnHeal(Vector3 position) {
        PlaySound(audioClipRefsSO.playerHeal, position, audioClipRefsSO.playerHealVolume);
    }

    /// <summary>
    /// Plays the Reload Sound Effects for the Assault Rifle
    /// </summary>
    /// <param name="position">The position of the Impact</param>
    /// <param name="volume">Volume of the Sound Effects</param>s
    public void AssaultRilfe_ReloadSound(Vector3 position, float volume) {
        PlaySound(audioClipRefsSO.assaultRifleReloadSounds, position, volume);
    }

    /// <summary>
    /// Plays the Reload Sound Effects for the Pistol
    /// </summary>
    /// <param name="position">The position of the Impact</param>
    /// <param name="volume">Volume of the Sound Effects</param>
    public void Pistol_ReloadSound(Vector3 position, float volume) {
        PlaySound(audioClipRefsSO.pistolReloadSounds, position, volume);
    }

    /// <summary>
    /// Plays the Reload Sound Effects for the Shotgun
    /// </summary>
    /// <param name="position">The position of the Impact</param>
    /// <param name="volume">Volume of the Sound Effects</param>
    public void Shotgun_ReloadSound(Vector3 position, float volume, GunSO gun) {
        if (gun.type == GunType.Shotgun) {
            StartCoroutine(ReloadMultipleTimes(position, volume, gun));
        } else {
            PlaySound(audioClipRefsSO.shotgunReloadSounds, position, volume);
        }
    }

    /// <summary>
    /// The Reload Sound will be played multiple times by the amount of bullets a gun has.
    /// This is just for the Shotgun, because it reloads every bullet one at a time;
    /// </summary>
    private IEnumerator ReloadMultipleTimes(Vector3 position, float volume, GunSO gun) {
        float missingAmmo = gun.shootConfigSO.maxAmmo - gun.currentAmmo;
        for (int i = 0; i < missingAmmo; i++) {
            PlaySound(audioClipRefsSO.shotgunReloadSounds, position, volume);

            yield return new WaitForSeconds(shotgunReloadDelay);
        }
    }

    /// <summary>
    /// Plays the Reload Sound Effects for the Sniper
    /// </summary>
    /// <param name="position">The position of the Impact</param>
    /// <param name="volume">Volume of the Sound Effects</param>
    public void Sniper_ReloadSound(Vector3 position, float volume) {
        PlaySound(audioClipRefsSO.sniperReloadSounds, position, volume);
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
    /// Plays the Sunset Sound Effect
    /// </summary>
    public void DayNightCycle_OnSunsetStarted() {
        audioSource.clip = audioClipRefsSO.sunsetSound;
        audioSource.volume = audioClipRefsSO.sunsetSoundVolume;
        audioSource.Play();
    }

    /// <summary>
    /// Plays an Explosion Sound Effect if the Objective is destroyed
    /// </summary>
    private void GasTankHealth_OnObjectiveDestroyed(Vector3 position) {
        PlaySound(audioClipRefsSO.objectiveExplosion, position, audioClipRefsSO.objectiveExplosionVolume);
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
