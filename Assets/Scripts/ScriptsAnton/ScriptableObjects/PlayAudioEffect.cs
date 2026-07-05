using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Scriptable Object to save the Audio that should be played on impact
/// </summary>
[CreateAssetMenu(fileName = "Play Audio Effect", menuName = "Impact System/Play Audio Effect")]
public class PlayAudioEffect : ScriptableObject {
    public AudioSource audioSourcePrefab;
    public List<AudioClip> audioClips = new List<AudioClip>();
    [Tooltip("Values are clamped to 0-1")]
    public Vector2 volumeRange = new Vector2(0, 1);
}
