using UnityEngine;

/// <summary>
/// The PoolableObject class to determine which GameObject should be pooled
/// </summary>
public class PoolableObject : MonoBehaviour {
    public ObjectPool parent;

    /// <summary>
    /// When the Poolable Object gets disabled it will go back to the pool
    /// </summary>
    public virtual void OnDisable() {
        if (parent != null) {
            parent.ReturnObjectToPool(this);
        }
    }
}
