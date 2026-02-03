using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;

/// <summary>
/// Anime le panel Game Over / Victory avec des effets stylés.
/// Utilise DOTween pour les animations.
/// </summary>
public class EndScreenAnimator : MonoBehaviour
{
    [Header("Références UI")]
    public CanvasGroup panelCanvasGroup;
    public RectTransform panelRect;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI messageText;
    public TextMeshProUGUI scoreText;
    
    [Header("Boutons")]
    public Button replayButton;
    public RectTransform replayButtonRect;
    public Button goHomeButton;          // Nouveau: "Rentrer du travail"
    public RectTransform goHomeButtonRect;

    [Header("Configuration")]
    public bool isGameOver = true;
    public float animationDuration = 0.5f;
    
    [Header("Couleurs")]
    public Color gameOverColor = new Color(0.9f, 0.2f, 0.2f);
    public Color victoryColor = new Color(0.2f, 0.8f, 0.3f);

    private int targetScore = 0;

    void Start()
    {
        // Configure les boutons
        if (replayButton != null)
        {
            replayButton.onClick.AddListener(OnReplayClicked);
        }
        
        if (goHomeButton != null)
        {
            goHomeButton.onClick.AddListener(OnGoHomeClicked);
        }
    }

    void OnEnable()
    {
        // Lance l'animation quand le panel s'active
        StartCoroutine(PlayEntryAnimation());
    }

    IEnumerator PlayEntryAnimation()
    {
        // Reset tout
        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = 0f;
        }
        
        if (panelRect != null)
        {
            panelRect.localScale = Vector3.one * 0.8f;
        }

        if (titleText != null)
        {
            titleText.alpha = 0f;
            titleText.color = isGameOver ? gameOverColor : victoryColor;
        }

        if (messageText != null)
        {
            messageText.alpha = 0f;
        }

        if (scoreText != null)
        {
            scoreText.alpha = 0f;
        }

        if (replayButtonRect != null)
        {
            replayButtonRect.localScale = Vector3.zero;
        }
        
        if (goHomeButtonRect != null)
        {
            goHomeButtonRect.localScale = Vector3.zero;
            // Cache le bouton "Rentrer" si c'est un Game Over
            goHomeButtonRect.gameObject.SetActive(!isGameOver);
        }

        yield return new WaitForSeconds(0.1f);

        // 1. Fade in du panel avec scale
        Sequence panelSeq = DOTween.Sequence();
        if (panelCanvasGroup != null)
        {
            panelSeq.Append(panelCanvasGroup.DOFade(1f, animationDuration));
        }
        if (panelRect != null)
        {
            panelSeq.Join(panelRect.DOScale(1f, animationDuration).SetEase(Ease.OutBack));
        }
        
        yield return panelSeq.WaitForCompletion();

        // 2. Titre avec effet de glitch/shake
        if (titleText != null)
        {
            titleText.DOFade(1f, 0.3f);
            
            if (isGameOver)
            {
                // Effet glitch pour game over
                titleText.rectTransform.DOShakeAnchorPos(0.5f, 10f, 20, 90, false, true);
            }
            else
            {
                // Bounce pour victoire
                titleText.rectTransform.DOScale(1.1f, 0.2f).SetEase(Ease.OutQuad)
                    .OnComplete(() => titleText.rectTransform.DOScale(1f, 0.2f).SetEase(Ease.OutBounce));
            }
        }

        yield return new WaitForSeconds(0.4f);

        // 3. Message
        if (messageText != null)
        {
            messageText.DOFade(1f, 0.3f);
        }

        yield return new WaitForSeconds(0.3f);

        // 4. Score avec compteur qui défile
        if (scoreText != null)
        {
            scoreText.DOFade(1f, 0.2f);
            AnimateScoreCounter();
        }

        yield return new WaitForSeconds(0.5f);

        // 5. Boutons avec bounce
        if (replayButtonRect != null)
        {
            replayButtonRect.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
        }
        
        // Bouton "Rentrer du travail" (uniquement en victoire)
        if (goHomeButtonRect != null && !isGameOver)
        {
            yield return new WaitForSeconds(0.1f);
            goHomeButtonRect.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
        }
    }

    void AnimateScoreCounter()
    {
        // Anime le score de 0 jusqu'à la valeur finale
        int currentScore = 0;
        DOTween.To(() => currentScore, x => {
            currentScore = x;
            scoreText.text = "Score : " + currentScore + " pts";
        }, targetScore, 1f).SetEase(Ease.OutQuad);
    }

    /// <summary>
    /// Configure le panel avant de l'afficher
    /// </summary>
    public void Setup(bool gameOver, int score, string title, string message)
    {
        isGameOver = gameOver;
        targetScore = score;
        
        if (titleText != null)
            titleText.text = title;
        
        if (messageText != null)
            messageText.text = message;
        
        if (scoreText != null)
            scoreText.text = "Score : 0 pts";
            
        // Affiche/cache le bouton "Rentrer du travail"
        if (goHomeButtonRect != null)
        {
            goHomeButtonRect.gameObject.SetActive(!gameOver);
        }
    }

    void OnReplayClicked()
    {
        PlayExitAnimation(() =>
        {
            if (isGameOver)
            {
                // Game Over → retour au menu principal
                if (MainMenu.Instance != null)
                {
                    MainMenu.Instance.Show();
                }
            }
            else
            {
                // Victoire → retour au menu principal (bouton "Quitter")
                if (MainMenu.Instance != null)
                {
                    MainMenu.Instance.Show();
                }
            }
        });
    }

    void OnGoHomeClicked()
    {
        PlayExitAnimation(() =>
        {
            // Ouvre l'écran appartement
            if (ApartmentScreen.Instance != null)
            {
                ApartmentScreen.Instance.Open();
            }
        });
    }

    /// <summary>
    /// Animation de sortie
    /// </summary>
    public void PlayExitAnimation(System.Action onComplete = null)
    {
        Sequence seq = DOTween.Sequence();
        
        if (panelRect != null)
        {
            seq.Append(panelRect.DOScale(0.8f, 0.3f).SetEase(Ease.InBack));
        }
        
        if (panelCanvasGroup != null)
        {
            seq.Join(panelCanvasGroup.DOFade(0f, 0.3f));
        }
        
        seq.OnComplete(() => {
            gameObject.SetActive(false);
            onComplete?.Invoke();
        });
    }

    void OnDisable()
    {
        // Kill toutes les animations quand désactivé
        DOTween.Kill(panelRect);
        DOTween.Kill(panelCanvasGroup);
        DOTween.Kill(titleText?.rectTransform);
        DOTween.Kill(scoreText);
        DOTween.Kill(replayButtonRect);
        DOTween.Kill(goHomeButtonRect);
    }
}