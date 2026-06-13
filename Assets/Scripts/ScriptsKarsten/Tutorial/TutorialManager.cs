using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Animations.Rigging;

/// <summary>
/// Steuert das Tutorial: Spieler läuft zu einem Zielpunkt,
/// während nacheinander Texte angezeigt werden.
/// Am Ende werden Steuerung und Aim-Layer wieder aktiviert.
/// </summary>
public class TutorialManager : MonoBehaviour {
    [Header("Player")]
    public GameObject playerObject;
    public Animator playerAnimator;
    public Transform playerMoveTarget;
    public float playerMoveSpeed = 1.5f;

    [Header("Rig")]
    public Rig aimLayer;

    [Header("Text")]
    public TMP_Text tutorialText;
    public string[] tutorialMessages;
    public float messageDelay = 5f;

    private Player playerScript;
    private PlayerInputHandler playerInputHandler;
    private PlayerAnimation playerAnimation;
    private PlayerInput playerInput;
    private CharacterController characterController;
    private Rigidbody playerRigidbody;

    private bool isRunning;

    private static readonly int inputXHash = Animator.StringToHash("inputX");
    private static readonly int inputYHash = Animator.StringToHash("inputY");
    private static readonly int inputMagnitudeHash = Animator.StringToHash("inputMagnitude");

    private void Awake() {
        if (playerObject != null) {
            playerScript = playerObject.GetComponent<Player>();
            playerInputHandler = playerObject.GetComponent<PlayerInputHandler>();
            playerAnimation = playerObject.GetComponent<PlayerAnimation>();
            playerInput = playerObject.GetComponent<PlayerInput>();
            characterController = playerObject.GetComponent<CharacterController>();
            playerRigidbody = playerObject.GetComponent<Rigidbody>();
        }

        if (tutorialText != null)
            tutorialText.text = "";
    }

    /// <summary>
    /// Startet die Tutorial-Sequenz, falls sie noch nicht läuft.
    /// </summary>
    public void StartTutorial() {
        if (isRunning)
            return;

        StartCoroutine(TutorialRoutine());
    }

    /// <summary>
    /// Voller Ablauf: Steuerung deaktivieren, Spieler laufen lassen,
    /// Messages anzeigen, Steuerung wieder aktivieren.
    /// </summary>
    private IEnumerator TutorialRoutine() {
        isRunning = true;

        // Aim-Layer aus
        if (aimLayer != null)
            aimLayer.weight = 0f;

        // Steuerung / Scripts aus
        if (playerInputHandler != null)
            playerInputHandler.enabled = false;

        if (playerAnimation != null)
            playerAnimation.enabled = false;

        if (playerScript != null)
            playerScript.enabled = false;

        if (playerInput != null)
            playerInput.enabled = false;

        if (characterController != null)
            characterController.enabled = false;

        if (playerRigidbody != null) {
            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
            playerRigidbody.isKinematic = true;
        }

        // Laufanimation erzwingen (wie im ExtractionController)
        if (playerAnimator != null) {
            playerAnimator.SetFloat(inputXHash, 0f);
            playerAnimator.SetFloat(inputYHash, 1f);
            playerAnimator.SetFloat(inputMagnitudeHash, 1f);
        }

        // Spieler zum Ziel bewegen
        yield return MovePlayerToTarget();

        // Nach Erreichen des Ziels Animation auf Idle zurück
        if (playerAnimator != null) {
            playerAnimator.SetFloat(inputMagnitudeHash, 0f);
            playerAnimator.SetFloat(inputXHash, 0f);
            playerAnimator.SetFloat(inputYHash, 0f);
        }

        // Tutorial-Messages anzeigen
        yield return ShowMessages();

        // Steuerung wieder aktivieren
        EnablePlayerControl();

        // Text leeren
        if (tutorialText != null)
            tutorialText.text = "";

        isRunning = false;
    }

    /// <summary>
    /// Bewegt den Spieler SMOOTH zum Zielpunkt.
    /// </summary>
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

    /// <summary>
    /// Zeigt alle Tutorial-Texte nacheinander an.
    /// </summary>
    private IEnumerator ShowMessages() {
        if (tutorialText == null || tutorialMessages == null)
            yield break;

        foreach (string msg in tutorialMessages) {
            tutorialText.text = msg;
            yield return new WaitForSeconds(messageDelay);
        }
    }

    /// <summary>
    /// Aktiviert nach dem Tutorial wieder alle relevanten Komponenten.
    /// </summary>
    private void EnablePlayerControl() {
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

        if (playerRigidbody != null)
            playerRigidbody.isKinematic = false;
    }
}