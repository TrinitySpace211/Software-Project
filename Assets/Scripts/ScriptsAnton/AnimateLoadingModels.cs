using UnityEngine;

public class AnimateLoadingModels : MonoBehaviour {

    [SerializeField] private GameObject item;

    private float rangeMultiplier = 0.1f;
    private float animationSpeed = 3f;
    private float animationTimer = 0f;
    private float rotationSpeed = 20f;

    /* private void Update() {
        AnimateItem();
        RotateItem();
    } */

    private void AnimateItem() {
        animationTimer += Time.deltaTime;

        float position = 1f + rangeMultiplier * Mathf.Sin(animationTimer * animationSpeed);
        item.transform.localPosition = Vector3.up * position;
    }

    private void RotateItem() {
        item.transform.RotateAround(transform.position, Vector3.up, Time.deltaTime * rotationSpeed);
    }
}
