using UnityEngine;

public class CutoutObject : MonoBehaviour {
    [SerializeField] private Transform sphere;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask obstacleMask = ~0;

    private bool isVisible = true; // aktueller Zustand

    private void Update() {
        if (sphere == null || mainCamera == null) return;

        var origin = mainCamera.transform.position;
        var direction = (sphere.position - origin).normalized;
        var distanceToSphere = Vector3.Distance(origin, sphere.position);

        // Raycast against the obstacle layers only. If something is hit before reaching the sphere,
        // the view is blocked. Otherwise the sphere is visible.
        LeanTween.cancel(gameObject);
        if (Physics.Raycast(origin, direction, out RaycastHit hit, distanceToSphere, obstacleMask)) {
            // No obstacle hit: show/scale up the sphere
            LeanTween.scale(sphere.gameObject, Vector3.one * 6, 0.5f).setIgnoreTimeScale(true);
        } else {            // Hit an obstacle between camera and sphere: hide/scale down the sphere
            LeanTween.scale(sphere.gameObject, Vector3.zero, 0.5f).setIgnoreTimeScale(true);
        var blocked = Physics.Raycast(origin, direction, out var hit, distanceToSphere, obstacleMask);

        if (blocked && isVisible) {
            isVisible = false;
            LeanTween.cancel(sphere.gameObject);
            LeanTween.scale(sphere.gameObject, Vector3.one * 6, 0.5f);
        } else if (!blocked && !isVisible) {
            isVisible = true;
            LeanTween.cancel(sphere.gameObject);
            LeanTween.scale(sphere.gameObject, Vector3.zero, 0.5f);
        }
    }
}