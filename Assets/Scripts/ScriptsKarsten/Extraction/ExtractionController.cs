using UnityEngine;
using System.Collections;
using UnityEngine.Animations.Rigging;
using TMPro;

public class ExtractionController : MonoBehaviour {
    [Header("Player")]
    public GameObject playerObject;
    public Animator playerAnimator;
    public Transform playerMoveTarget;
    public float playerMoveSpeed = 1.5f;

    [Header("Rig")]
    public Rig aimLayer;

    [Header("Fade")]
    public CanvasGroup fadeGroup;
    public float fadeDuration = 1f;

    [Header("End Text")]
    public TMP_Text endText;
    public string endMessage = "The 10 dayz are over, but the nightmare isn’t.";
    public float endTextVisibleTime = 2f;

    private Player playerScript;
    private PlayerInputHandler playerInputHandler;
    private PlayerAnimation playerAnimation;
    private CharacterController characterController;
    private bool isRunning;
    private float originalAimWeight = 1f;

    private static readonly int inputXHash = Animator.StringToHash("inputX");
    private static readonly int inputYHash = Animator.StringToHash("inputY");
    private static readonly int inputMagnitudeHash = Animator.StringToHash("inputMagnitude");

    private void Awake() {
        if (playerObject != null) {
            playerScript = playerObject.GetComponent<Player>();
            playerInputHandler = playerObject.GetComponent<PlayerInputHandler>();
            playerAnimation = playerObject.GetComponent<PlayerAnimation>();
            characterController = playerObject.GetComponent<CharacterController>();
        }

        if (playerScript != null)
            originalAimWeight = playerScript.GetAimLayerWeight();

        if (fadeGroup != null)
            fadeGroup.alpha = 0f;

        if (endText != null) {
            endText.gameObject.SetActive(false);
            endText.text = endMessage;
        }
    }

    public void StartExtraction() {
        if (isRunning)
            return;

        StartCoroutine(ExtractionRoutine());
    }

    private IEnumerator ExtractionRoutine() {
        isRunning = true;

        if (aimLayer != null)
            aimLayer.weight = 0f;

        if (playerInputHandler != null)
            playerInputHandler.enabled = false;

        if (playerAnimation != null)
            playerAnimation.enabled = false;

        if (playerScript != null)
            playerScript.enabled = false;

        if (characterController != null)
            characterController.enabled = false;

        if (playerAnimator != null) {
            playerAnimator.SetFloat(inputXHash, 0f);
            playerAnimator.SetFloat(inputYHash, 1f);
            playerAnimator.SetFloat(inputMagnitudeHash, 1f);
        }

        yield return MovePlayerToTarget();

        if (playerAnimator != null) {
            playerAnimator.SetFloat(inputMagnitudeHash, 0f);
            playerAnimator.SetFloat(inputXHash, 0f);
            playerAnimator.SetFloat(inputYHash, 0f);
        }

        yield return FadeCanvasGroup(fadeGroup, 0f, 1f);

        if (endText != null) {
            endText.gameObject.SetActive(true);
            endText.text = endMessage;
        }

        yield return new WaitForSeconds(endTextVisibleTime);
    }

    private IEnumerator MovePlayerToTarget() {
        if (playerObject == null || playerMoveTarget == null)
            yield break;

        while (Vector3.Distance(playerObject.transform.position, playerMoveTarget.position) > 0.05f) {
            playerObject.transform.position = Vector3.MoveTowards(
                playerObject.transform.position,
                playerMoveTarget.position,
                playerMoveSpeed * Time.deltaTime
            );

            yield return null;
        }

        playerObject.transform.position = playerMoveTarget.position;
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup group, float from, float to) {
        if (group == null)
            yield break;

        float t = 0f;
        group.alpha = from;

        while (t < fadeDuration) {
            t += Time.deltaTime;
            group.alpha = Mathf.Lerp(from, to, t / fadeDuration);
            yield return null;
        }

        group.alpha = to;
    }
}