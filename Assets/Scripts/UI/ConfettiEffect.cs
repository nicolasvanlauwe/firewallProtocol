using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

/// <summary>
/// Effet de confettis/feux d'artifice pour la victoire avec DOTween.
/// </summary>
public class ConfettiEffect : MonoBehaviour
{
    public static ConfettiEffect Instance;

    [Header("Configuration")]
    public Canvas parentCanvas;
    public int numberOfConfetti = 40;
    public float duration = 2.5f;

    [Header("Couleurs Confettis")]
    public Color[] confettiColors = new Color[]
    {
        new Color(1f, 0.84f, 0f, 1f),   // Or
        new Color(0.2f, 1f, 0.4f, 1f),  // Vert
        new Color(0f, 0.8f, 1f, 1f),    // Cyan
        new Color(1f, 0.4f, 0.7f, 1f),  // Rose
        new Color(1f, 1f, 1f, 1f),      // Blanc
        new Color(1f, 0.5f, 0f, 1f),    // Orange
        new Color(0.6f, 0.4f, 1f, 1f)   // Violet
    };

    private RectTransform confettiContainer;
    private List<Image> confettis = new List<Image>();
    private bool isInitialized = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        if (isInitialized) return;

        if (parentCanvas == null)
        {
            parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas == null)
                parentCanvas = FindObjectOfType<Canvas>();
        }

        if (parentCanvas == null)
        {
            Debug.LogError("ConfettiEffect: Pas de Canvas trouvé!");
            return;
        }

        // Crée le container
        GameObject containerObj = new GameObject("ConfettiContainer");
        containerObj.transform.SetParent(parentCanvas.transform, false);
        confettiContainer = containerObj.AddComponent<RectTransform>();
        confettiContainer.anchorMin = Vector2.zero;
        confettiContainer.anchorMax = Vector2.one;
        confettiContainer.offsetMin = Vector2.zero;
        confettiContainer.offsetMax = Vector2.zero;

        // Crée les confettis
        for (int i = 0; i < numberOfConfetti; i++)
        {
            GameObject confettiObj = new GameObject("Confetti_" + i);
            confettiObj.transform.SetParent(confettiContainer, false);

            RectTransform rect = confettiObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(Random.Range(10f, 25f), Random.Range(10f, 25f));

            Image img = confettiObj.AddComponent<Image>();
            img.raycastTarget = false;

            confettiObj.SetActive(false);
            confettis.Add(img);
        }

        confettiContainer.gameObject.SetActive(false);
        isInitialized = true;
    }

    /// <summary>
    /// Lance l'effet de confettis
    /// </summary>
    public void PlayConfetti()
    {
        if (!isInitialized) Initialize();

        confettiContainer.gameObject.SetActive(true);
        confettiContainer.SetAsLastSibling();

        RectTransform canvasRect = parentCanvas.GetComponent<RectTransform>();
        float canvasHeight = canvasRect.rect.height;
        float canvasWidth = canvasRect.rect.width;

        foreach (var confetti in confettis)
        {
            // Reset
            confetti.DOKill();
            RectTransform rect = confetti.rectTransform;

            // Position de départ : en haut de l'écran
            float startX = Random.Range(-canvasWidth * 0.4f, canvasWidth * 0.4f);
            float startY = canvasHeight * 0.6f;
            rect.anchoredPosition = new Vector2(startX, startY);

            // Taille aléatoire
            float size = Random.Range(12f, 28f);
            rect.sizeDelta = new Vector2(size, size);

            // Couleur aléatoire
            Color color = confettiColors[Random.Range(0, confettiColors.Length)];
            confetti.color = color;

            // Rotation initiale
            rect.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

            confetti.gameObject.SetActive(true);

            // Animation avec DOTween
            float confettiDuration = duration * Random.Range(0.8f, 1.2f);
            float endX = startX + Random.Range(-200f, 200f);
            float endY = -canvasHeight * 0.6f;

            // Mouvement avec courbe personnalisée (effet de chute avec rebond léger)
            Sequence seq = DOTween.Sequence();

            // Position X : mouvement sinusoïdal
            rect.DOAnchorPosX(endX, confettiDuration)
                .SetEase(Ease.InOutSine);

            // Position Y : chute avec accélération
            rect.DOAnchorPosY(endY, confettiDuration)
                .SetEase(Ease.InQuad);

            // Rotation continue
            rect.DORotate(new Vector3(0, 0, Random.Range(-720f, 720f)), confettiDuration, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear);

            // Fade out à la fin
            confetti.DOFade(0f, confettiDuration * 0.3f)
                .SetDelay(confettiDuration * 0.7f)
                .OnComplete(() => confetti.gameObject.SetActive(false));
        }

        // Désactive le container après la durée
        DOVirtual.DelayedCall(duration * 1.3f, () =>
        {
            confettiContainer.gameObject.SetActive(false);
        });
    }

    public void StopConfetti()
    {
        foreach (var confetti in confettis)
        {
            if (confetti != null)
            {
                confetti.DOKill();
                confetti.gameObject.SetActive(false);
            }
        }

        if (confettiContainer != null)
            confettiContainer.gameObject.SetActive(false);
    }
}
