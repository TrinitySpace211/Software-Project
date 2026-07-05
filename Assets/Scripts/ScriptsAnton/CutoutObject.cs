using UnityEngine;

/// <summary>
/// Activates or disables the Sphere which cuts out the Player in the Camera
/// </summary>
public class CutoutObject : MonoBehaviour {
    [SerializeField] private Transform sphere;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask obstacleMask = ~0;

    /// <summary>
    /// If the player is staying behind a Object, which is on a certain Layer, the Raycast can't hit the player
    /// then the Cutout will be scaled up, otherwise if it can hit the player then it will scale down
    /// </summary>
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
        }
    }
}