using UnityEngine;

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

    [Space]
    [Header("Melee Swing")]
    public AudioClip[] meleeSwing;
}
