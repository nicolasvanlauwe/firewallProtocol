using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

/// <summary>
/// Effet de glitch intense style cyberpunk/corruption numérique.
/// Couvre tout l'écran avec des bandes colorées et des décalages.
/// </summary>
public class GlitchEffect : MonoBehaviour
{
    public static GlitchEffect Instance;

    [Header("🎨 Configuration")]
    [Tooltip("Canvas parent")]
    public Canvas parentCanvas;
    
    [Tooltip("Nombre de bandes de glitch")]
    public int numberOfBands = 40;
    
    [Tooltip("Durée de l'effet intense")]
    public float intenseDuration = 1.5f;

    [Header("🎨 Couleurs Glitch")]
    public Color glitchCyan = new Color(0f, 1f, 1f, 0.8f);
    public Color glitchMagenta = new Color(1f, 0f, 0.8f, 0.8f);
    public Color glitchGreen = new Color(0f, 1f, 0.3f, 0.8f);
    public Color glitchRed = new Color(1f, 0.1f, 0.1f, 0.7f);
    public Color glitchWhite = new Color(1f, 1f, 1f, 0.6f);
    public Color glitchBlack = new Color(0f, 0f, 0f, 0.9f);

    // Composants
    private RectTransform glitchContainer;
    private List<Image> glitchBands = new List<Image>();
    private Image blackOverlay;
    private bool isPlaying = false;
    
