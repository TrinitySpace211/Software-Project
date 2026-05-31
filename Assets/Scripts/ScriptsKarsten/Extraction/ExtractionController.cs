using UnityEngine;
using System.Collections;
using UnityEngine.Animations.Rigging;
using TMPro;
using UnityEngine.UI;

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
    public string endMessage = "The 10 dayz are over, but the nightmare isn't.";
    public float endTextVisibleTime = 2f;

    [Header("Buttons")]
    public Button retryButton;
    public Button mainMenuButton;
    public Button exitButton;


    public float buttonFadeDuration = 1.5f;

    // CanvasGroups für die Buttons
    private CanvasGroup retryButtonCanvasGroup;
    private CanvasGroup mainMenuButtonCanvasGroup;
    private CanvasGroup exitButtonCanvasGroup;


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

        // Buttons initial verstecken
        if (retryButton != null) {
            retryButtonCanvasGroup = retryButton.GetComponent<CanvasGroup>();
            if (retryButtonCanvasGroup == null)
                retryButtonCanvasGroup = retryButton.gameObject.AddComponent<CanvasGroup>();

            retryButtonCanvasGroup.alpha = 0f;
            retryButtonCanvasGroup.interactable = false;
            retryButtonCanvasGroup.blocksRaycasts = false;
            retryButton.gameObject.SetActive(true); // Objekt aktiv, aber unsichtbar
        }

        if (mainMenuButton != null) {
            mainMenuButtonCanvasGroup = mainMenuButton.GetComponent<CanvasGroup>();
            if (mainMenuButtonCanvasGroup == null)
                mainMenuButtonCanvasGroup = mainMenuButton.gameObject.AddComponent<CanvasGroup>();

            mainMenuButtonCanvasGroup.alpha = 0f;
            mainMenuButtonCanvasGroup.interactable = false;
            mainMenuButtonCanvasGroup.blocksRaycasts = false;
            mainMenuButton.gameObject.SetActive(true);
        }

        if (exitButton != null) {
            exitButtonCanvasGroup = exitButton.GetComponent<CanvasGroup>();
            if (exitButtonCanvasGroup == null)
                exitButtonCanvasGroup = exitButton.gameObject.AddComponent<CanvasGroup>();

            exitButtonCanvasGroup.alpha = 0f;
            exitButtonCanvasGroup.interactable = false;
            exitButtonCanvasGroup.blocksRaycasts = false;
            exitButton.gameObject.SetActive(true);
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

        // Buttons sind bereits aktiv ( setActive(true) in Awake ), aber unsichtbar (alpha = 0)
        // Jetzt 2 Sekunden warten, bevor sie reinfaden
        yield return new WaitForSeconds(3f);

        // Buttons fadeInen
        if (retryButtonCanvasGroup != null)
            StartCoroutine(FadeButton(retryButtonCanvasGroup, 0f, 0.9f));

        if (mainMenuButtonCanvasGroup != null)
            StartCoroutine(FadeButton(mainMenuButtonCanvasGroup, 0f, 0.9f));

        if (exitButtonCanvasGroup != null)
            StartCoroutine(FadeButton(exitButtonCanvasGroup, 0f, 0.9f));

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

    // Fade für Button-CanvasGroup (kürzer, z. B. 0.5s)
    // Fade für Button-CanvasGroup
    private IEnumerator FadeButton(CanvasGroup group, float from, float to) {
        if (group == null)
            yield break;

        float duration = buttonFadeDuration;
        float t = 0f;

        group.alpha = from;
        group.interactable = false;
        group.blocksRaycasts = false;

        while (t < duration) {
            t += Time.deltaTime;
            float normalizedTime = t / duration;

            normalizedTime = Mathf.Clamp01(normalizedTime);

            float easeOut = normalizedTime * normalizedTime * (3f - 2f * normalizedTime);

            group.alpha = Mathf.Lerp(from, to, easeOut);
            yield return null;
        }


        group.alpha = to; 
        group.interactable = true;
        group.blocksRaycasts = true;
    }

    public void OnExitClicked() {

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    public void OnRetryClicked() {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
    }

    public void OnMainMenuClicked() {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu"); // Scene-Namen anpassen
    }
}