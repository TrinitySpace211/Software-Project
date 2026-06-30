using UnityEngine;

[System.Serializable]
public struct SaveableRef {
    [SerializeField] private MonoBehaviour target;

    public ISaveable Interface => target as ISaveable;

    public void OnValidate() {
        if (target != null && !(target is ISaveable)) {
            Debug.LogError($"[SaveSystem] {target.name} ({target.GetType().Name}) implementiert nicht das ISaveable-Interface!", target);
            target = null;
        }
    }
}
