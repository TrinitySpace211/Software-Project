using UnityEngine;

public class CutoutObject : MonoBehaviour {
    [SerializeField] private Transform sphere;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask obstacleMask = ~0;

    private void Update() {
        if (sphere == null || mainCamera == null) {
            return;
        }

        Vector3 origin = mainCamera.transform.position;
        Vector3 direction = (sphere.position - origin).normalized;
        float distanceToSphere = Vector3.Distance(origin, sphere.position);

        // Raycast against the obstacle layers only. If something is hit before reaching the sphere,
        // the view is blocked. Otherwise the sphere is visible.
        LeanTween.cancel(gameObject);
        if (Physics.Raycast(origin, direction, out RaycastHit hit, distanceToSphere, obstacleMask)) {
            // No obstacle hit: show/scale up the sphere
            LeanTween.scale(sphere.gameObject, Vector3.one * 6, 0.5f);
        } else {            // Hit an obstacle between camera and sphere: hide/scale down the sphere
            LeanTween.scale(sphere.gameObject, Vector3.zero, 0.5f);
        }
    }
}

