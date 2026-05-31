using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SurfaceEffect", menuName = "Impact System/SurfaceEffect")]
public class SurfaceEffect : ScriptableObject {

    public List<SpawnObjectEffects> spawnObjectEffects = new List<SpawnObjectEffects>();
    public List<PlayAudioEffect> playAudioEffects = new List<PlayAudioEffect>();
}
