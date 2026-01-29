using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

/// <summary>
/// Gère tous les effets visuels avec DOTween.
/// Animations subtiles et stylées.
/// </summary>
public class EffectsManager : MonoBehaviour
{
    public static EffectsManager Instance;

    [Header("🎨 Références UI")]
    [Tooltip("La carte email (pour les animations)")]
    public RectTransform emailCard;
    
    [Tooltip("Le canvas principal")]
    public RectTransform mainCanvas;
    
    [Tooltip("Contour de la carte (pour effet de bordure)")]
    public Image cardOutline;

    [Header("⚙️ Paramètres")]
    [Tooltip("Probabilité d'effet glitch (0-1)")]
    [Range(0f, 1f)]
    public float glitchProbability = 0.4f;

    // Variables privées
    private Vector2 emailCardOriginalPos;
    private Vector3 emailCardOriginalScale;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DOTween.Init();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (emailCard != null)
        {
            emailCardOriginalPos = emailCard.anchoredPosition;
            emailCardOriginalScale = emailCard.localScale;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // EFFETS BONNE RÉPONSE
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Effet de bonne réponse
    /// </summary>
    public void PlayCorrectEffect()
    {
        // Petit bounce satisfaisant de la carte
        if (emailCard != null)
        {
            emailCard.DOKill();
            Sequence seq = DOTween.Sequence();
            seq.Append(emailCard.DOScale(emailCardOriginalScale * 1.05f, 0.1f).SetEase(Ease.OutQuad));
            seq.Append(emailCard.DOScale(emailCardOriginalScale, 0.2f).SetEase(Ease.OutBounce));
        }
    }

    // ═══════════════════════════════════════════════════════════
    // EFFETS MAUVAISE RÉPONSE
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Effet de mauvaise réponse
    /// </summary>
    public void PlayWrongEffect()
    {
        // Effet glitch seulement (avec probabilité)
        if (Random.value < glitchProbability && GlitchEffect.Instance != null)
        {
            GlitchEffect.Instance.PlayGlitch();
        }
    }

    // ═══════════════════════════════════════════════════════════
    // EFFETS VICTOIRE
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Effet de victoire - Confettis !
    /// </summary>
    public void PlayVictoryEffect()
    {
        if (ConfettiEffect.Instance != null)
        {
            ConfettiEffect.Instance.PlayConfetti();
        }
    }

    // ═══════════════════════════════════════════════════════════
    // EFFETS GAME OVER
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Effet de Game Over - Glitch intense puis callback
    /// </summary>
    public void PlayGameOverEffect(System.Action onComplete = null)
    {
        if (GlitchEffect.Instance != null)
        {
            GlitchEffect.Instance.PlayIntenseGlitch(onComplete);
        }
        else
        {
            // Fallback si pas de GlitchEffect
            onComplete?.Invoke();
        }
    }

    // ═══════════════════════════════════════════════════════════
    // UTILITAIRES
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Anime un élément avec un bounce
    /// </summary>
    public void AnimateBounce(RectTransform element)
    {
        if (element == null) return;
        
        element.localScale = Vector3.zero;
        element.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
    }

    /// <summary>
    /// Reset tous les effets
    /// </summary>
    public void ResetAllEffects()
    {
        DOTween.KillAll();
        
        if (emailCard != null)
        {
            emailCard.anchoredPosition = emailCardOriginalPos;
            emailCard.localScale = emailCardOriginalScale;
        }
        
        if (mainCanvas != null)
        {
            mainCanvas.anchoredPosition = Vector2.zero;
        }
        
        if (GlitchEffect.Instance != null)
        {
            GlitchEffect.Instance.StopGlitch();
        }
    }

    void OnDestroy()
    {
        DOTween.Kill(this);
    }
}