    // Callback après l'effet
    private System.Action onEffectComplete;

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
        SetupGlitchSystem();
    }

    void SetupGlitchSystem()
    {
        // Trouve le canvas
        if (parentCanvas == null)
        {
            parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas == null)
            {
                parentCanvas = FindObjectOfType<Canvas>();
            }
        }

        // Crée le container
        GameObject containerObj = new GameObject("GlitchContainer");
        containerObj.transform.SetParent(parentCanvas.transform, false);
        glitchContainer = containerObj.AddComponent<RectTransform>();
        
        // Plein écran
        glitchContainer.anchorMin = Vector2.zero;
        glitchContainer.anchorMax = Vector2.one;
        glitchContainer.offsetMin = Vector2.zero;
        glitchContainer.offsetMax = Vector2.zero;
        
        // Au-dessus de tout
        glitchContainer.SetAsLastSibling();
        
        // Crée l'overlay noir pour la fin
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
        blackObj.SetActive(false);

        // Crée les bandes
        for (int i = 0; i < numberOfBands; i++)
        {
            CreateGlitchBand(i);
        }

        // Cache tout au départ
        glitchContainer.gameObject.SetActive(false);
    }

    void CreateGlitchBand(int index)
    {
        GameObject bandObj = new GameObject($"GlitchBand_{index}");
        bandObj.transform.SetParent(glitchContainer, false);
        
        RectTransform rect = bandObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0.5f);
        rect.anchorMax = new Vector2(1, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(0, 20);
        rect.anchoredPosition = Vector2.zero;
        
        Image img = bandObj.AddComponent<Image>();
        img.raycastTarget = false;
        
        bandObj.SetActive(false);
        glitchBands.Add(img);
    }

    /// <summary>
    /// Lance l'effet de glitch léger (erreur normale)
    /// </summary>
    public void PlayGlitch()
    {
        if (isPlaying) return;
        StartCoroutine(LightGlitchSequence());
    }

    /// <summary>
    /// Lance l'effet de glitch INTENSE (Game Over)
    /// </summary>
    public void PlayIntenseGlitch(System.Action onComplete = null)
    {
        if (isPlaying) return;
        onEffectComplete = onComplete;
        StartCoroutine(IntenseGlitchSequence());
    }

    // ═══════════════════════════════════════════════════════════
    // GLITCH LÉGER (erreur normale)
    // ═══════════════════════════════════════════════════════════
    
    IEnumerator LightGlitchSequence()
    {
        isPlaying = true;
        glitchContainer.gameObject.SetActive(true);
        
        float duration = 0.5f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            
            // Active quelques bandes aléatoires
            int bandsToShow = Random.Range(3, 8);
            
            // Cache toutes les bandes
            foreach (var band in glitchBands)
            {
                band.gameObject.SetActive(false);
            }
            
            // Active quelques bandes aléatoires
            for (int i = 0; i < bandsToShow; i++)
            {
                int idx = Random.Range(0, glitchBands.Count);
                RandomizeBand(glitchBands[idx], true);
            }
            
            yield return new WaitForSeconds(0.05f);
        }
        
        // Cache tout
        foreach (var band in glitchBands)
        {
            band.gameObject.SetActive(false);
        }
        
        glitchContainer.gameObject.SetActive(false);
        isPlaying = false;
    }

    // ═══════════════════════════════════════════════════════════
    // GLITCH INTENSE (Game Over)
    // ═══════════════════════════════════════════════════════════
    
    IEnumerator IntenseGlitchSequence()
    {
        isPlaying = true;
        glitchContainer.gameObject.SetActive(true);
        glitchContainer.SetAsLastSibling();
        
        float elapsed = 0f;
        float phase1Duration = intenseDuration * 0.7f;  // Glitch intense
        float phase2Duration = intenseDuration * 0.3f;  // Fade to black
        
        // Phase 1 : Glitch intense
        while (elapsed < phase1Duration)
        {
            elapsed += Time.deltaTime;
            float intensity = elapsed / phase1Duration;
            
            // Plus on avance, plus c'est intense
            int bandsToShow = Mathf.RoundToInt(Mathf.Lerp(10, numberOfBands, intensity));
            
            // Randomize toutes les bandes
            for (int i = 0; i < glitchBands.Count; i++)
            {
                if (i < bandsToShow)
                {
                    RandomizeBand(glitchBands[i], false);
                }
                else
                {
                    glitchBands[i].gameObject.SetActive(false);
                }
            }
            
            // Shake le container
            float shakeAmount = Mathf.Lerp(5f, 30f, intensity);
            glitchContainer.anchoredPosition = new Vector2(
                Random.Range(-shakeAmount, shakeAmount),
                Random.Range(-shakeAmount * 0.3f, shakeAmount * 0.3f)
            );
            
            yield return new WaitForSeconds(Random.Range(0.02f, 0.08f));
        }
        
        // Phase 2 : Fade to black
        blackOverlay.gameObject.SetActive(true);
        blackOverlay.transform.SetAsLastSibling();
        
        elapsed = 0f;
        while (elapsed < phase2Duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / phase2Duration;
            
            // Fade in du noir
            blackOverlay.color = new Color(0, 0, 0, Mathf.Lerp(0, 1, t));
            
            // Quelques glitchs qui continuent
            if (Random.value > 0.5f)
            {
                int idx = Random.Range(0, glitchBands.Count);
                RandomizeBand(glitchBands[idx], false);
            }
            
            yield return null;
        }
        
        // Reset position
        glitchContainer.anchoredPosition = Vector2.zero;
        
        // Cache les bandes mais garde le noir
        foreach (var band in glitchBands)
        {
            band.gameObject.SetActive(false);
        }
        
        isPlaying = false;
        
        // Callback
        onEffectComplete?.Invoke();
    }

    void RandomizeBand(Image band, bool lightMode)
    {
        RectTransform rect = band.rectTransform;
        float canvasHeight = parentCanvas.GetComponent<RectTransform>().rect.height;
        float canvasWidth = parentCanvas.GetComponent<RectTransform>().rect.width;
        
        // Position Y aléatoire sur tout l'écran
        float posY = Random.Range(-canvasHeight * 0.5f, canvasHeight * 0.5f);
        
        // Décalage X (effet de corruption)
        float offsetX = Random.Range(-100f, 100f);
        
        // Hauteur variable
        float height;
        if (lightMode)
        {
            height = Random.Range(5f, 40f);
        }
        else
        {
            // Mode intense : plus de variation
            float rand = Random.value;
            if (rand < 0.3f)
            {
                height = Random.Range(2f, 10f);   // Fines lignes
            }
            else if (rand < 0.7f)
            {
                height = Random.Range(15f, 50f);  // Moyennes
            }
            else
            {
                height = Random.Range(60f, 150f); // Grosses bandes
            }
        }
        
        // Largeur (parfois plein écran, parfois partielle)
        float width;
        if (Random.value > 0.3f)
        {
            width = canvasWidth + 200; // Dépasse l'écran
        }
        else
        {
            width = Random.Range(canvasWidth * 0.3f, canvasWidth * 0.8f);
        }
        
        rect.sizeDelta = new Vector2(width, height);
        rect.anchoredPosition = new Vector2(offsetX, posY);
        
        // Couleur aléatoire
        Color[] colors = { glitchCyan, glitchMagenta, glitchGreen, glitchRed, glitchWhite, glitchBlack };
        Color bandColor = colors[Random.Range(0, colors.Length)];
        bandColor.a = Random.Range(0.4f, 0.95f);
        band.color = bandColor;
        
        band.gameObject.SetActive(true);
    }

    /// <summary>
    /// Cache l'overlay noir (appelé après le Game Over)
    /// </summary>
    public void HideBlackOverlay()
    {
        if (blackOverlay != null)
        {
            blackOverlay.DOFade(0f, 0.3f).OnComplete(() => {
                blackOverlay.gameObject.SetActive(false);
                glitchContainer.gameObject.SetActive(false);
            });
        }
    }

    /// <summary>
    /// Reset complet
    /// </summary>
    public void StopGlitch()
    {
        StopAllCoroutines();
        
        foreach (var band in glitchBands)
        {
            band.gameObject.SetActive(false);
        }
        
        if (blackOverlay != null)
        {
            blackOverlay.color = new Color(0, 0, 0, 0);
            blackOverlay.gameObject.SetActive(false);
        }
        
        if (glitchContainer != null)
        {
            glitchContainer.anchoredPosition = Vector2.zero;
            glitchContainer.gameObject.SetActive(false);
        }
        
        isPlaying = false;
    }
}