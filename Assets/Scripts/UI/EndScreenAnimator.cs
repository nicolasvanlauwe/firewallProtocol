using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;

/// <summary>
/// Anime le panel Game Over / Victory avec des effets stylés.
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
    public Button replayButton;              // Game Over: "Recommencer" | Victoire: "Rentrer du travail"
    public RectTransform replayButtonRect;
    public Button quitButton;                // "Quitter" (retour menu)
    public RectTransform quitButtonRect;

    [Header("Configuration")]
    public bool isGameOver = true;
    public float animationDuration = 0.5f;

    [Header("Couleurs")]
    public Color gameOverColor = new Color(0.9f, 0.2f, 0.2f);
    public Color victoryColor = new Color(0.2f, 0.8f, 0.3f);

    private int targetScore = 0;

    void Start()
    {
        if (replayButton != null)
            replayButton.onClick.AddListener(OnReplayClicked);
        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);
    }

    void OnEnable()
    {
        StartCoroutine(PlayEntryAnimation());
    }

    IEnumerator PlayEntryAnimation()
    {
        // Reset tout
        if (panelCanvasGroup != null)
            panelCanvasGroup.alpha = 0f;

        if (panelRect != null)
            panelRect.localScale = Vector3.one * 0.8f;

        if (titleText != null)
        {
            titleText.alpha = 0f;
            titleText.color = isGameOver ? gameOverColor : victoryColor;
        }

        if (messageText != null)
            messageText.alpha = 0f;

        if (scoreText != null)
            scoreText.alpha = 0f;

        if (replayButtonRect != null)
            replayButtonRect.localScale = Vector3.zero;

        if (quitButtonRect != null)
            quitButtonRect.localScale = Vector3.zero;

        yield return new WaitForSeconds(0.1f);

        // 1. Fade in du panel avec scale
        Sequence panelSeq = DOTween.Sequence();
        if (panelCanvasGroup != null)
            panelSeq.Append(panelCanvasGroup.DOFade(1f, animationDuration));
        if (panelRect != null)
            panelSeq.Join(panelRect.DOScale(1f, animationDuration).SetEase(Ease.OutBack));

        yield return panelSeq.WaitForCompletion();

        // 2. Titre avec effet
        if (titleText != null)
        {
            titleText.DOFade(1f, 0.3f);

            if (isGameOver)
                titleText.rectTransform.DOShakeAnchorPos(0.5f, 10f, 20, 90, false, true);
            else
                titleText.rectTransform.DOScale(1.1f, 0.2f).SetEase(Ease.OutQuad)
                    .OnComplete(() => titleText.rectTransform.DOScale(1f, 0.2f).SetEase(Ease.OutBounce));
        }

        yield return new WaitForSeconds(0.4f);

        // 3. Message
        if (messageText != null)
            messageText.DOFade(1f, 0.3f);

        yield return new WaitForSeconds(0.3f);

        // 4. Score avec compteur
        if (scoreText != null)
        {
            scoreText.DOFade(1f, 0.2f);
            AnimateScoreCounter();
        }

        yield return new WaitForSeconds(0.5f);

        // 5. Boutons avec bounce
        if (replayButtonRect != null)
            replayButtonRect.DOScale(1f, 0.4f).SetEase(Ease.OutBack);

        if (quitButtonRect != null)
        {
            yield return new WaitForSeconds(0.1f);
            quitButtonRect.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
        }
    }

    void AnimateScoreCounter()
    {
        int currentScore = 0;
        DOTween.To(() => currentScore, x => {
            currentScore = x;
            scoreText.text = "Score : " + currentScore + " pts";
        }, targetScore, 1f).SetEase(Ease.OutQuad);
    }

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
    }

    /// <summary>
    /// Bouton principal :
    /// - Game Over → Recommencer (reset + jeu direct)
    /// - Victoire → Rentrer du travail (appartement sans reset)
    /// </summary>
    void OnReplayClicked()
    {
        PlayExitAnimation(() =>
        {
            if (GameManager.Instance == null) return;

            if (isGameOver)
            {
                // Game Over : reset et relance directement en jeu
                GameManager.Instance.RedemarrerPartie();
            }
            else
            {
                // Victoire : retour à l'appartement (continue la progression)
                GameManager.Instance.RetourMenu();
            }
        });
    }

    /// <summary>
    /// Bouton Quitter :
    /// - Game Over → Menu principal avec reset
    /// - Victoire → Menu principal sans reset
    /// </summary>
    void OnQuitClicked()
    {
        PlayExitAnimation(() =>
        {
            if (GameManager.Instance == null) return;

            if (isGameOver)
            {
                // Game Over : reset + menu principal
                GameManager.Instance.RetourMenuAvecReset();
            }
            else
            {
                // Victoire : menu principal sans reset
                GameManager.Instance.RetourMenuSansReset();
            }
        });
    }

    public void PlayExitAnimation(System.Action onComplete = null)
    {
        Sequence seq = DOTween.Sequence();

        if (panelRect != null)
            seq.Append(panelRect.DOScale(0.8f, 0.3f).SetEase(Ease.InBack));

        if (panelCanvasGroup != null)
            seq.Join(panelCanvasGroup.DOFade(0f, 0.3f));

        seq.OnComplete(() => {
            gameObject.SetActive(false);
            onComplete?.Invoke();
        });
    }

    void OnDisable()
    {
        DOTween.Kill(panelRect);
        DOTween.Kill(panelCanvasGroup);
        DOTween.Kill(titleText?.rectTransform);
        DOTween.Kill(scoreText);
        DOTween.Kill(replayButtonRect);
        DOTween.Kill(quitButtonRect);
    }
}
