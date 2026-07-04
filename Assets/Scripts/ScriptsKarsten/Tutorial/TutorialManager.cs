using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Animations.Rigging;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls the tutorial flow: player movement, messages, and scene transition.
/// Supports skipping and fade-out to next scene.
/// </summary>
public class TutorialManager : MonoBehaviour {
    public static readonly string ID = "TutorialManager";

    [Header("Player")]
    public GameObject playerObject;
    public Animator playerAnimator;
    public Transform point1;
    public Transform point2;
    public Transform point3;
    public Transform point4;
    public float playerMoveSpeed = 1.5f;

    [Header("Rotation")]
    public float rotationDuration = 0.5f;
    public float pauseAfterRotation = 0.2f;

    [Header("Rig")]
    public Rig aimLayer;

    [Header("Text")]
    public TextMeshProUGUI tutorialText;
    public string[] tutorialMessages;
    public float messageDelay = 5f;

    [Header("Scene Transition")]
    public Image fadeImage;
    public float fadeDuration = 2f;
    public Loader.Scene nextScene = Loader.Scene.MainScene;

    [Header("UI")]
    public Button skipButton;

    private bool isSkipping;
    private bool isRunning;

    private bool tutorialFinished = false;

    private Player playerScript;
    private PlayerInputHandler playerInputHandler;
    private PlayerAnimation playerAnimation;
    private PlayerInput playerInput;
    private CharacterController characterController;
    private Rigidbody playerRigidbody;

    private static readonly int inputXHash = Animator.StringToHash("inputX");
    private static readonly int inputYHash = Animator.StringToHash("inputY");
    private static readonly int inputMagnitudeHash = Animator.StringToHash("inputMagnitude");

    private Coroutine messageCoroutine;

    /// <summary>
    /// Initializes references and UI state.
    /// </summary>
    private void Awake() {
        if (playerObject != null) {
            playerScript = playerObject.GetComponent<Player>();
            playerInputHandler = playerObject.GetComponent<PlayerInputHandler>();
            playerAnimation = playerObject.GetComponent<PlayerAnimation>();
            playerInput = playerObject.GetComponent<PlayerInput>();
            characterController = playerObject.GetComponent<CharacterController>();
            playerRigidbody = playerObject.GetComponent<Rigidbody>();
        }

        if (fadeImage != null) {
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
        }

        if (skipButton != null)
            skipButton.onClick.AddListener(SkipTutorial);
    }

    /// <summary>
    /// Starts the tutorial sequence.
    /// </summary>
    public void StartTutorial() {
        if (isRunning) return;
        StartCoroutine(TutorialRoutine());
    }

    /// <summary>
    /// Main tutorial flow controlling movement, messages, and transitions.
    /// </summary>
    private IEnumerator TutorialRoutine() {
        isRunning = true;

        DisablePlayer();

        if (playerAnimator != null) {
            playerAnimator.SetFloat(inputXHash, 0f);
            playerAnimator.SetFloat(inputYHash, 1f);
            playerAnimator.SetFloat(inputMagnitudeHash, 1f);
        }

        messageCoroutine = StartCoroutine(MessageRoutine());

        yield return MovePlayer(point1);
        yield return MovePlayer(point2);
        yield return MovePlayer(point3);

        yield return RotateLeft90();
        yield return new WaitForSeconds(pauseAfterRotation);

        yield return MovePlayer(point4);

        StopMessage();
        StopAnimation();

        if (tutorialText != null && tutorialMessages.Length > 0)
            tutorialText.text = tutorialMessages[tutorialMessages.Length - 1];

        tutorialFinished = true;
        //Save that the tutorial got saved
        SaveManager.Instance.SaveData(ID, new TutorialData {
            tutorialFinished = tutorialFinished
        });
        //Debug.Log("Saved tutorialFinished!" + tutorialFinished);

        yield return new WaitForSeconds(2f);

        yield return FadeToScene(nextScene);
    }

