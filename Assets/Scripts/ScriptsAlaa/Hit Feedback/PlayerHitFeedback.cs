using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays a short blood vignette when the player takes damage.
/// </summary>
public class PlayerHitFeedback : MonoBehaviour
{
    // Color and intensity of the blood effect.
    [SerializeField] private Color vignetteColor = new Color(1f, 1f, 1f, 0.7f);

    // Short duration for which the red color remains visible.
    [SerializeField] private float holdDuration = 0.06f;

    // Time used to fade the effect out again.
    [SerializeField] private float fadeDuration = 0.3f;

    private static PlayerHitFeedback instance;
    private Image hitImage;
    private Coroutine effectRoutine;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateAutomatically()
    {
        // Creates the effect automatically so nothing must be assigned in the Inspector.
        PlayerHitFeedback existingFeedback = FindFirstObjectByType<PlayerHitFeedback>();
        if (existingFeedback != null)
        {
            instance = existingFeedback;
            return;
        }

        GameObject feedbackObject = new GameObject("Player Hit Feedback");
        feedbackObject.AddComponent<PlayerHitFeedback>();
    }

    private void Awake()
    {
        // Prevents multiple hit displays from existing at the same time.
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        CreateHitImage();
    }

    private void OnEnable()
    {
        // Reacts whenever the player takes damage.
        PlayerHealth.OnTakeDamage += ShowHitEffect;
    }

    private void OnDisable()
    {
        PlayerHealth.OnTakeDamage -= ShowHitEffect;
    }

    private void CreateHitImage()
    {
        // Creates a canvas over the entire screen.
        GameObject canvasObject = new GameObject("Hit Feedback Canvas");
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = short.MaxValue;

        CanvasScaler canvasScaler = canvasObject.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920f, 1080f);

        // Creates the blood vignette without blocking mouse clicks.
        GameObject imageObject = new GameObject("Hit Color");
        imageObject.transform.SetParent(canvasObject.transform, false);

        RectTransform imageRect = imageObject.AddComponent<RectTransform>();
        imageRect.anchorMin = Vector2.zero;
        imageRect.anchorMax = Vector2.one;
        imageRect.offsetMin = Vector2.zero;
        imageRect.offsetMax = Vector2.zero;

        hitImage = imageObject.AddComponent<Image>();
        hitImage.sprite = Resources.Load<Sprite>("BloodHit");
        hitImage.raycastTarget = false;
        hitImage.color = new Color(vignetteColor.r, vignetteColor.g, vignetteColor.b, 0f);
    }

    private void ShowHitEffect(Vector3 hitPosition)
    {
        // Restarts the effect when several hits occur.
        if (effectRoutine != null)
        {
            StopCoroutine(effectRoutine);
        }

        // Displays the blood vignette immediately when the hit occurs.
        hitImage.color = vignetteColor;
        hitImage.SetVerticesDirty();
        hitImage.SetMaterialDirty();
        Canvas.ForceUpdateCanvases();

        effectRoutine = StartCoroutine(PlayHitEffect());
    }

    private IEnumerator PlayHitEffect()
    {
        yield return new WaitForSecondsRealtime(holdDuration);

        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsedTime / fadeDuration);
            float alpha = Mathf.Lerp(vignetteColor.a, 0f, progress);
            hitImage.color = new Color(vignetteColor.r, vignetteColor.g, vignetteColor.b, alpha);
            yield return null;
        }

        hitImage.color = new Color(vignetteColor.r, vignetteColor.g, vignetteColor.b, 0f);
        effectRoutine = null;
    }
}
