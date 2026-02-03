using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

/// <summary>
/// Zone cliquable dans l'appartement.
/// Gère le hover, le clic et les animations.
/// </summary>
public class ApartmentZone : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Configuration")]
    public string zoneName = "Zone";
    public string zoneDescription = "Description";

    [Header("Visuel")]
    public Image zoneImage;
    public Image highlightImage;
    public Image iconImage;

    [Header("Animation")]
    public float hoverScale = 1.1f;
    public float animationDuration = 0.2f;
    public bool pulseWhenIdle = false;

    // Callbacks
    private System.Action onClickCallback;
    private System.Action<string, string, Vector3> onHoverCallback;
    private System.Action onExitCallback;

    // État
    private Vector3 originalScale;
    private bool isHovered = false;
    private Tweener pulseTween;

    void Awake()
    {
        originalScale = transform.localScale;

        if (highlightImage != null)
        {
            highlightImage.color = new Color(1, 1, 1, 0);
        }
    }

    void Start()
    {
        // Désactive le pulse par défaut
        pulseWhenIdle = false;
    }

    /// <summary>
    /// Configure la zone avec ses callbacks
    /// </summary>
    public void Setup(string name, string description, System.Action onClick,
                      System.Action<string, string, Vector3> onHover, System.Action onExit)
    {
        zoneName = name;
        zoneDescription = description;
        onClickCallback = onClick;
        onHoverCallback = onHover;
        onExitCallback = onExit;
    }

    /// <summary>
    /// Animation d'entrée dans l'appartement
    /// </summary>
    public void AnimateEntry(float delay)
    {
        transform.localScale = Vector3.zero;
        transform.DOScale(originalScale, 0.4f)
            .SetEase(Ease.OutBack)
            .SetDelay(delay);
    }

    // ═══════════════════════════════════════════════════════════
    // ÉVÉNEMENTS POINTER
    // ═══════════════════════════════════════════════════════════

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;

        // Stop le pulse
        pulseTween?.Kill();

        // Scale up
        transform.DOKill();
        transform.DOScale(originalScale * hoverScale, animationDuration).SetEase(Ease.OutQuad);

        // Highlight
        if (highlightImage != null)
        {
            highlightImage.DOKill();
            highlightImage.DOFade(0.5f, animationDuration);
        }

        // Callback
        onHoverCallback?.Invoke(zoneName, zoneDescription, transform.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;

        // Scale down
        transform.DOKill();
        transform.DOScale(originalScale, animationDuration).SetEase(Ease.OutQuad);

        // Hide highlight
        if (highlightImage != null)
        {
            highlightImage.DOKill();
            highlightImage.DOFade(0f, animationDuration);
        }

        // Restart pulse
        if (pulseWhenIdle)
        {
            StartIdlePulse();
        }

        // Callback
        onExitCallback?.Invoke();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Animation de clic
        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOScale(originalScale * 0.95f, 0.1f).SetEase(Ease.InQuad));
        seq.Append(transform.DOScale(originalScale * hoverScale, 0.1f).SetEase(Ease.OutQuad));

        // Callback
        onClickCallback?.Invoke();
    }

    // ═══════════════════════════════════════════════════════════
    // ANIMATION IDLE
    // ═══════════════════════════════════════════════════════════

    void StartIdlePulse()
    {
        if (isHovered) return;

        pulseTween?.Kill();
        pulseTween = transform.DOScale(originalScale * 1.03f, 1f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    void OnDestroy()
    {
        pulseTween?.Kill();
        transform.DOKill();
    }
}