    /// <summary>
    /// Moves the player to a target position.
    /// </summary>
    private IEnumerator MovePlayer(Transform target) {
        if (target == null || playerObject == null) yield break;

        while (Vector3.Distance(playerObject.transform.position, target.position) > 0.05f) {
            playerObject.transform.position = Vector3.MoveTowards(
                playerObject.transform.position,
                target.position,
                playerMoveSpeed * Time.deltaTime
            );

            yield return null;
        }

        playerObject.transform.position = target.position;
    }

    /// <summary>
    /// Rotates the player 90 degrees to the left.
    /// </summary>
    private IEnumerator RotateLeft90() {
        Quaternion start = playerObject.transform.rotation;
        Quaternion target = start * Quaternion.Euler(0f, -90f, 0f);

        float t = 0f;

        while (t < rotationDuration) {
            t += Time.deltaTime;
            float lerp = t / rotationDuration;

            playerObject.transform.rotation = Quaternion.Slerp(start, target, lerp);
            yield return null;
        }

        playerObject.transform.rotation = target;
    }

    /// <summary>
    /// Displays tutorial messages sequentially.
    /// </summary>
    private IEnumerator MessageRoutine() {
        foreach (var msg in tutorialMessages) {
            tutorialText.text = msg;
            yield return new WaitForSeconds(messageDelay);
        }
    }

    /// <summary>
    /// Stops the message coroutine.
    /// </summary>
    private void StopMessage() {
        if (messageCoroutine != null)
            StopCoroutine(messageCoroutine);
    }

    /// <summary>
    /// Resets animator movement values.
    /// </summary>
    private void StopAnimation() {
        if (playerAnimator == null) return;

        playerAnimator.SetFloat(inputXHash, 0f);
        playerAnimator.SetFloat(inputYHash, 0f);
        playerAnimator.SetFloat(inputMagnitudeHash, 0f);
    }

    /// <summary>
    /// Disables player control during tutorial.
    /// </summary>
    private void DisablePlayer() {
        if (aimLayer != null) aimLayer.weight = 0f;
        if (playerInputHandler != null) playerInputHandler.enabled = false;
        if (playerAnimation != null) playerAnimation.enabled = false;
        if (playerScript != null) playerScript.enabled = false;
        if (playerInput != null) playerInput.enabled = false;
        if (characterController != null) characterController.enabled = false;

        if (playerRigidbody != null) {
            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
            playerRigidbody.isKinematic = true;
        }
    }

    /// <summary>
    /// Skips the tutorial and starts scene transition immediately.
    /// </summary>
    public void SkipTutorial() {
        if (isSkipping) return;
        StartCoroutine(SkipRoutine());
    }

    /// <summary>
    /// Handles tutorial skip flow.
    /// </summary>
    private IEnumerator SkipRoutine() {
        isSkipping = true;
        isRunning = false;

        StopAnimation();

        if (tutorialText != null)
            tutorialText.text = "";

        if (skipButton != null)
            skipButton.gameObject.SetActive(false);

        RestorePlayerControlState();

        tutorialFinished = true;
        //Save that the tutorial got saved
        SaveManager.Instance.SaveData(ID, new TutorialData {
            tutorialFinished = tutorialFinished
        });

        yield return null;

        yield return FadeToScene(nextScene);
    }

    /// <summary>
    /// Fades out and loads a new scene.
    /// </summary>
    private IEnumerator FadeToScene(Loader.Scene scene) {
        if (fadeImage == null) {
            Loader.Load(scene);
            yield break;
        }

        float t = 0f;
        Color c = fadeImage.color;

        while (t < fadeDuration) {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(0f, 1f, t / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }

        c.a = 1f;
        fadeImage.color = c;

        Loader.Load(scene);
    }

    /// <summary>
    /// Restores player control after skipping.
    /// </summary>
    private void RestorePlayerControlState() {
        if (aimLayer != null)
            aimLayer.weight = 1f;

        if (playerInputHandler != null)
            playerInputHandler.enabled = true;

        if (playerAnimation != null)
            playerAnimation.enabled = true;

        if (playerScript != null)
            playerScript.enabled = true;

        if (playerInput != null)
            playerInput.enabled = true;

        if (characterController != null)
            characterController.enabled = true;

        if (playerRigidbody != null) {
            playerRigidbody.isKinematic = false;
            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
        }
    }

    public class TutorialData {
        public bool tutorialFinished = false;
    }
}