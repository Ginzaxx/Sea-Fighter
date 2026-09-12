using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class DynamicTouchController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("UI Visual Indicator")]
    [Tooltip("The RectTransform of the button container that appears at touch position.")]
    [SerializeField] private RectTransform touchContainer;

    [FormerlySerializedAs("leftButtonImage")]
    [SerializeField] private Image buttonImage;

    [Header("Visual Feedback Animation")]
    [SerializeField] private float appearDuration = 0.15f;
    [SerializeField] private float disappearDuration = 0.15f;
    [SerializeField] private float idleScaleAmount = 0.08f;
    [SerializeField] private float idlePulseSpeed = 4f;

    private bool isTouching = false;
    private CanvasGroup buttonCanvasGroup;
    private Vector3 baseScale = Vector3.one;
    private Coroutine animationRoutine;
    private RectTransform selfRectTransform;
    private Canvas parentCanvas;
    private Image raycastImage;

    private void Awake()
    {
        selfRectTransform = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();

        // Ensure full screen raycast overlay so touches anywhere on screen are captured
        EnsureFullScreenOverlay();

        if (touchContainer != null)
        {
            buttonCanvasGroup = touchContainer.GetComponent<CanvasGroup>();
            if (buttonCanvasGroup == null) buttonCanvasGroup = touchContainer.gameObject.AddComponent<CanvasGroup>();

            baseScale = touchContainer.localScale;
            buttonCanvasGroup.alpha = 0f;
            touchContainer.gameObject.SetActive(false);
        }
    }

    private void EnsureFullScreenOverlay()
    {
        // Ensure image component with Raycast Target enabled exists for receiving full-screen touches
        raycastImage = GetComponent<Image>();
        if (raycastImage == null)
        {
            raycastImage = gameObject.AddComponent<Image>();
        }
        raycastImage.color = new Color(0f, 0f, 0f, 0f); // Completely transparent
        raycastImage.raycastTarget = true;

        // Stretch to cover full screen
        if (selfRectTransform != null)
        {
            selfRectTransform.anchorMin = Vector2.zero;
            selfRectTransform.anchorMax = Vector2.one;
            selfRectTransform.offsetMin = Vector2.zero;
            selfRectTransform.offsetMax = Vector2.one;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isTouching = true;

        if (InputManager.Instance != null)
        {
            InputManager.Instance.UseFixedInputs = true;
        }

        // Position the button container at the touch point anywhere on screen
        PositionTouchContainer(eventData);

        if (touchContainer != null)
        {
            touchContainer.gameObject.SetActive(true);
        }

        StartAppearAnimation();
        ProcessInput(eventData.position);
    }

    private void PositionTouchContainer(PointerEventData eventData)
    {
        if (touchContainer == null) return;

        Canvas canvas = parentCanvas != null ? parentCanvas : GetComponentInParent<Canvas>();
        Camera cam = (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay) ? eventData.pressEventCamera : null;

        RectTransform parentRect = touchContainer.parent as RectTransform;
        if (parentRect != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, eventData.position, cam, out Vector2 localPoint))
        {
            touchContainer.anchoredPosition = localPoint;
        }
        else
        {
            touchContainer.position = eventData.position;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isTouching) return;
        ProcessInput(eventData.position);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ReleaseInput(true);
    }

    private void OnDisable()
    {
        if (raycastImage != null) raycastImage.raycastTarget = false;
        ReleaseInput(false);
    }

    private void ReleaseInput(bool animate)
    {
        isTouching = false;

        if (InputManager.Instance != null)
        {
            InputManager.Instance.ReleaseMobileMoveX(1f);
            InputManager.Instance.ReleaseMobileMoveX(-1f);
        }

        if (animate)
        {
            StartDisappearAnimation();
        }
        else
        {
            StopAnimation();
            HideButtonImmediately();
        }
    }

    private void StartAppearAnimation()
    {
        StopAnimation();
        animationRoutine = StartCoroutine(AppearAndIdle());
    }

    private void StartDisappearAnimation()
    {
        StopAnimation();
        if (buttonCanvasGroup == null || touchContainer == null)
        {
            HideButtonImmediately();
            return;
        }

        animationRoutine = StartCoroutine(Disappear());
    }

    private System.Collections.IEnumerator AppearAndIdle()
    {
        if (buttonCanvasGroup == null || touchContainer == null) yield break;

        float elapsed = 0f;
        Vector3 startScale = baseScale * 0.7f;
        touchContainer.localScale = startScale;
        buttonCanvasGroup.alpha = 0f;

        while (elapsed < appearDuration && isTouching)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = appearDuration <= 0f ? 1f : Mathf.Clamp01(elapsed / appearDuration);
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);
            touchContainer.localScale = Vector3.LerpUnclamped(startScale, baseScale, easedProgress);
            buttonCanvasGroup.alpha = easedProgress;
            yield return null;
        }

        while (isTouching)
        {
            float pulse = 1f + Mathf.Sin(Time.unscaledTime * idlePulseSpeed) * idleScaleAmount;
            touchContainer.localScale = baseScale * pulse;
            buttonCanvasGroup.alpha = 1f;
            yield return null;
        }
    }

    private void ProcessInput(Vector2 currentPos)
    {
        float screenMiddle = Screen.width / 2f;
        float moveX = (currentPos.x < screenMiddle) ? -1f : 1f;

        if (InputManager.Instance != null && moveX != 0f)
        {
            InputManager.Instance.SetMobileMoveX(moveX);
        }
    }

    private System.Collections.IEnumerator Disappear()
    {
        float elapsed = 0f;
        Vector3 startScale = touchContainer.localScale;
        float startAlpha = buttonCanvasGroup.alpha;

        while (elapsed < disappearDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = disappearDuration <= 0f ? 1f : Mathf.Clamp01(elapsed / disappearDuration);
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);
            touchContainer.localScale = Vector3.Lerp(startScale, baseScale * 0.7f, easedProgress);
            buttonCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, easedProgress);
            yield return null;
        }

        HideButtonImmediately();
    }

    private void StopAnimation()
    {
        if (animationRoutine != null)
        {
            StopCoroutine(animationRoutine);
            animationRoutine = null;
        }
    }

    private void HideButtonImmediately()
    {
        if (touchContainer != null)
        {
            touchContainer.localScale = baseScale;
            touchContainer.gameObject.SetActive(false);
        }

        if (buttonCanvasGroup != null) buttonCanvasGroup.alpha = 0f;
    }
}
