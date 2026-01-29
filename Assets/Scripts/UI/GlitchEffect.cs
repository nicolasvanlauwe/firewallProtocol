using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

/// <summary>
/// Effet de glitch intense style cyberpunk avec DOTween.
/// </summary>
public class GlitchEffect : MonoBehaviour
{
    public static GlitchEffect Instance;

    [Header("Configuration")]
    public Canvas parentCanvas;
    public int numberOfBands = 30;
    public float intenseDuration = 1.5f;
    public float lightDuration = 0.4f;

    [Header("Couleurs Glitch")]
    public Color glitchCyan = new Color(0f, 1f, 1f, 0.8f);
    public Color glitchMagenta = new Color(1f, 0f, 0.8f, 0.8f);
    public Color glitchGreen = new Color(0f, 1f, 0.3f, 0.8f);
    public Color glitchRed = new Color(1f, 0.1f, 0.1f, 0.7f);

    private RectTransform glitchContainer;
    private List<Image> glitchBands = new List<Image>();
    private Image blackOverlay;
    private bool isInitialized = false;
    private bool isPlaying = false;
    private Sequence currentSequence;

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
            Debug.LogError("GlitchEffect: Pas de Canvas trouvé!");
            return;
        }

        // Crée le container
        GameObject containerObj = new GameObject("GlitchContainer");
        containerObj.transform.SetParent(parentCanvas.transform, false);
        glitchContainer = containerObj.AddComponent<RectTransform>();
        glitchContainer.anchorMin = Vector2.zero;
        glitchContainer.anchorMax = Vector2.one;
        glitchContainer.offsetMin = Vector2.zero;
        glitchContainer.offsetMax = Vector2.zero;

        // Crée l'overlay noir
        GameObject blackObj = new GameObject("BlackOverlay");
        blackObj.transform.SetParent(glitchContainer, false);
        RectTransform blackRect = blackObj.AddComponent<RectTransform>();
        blackRect.anchorMin = Vector2.zero;
        blackRect.anchorMax = Vector2.one;
        blackRect.offsetMin = Vector2.zero;
        blackRect.offsetMax = Vector2.zero;
        blackOverlay = blackObj.AddComponent<Image>();
        blackOverlay.color = new Color(0, 0, 0, 0);
        blackOverlay.raycastTarget = false;

        // Crée les bandes
        for (int i = 0; i < numberOfBands; i++)
        {
            GameObject bandObj = new GameObject("Band_" + i);
            bandObj.transform.SetParent(glitchContainer, false);

            RectTransform rect = bandObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0.5f);
            rect.anchorMax = new Vector2(1, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(0, 50);

            Image img = bandObj.AddComponent<Image>();
            img.raycastTarget = false;
            img.color = glitchCyan;

            bandObj.SetActive(false);
            glitchBands.Add(img);
        }

        glitchContainer.gameObject.SetActive(false);
        isInitialized = true;
    }

    /// <summary>
    /// Glitch léger pour erreur normale
    /// </summary>
    public void PlayGlitch()
    {
        if (isPlaying) return;
        if (!isInitialized) Initialize();

        isPlaying = true;
        glitchContainer.gameObject.SetActive(true);
        glitchContainer.SetAsLastSibling();

        currentSequence = DOTween.Sequence();

        // Plusieurs "flashs" de glitch
        int flashCount = 8;
        float flashDuration = lightDuration / flashCount;

        for (int f = 0; f < flashCount; f++)
        {
            currentSequence.AppendCallback(() => RandomizeBands(Random.Range(3, 8)));
            currentSequence.AppendInterval(flashDuration);
        }

        currentSequence.OnComplete(() =>
        {
            HideAllBands();
            glitchContainer.gameObject.SetActive(false);
            isPlaying = false;
        });
    }

    /// <summary>
    /// Glitch intense pour Game Over
    /// </summary>
    public void PlayIntenseGlitch(System.Action onComplete)
    {
        if (isPlaying)
        {
            onComplete?.Invoke();
            return;
        }
        if (!isInitialized) Initialize();

        isPlaying = true;
        glitchContainer.gameObject.SetActive(true);
        glitchContainer.SetAsLastSibling();
        blackOverlay.color = new Color(0, 0, 0, 0);

        float glitchPhase = intenseDuration * 0.7f;
        float fadePhase = intenseDuration * 0.3f;

        currentSequence = DOTween.Sequence();

        // Phase 1: Glitch intense avec shake croissant
        int steps = Mathf.RoundToInt(glitchPhase / 0.03f);
        for (int i = 0; i < steps; i++)
        {
            float progress = (float)i / steps;
            int bandsCount = Mathf.RoundToInt(Mathf.Lerp(5, numberOfBands, progress));
            float shake = Mathf.Lerp(5f, 25f, progress);

            currentSequence.AppendCallback(() =>
            {
                RandomizeBands(bandsCount);
                glitchContainer.anchoredPosition = new Vector2(
                    Random.Range(-shake, shake),
                    Random.Range(-shake * 0.3f, shake * 0.3f)
                );
            });
            currentSequence.AppendInterval(0.03f);
        }

        // Phase 2: Fade to black
        currentSequence.Append(blackOverlay.DOFade(1f, fadePhase).SetEase(Ease.InQuad));

        // Reset et callback
        currentSequence.AppendCallback(() =>
        {
            glitchContainer.anchoredPosition = Vector2.zero;
            HideAllBands();
            onComplete?.Invoke();
        });

        // Nettoyage final
        currentSequence.AppendCallback(() =>
        {
            blackOverlay.color = new Color(0, 0, 0, 0);
            glitchContainer.gameObject.SetActive(false);
            isPlaying = false;
        });
    }

    void RandomizeBands(int count)
    {
        HideAllBands();

        float canvasHeight = parentCanvas.GetComponent<RectTransform>().rect.height;
        float canvasWidth = parentCanvas.GetComponent<RectTransform>().rect.width;
        Color[] colors = { glitchCyan, glitchMagenta, glitchGreen, glitchRed };

        for (int i = 0; i < Mathf.Min(count, glitchBands.Count); i++)
        {
            Image band = glitchBands[Random.Range(0, glitchBands.Count)];
            RectTransform rect = band.rectTransform;

            // Position Y aléatoire
            float posY = Random.Range(-canvasHeight * 0.5f, canvasHeight * 0.5f);
            float offsetX = Random.Range(-50f, 50f);

            // Taille
            float height = Random.Range(10f, 100f);
            float width = Random.Range(canvasWidth * 0.5f, canvasWidth * 1.2f);

            rect.sizeDelta = new Vector2(width, height);
            rect.anchoredPosition = new Vector2(offsetX, posY);

            // Couleur
            Color color = colors[Random.Range(0, colors.Length)];
            color.a = Random.Range(0.5f, 0.9f);
            band.color = color;

            band.gameObject.SetActive(true);
        }
    }

    void HideAllBands()
    {
        foreach (var band in glitchBands)
            band.gameObject.SetActive(false);
    }

    public void StopGlitch()
    {
        currentSequence?.Kill();
        isPlaying = false;

        HideAllBands();

        if (blackOverlay != null)
            blackOverlay.color = new Color(0, 0, 0, 0);

        if (glitchContainer != null)
        {
            glitchContainer.anchoredPosition = Vector2.zero;
            glitchContainer.gameObject.SetActive(false);
        }
    }
}
