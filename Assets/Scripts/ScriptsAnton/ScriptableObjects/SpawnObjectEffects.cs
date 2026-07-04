using UnityEngine;

[CreateAssetMenu(fileName = "Spawn Object Effects", menuName = "Impact System/Spawn Object Effects")]
public class SpawnObjectEffects : ScriptableObject {
    public GameObject prefab;
    public float probability = 1;
    public bool randomizeRotation;
    [Tooltip("Zero values will lock the rotation on that axis. Value up to 360 are sensible for each X,Y,Z")]
    public Vector3 randomizedRotationMultiplier = Vector3.zero;
}
