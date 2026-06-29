using UnityEngine;

/// <summary>
/// Scriptable Object to to keep most of the Sound Effects in one place
/// </summary>
[CreateAssetMenu(fileName = "AudioClipRefsSO", menuName = "Sounds/AudioClipRefsSO")]
public class AudioClipRefsSO : ScriptableObject {

    [Header("Shoot Sounds")]
    public AudioClip[] assaultRifleSounds;
    public AudioClip[] pistolSounds;
    public AudioClip[] shotgunSounds;
    public AudioClip[] sniperSounds;

    [Space]
    [Header("Reload Sounds")]
    public AudioClip[] assaultRifleReloadSounds;
    public AudioClip[] pistolReloadSounds;
    public AudioClip[] shotgunReloadSounds;
    public AudioClip[] sniperReloadSounds;
    public float reloadSoundVolume;

    [Space]
    [Header("Melee Swing")]
    public AudioClip[] meleeSwing;

    [Space]
    [Header("Grenade Sounds")]
    public AudioClip[] grenadePin;
    public float grenadePinVolume;
    public AudioClip[] explosionSounds;
    public float explosionSoundsVolume;

    [Space]
    [Header("Zombie Hurt")]
    public AudioClip[] zombieHurt;
    public float zombieHurtVolume;

    [Space]
    [Header("Player Hurt")]
    public AudioClip[] playerHurt;
    public float playerHurtVolume;

    [Space]
    [Header("Player Death")]
    public AudioClip playerDeath;
    public float playerDeathVolume;

    [Space]
    [Header("Player Heal")]
    public AudioClip playerHeal;
    public float playerHealVolume;

    [Space]
    [Header("Map")]
    public AudioClip[] mapOpen;
    public float mapOpenVolume;

    [Space]
    [Header("Sunset")]
    public AudioClip sunsetSound;
    public float sunsetSoundVolume;

    [Space]
    [Header("Objective Destroyed")]
    public AudioClip objectiveExplosion;
    public float objectiveExplosionVolume;
}
