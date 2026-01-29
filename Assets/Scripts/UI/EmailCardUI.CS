using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// Gère l'affichage et l'interaction avec la carte email.
/// Détecte les swipes gauche/droite pour approuver ou rejeter.
/// Effets visuels : contours colorés, glitch, feux d'artifice.
/// </summary>
public class EmailCardUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("📧 Éléments de Texte")]
    public TextMeshProUGUI expediteurText;
    public TextMeshProUGUI objetText;
    public TextMeshProUGUI corpsText;

    [Header("🎮 Paramètres du Swipe")]
    public float swipeThreshold = 150f;
    public float returnSpeed = 10f;

    [Header("🎨 Effets Visuels - Contours")]
    [Tooltip("Image de contour/bordure de la carte (doit entourer la carte)")]
    public Image cardOutline;
    
    [Tooltip("Couleur pour approuver (vert)")]
    public Color approveColor = new Color(0.2f, 0.9f, 0.3f, 1f);
    
    [Tooltip("Couleur pour rejeter (rouge)")]
    public Color rejectColor = new Color(0.9f, 0.2f, 0.2f, 1f);

    [Header("✨ Effets Spéciaux")]
    [Tooltip("Système de particules pour les feux d'artifice")]
    public ParticleSystem fireworksEffect;

    [Header("📳 Vibration (Mobile)")]
    [Tooltip("Activer les vibrations sur mobile")]
    public bool enableVibration = true;

    // Variables privées
    private Vector2 startDragPosition;
    private RectTransform rectTransform;
    private Canvas canvas;
    private Vector2 originalPosition;
    private bool isDragging = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        originalPosition = rectTransform.anchoredPosition;
    }

    void Start()
    {
        // Cache le contour au démarrage
        if (cardOutline != null)
        {
            cardOutline.gameObject.SetActive(false);
        }

        // Désactive les particules au démarrage
        if (fireworksEffect != null) fireworksEffect.Stop();
    }

    /// <summary>
    /// Affiche un nouvel email dans la carte.
    /// </summary>
    public void AfficherEmail(EmailData email)
    {
        expediteurText.text = "De: " + email.expediteurNom + " <" + email.expediteurEmail + ">";
        objetText.text = "Objet: " + email.objet;
        corpsText.text = email.corpsDuMessage;

        rectTransform.anchoredPosition = originalPosition;
        
        // Reset le contour
        HideOutline();
    }

    void HideOutline()
    {
        if (cardOutline != null)
        {
            cardOutline.gameObject.SetActive(false);
        }
    }

    void ShowOutline(Color color, float intensity)
    {
        if (cardOutline != null)
        {
            cardOutline.gameObject.SetActive(true);
            
            // Applique la couleur avec l'intensité
            Color colorWithAlpha = color;
            colorWithAlpha.a = intensity;
            cardOutline.color = colorWithAlpha;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // SYSTÈME DE DÉTECTION DU SWIPE
    // ═══════════════════════════════════════════════════════════

    public void OnBeginDrag(PointerEventData eventData)
    {
        startDragPosition = eventData.position;
        isDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        Vector2 dragDelta = eventData.position - startDragPosition;
        Vector2 newPosition = originalPosition + (dragDelta / canvas.scaleFactor);
        rectTransform.anchoredPosition = new Vector2(newPosition.x, originalPosition.y);

        // Calcule l'intensité du swipe (0 à 1)
        float swipeIntensity = Mathf.Clamp01(Mathf.Abs(dragDelta.x) / swipeThreshold);

        if (dragDelta.x > 50f)
        {
            // Swipe vers la DROITE = Approuver (VERT)
            ShowOutline(approveColor, swipeIntensity);
        }
        else if (dragDelta.x < -50f)
        {
            // Swipe vers la GAUCHE = Rejeter (ROUGE)
            ShowOutline(rejectColor, swipeIntensity);
        }
        else
        {
            // Swipe trop petit, cache le contour
            HideOutline();
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        Vector2 dragDelta = eventData.position - startDragPosition;

        if (Mathf.Abs(dragDelta.x) > swipeThreshold)
        {
            bool approuve = dragDelta.x > 0;
            Debug.Log(approuve ? "➡️ APPROUVÉ" : "⬅️ REJETÉ");

            // Informe le GameManager et récupère si c'était correct
            bool wasCorrect = GameManager.Instance.TraiterDecisionAvecRetour(approuve);
            
            // Joue les effets via EffectsManager (centralise tous les effets)
            if (EffectsManager.Instance != null)
            {
                if (wasCorrect)
                {
                    EffectsManager.Instance.PlayCorrectEffect();
                    PlayFireworks();
                }
                else
                {
                    EffectsManager.Instance.PlayWrongEffect();
                }
            }
            
            // Vibration sur mobile
            if (enableVibration)
            {
                if (wasCorrect)
                {
                    Vibrate(50);
                }
                else
                {
                    Vibrate(200);
                }
            }
        }
        
        // Cache le contour
        HideOutline();
    }

    void Update()
    {
        if (!isDragging)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(
                rectTransform.anchoredPosition,
                originalPosition,
                Time.deltaTime * returnSpeed
            );

            if (Vector2.Distance(rectTransform.anchoredPosition, originalPosition) < 1f)
            {
                HideOutline();
            }
        }
    }

    // ═══════════════════════════════════════════════════════════
    // EFFETS SPÉCIAUX
    // ═══════════════════════════════════════════════════════════

    void PlayFireworks()
    {
        if (fireworksEffect != null)
        {
            fireworksEffect.Play();
        }
    }

    void Vibrate(long milliseconds)
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaObject vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
                vibrator.Call("vibrate", milliseconds);
            }
        }
        catch (System.Exception e)
        {
            Debug.Log("Vibration non disponible: " + e.Message);
        }
        #elif UNITY_IOS && !UNITY_EDITOR
        Handheld.Vibrate();
        #endif
    }
}