using UnityEngine;

public abstract class Pickup : MonoBehaviour, IVisitor {
    protected abstract void ApplyPickupEffect(Player player);

    public void Visit<T>(T visitable) where T : Component, IVisitable {
        if (visitable is Player player) {
            ApplyPickupEffect(player);
        }
    }

    public void OnTriggerEnter(Collider other) {
        other.GetComponent<IVisitable>()?.Accept(this);
        Destroy(gameObject);
    }
}
