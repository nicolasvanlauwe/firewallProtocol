using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/// <summary>
/// Menu principal affiché au lancement du jeu.
/// </summary>
public class MainMenu : MonoBehaviour
{
    public static MainMenu Instance;

    [Header("Panels")]
    public GameObject menuPanel;
    public CanvasGroup canvasGroup;

    [Header("Boutons")]
    public Button continueButton;      // Nouvelle partie / Continuer
    public Button playButton;          // Recommencer (si partie en cours)
    public Button leaderboardButton;   // Classement en ligne

    [Header("Textes")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI dayText;
    public TextMeshProUGUI pseudoText;

    [Header("Leaderboard")]
    public LeaderboardUI leaderboardUI;
    public GameObject pseudoPanel;
    public TMP_InputField pseudoInput;
    public Button pseudoConfirmButton;

    [Header("Animation")]
    public RectTransform logoRect;
    public float animationDuration = 0.5f;

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
        // Configure les boutons
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueClicked);
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayClicked);
        if (leaderboardButton != null)
            leaderboardButton.onClick.AddListener(OnLeaderboardClicked);
        if (pseudoConfirmButton != null)
            pseudoConfirmButton.onClick.AddListener(OnPseudoConfirmed);

        if (pseudoPanel != null)
            pseudoPanel.SetActive(false);

        // Affiche le menu au démarrage
        Show();
    }

    /// <summary>
    /// Affiche le menu principal avec animation
    /// </summary>
    public void Show()
    {
        if (menuPanel == null) return;

        menuPanel.SetActive(true);
        UpdateUI();

        // Animation d'entrée
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, animationDuration).SetEase(Ease.OutQuad);
        }

        // Animation du logo
        if (logoRect != null)
        {
            logoRect.localScale = Vector3.zero;
            logoRect.DOScale(1f, 0.6f).SetEase(Ease.OutBack).SetDelay(0.2f);
        }

        // Animation des boutons
        AnimateButtons();
    }

    void AnimateButtons()
    {
        float delay = 0.4f;
        float interval = 0.1f;

        Button[] buttons = { continueButton, playButton, leaderboardButton };
        foreach (var btn in buttons)
        {
            if (btn != null && btn.gameObject.activeSelf)
            {
                RectTransform rect = btn.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.localScale = Vector3.zero;
                    rect.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetDelay(delay);
                    delay += interval;
                }
            }
        }
    }

    void UpdateUI()
    {
        PlayerProgress progress = PlayerProgress.Instance;
        if (progress == null) return;

        if (coinsText != null)
            coinsText.text = progress.coins + " cryptos";

        if (dayText != null)
        {
            DayConfig config = PlayerProgress.GetDayConfig(progress.currentDay);
            dayText.text = config.dayName;
        }

        // Affiche le pseudo si le joueur en a un
        if (pseudoText != null)
        {
            if (LeaderboardManager.Instance != null && LeaderboardManager.Instance.HasPseudo())
            {
                pseudoText.text = "Connecté : " + LeaderboardManager.Instance.GetPseudo();
                pseudoText.gameObject.SetActive(true);
            }
            else
            {
                pseudoText.gameObject.SetActive(false);
            }
        }

        // Vérifie si le joueur a progressé au-delà du jour 1
        bool hasProgress = progress.currentDay > 1;

        // Change le texte du bouton Continuer selon l'état
        if (continueButton != null)
        {
            TextMeshProUGUI btnText = continueButton.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
            {
                btnText.text = hasProgress ? "Continuer" : "Nouvelle partie";
            }
        }

        // Affiche le bouton Recommencer seulement si partie en cours
        if (playButton != null)
        {
            playButton.gameObject.SetActive(hasProgress);
        }
    }

    /// <summary>
    /// Ferme le menu avec animation
    /// </summary>
    public void Hide(System.Action onComplete = null)
    {
        if (canvasGroup != null)
        {
            canvasGroup.DOFade(0f, animationDuration * 0.5f).SetEase(Ease.InQuad)
                .OnComplete(() =>
                {
                    menuPanel.SetActive(false);
                    onComplete?.Invoke();
                });
        }
        else
        {
            menuPanel.SetActive(false);
            onComplete?.Invoke();
        }
    }

    // ═══════════════════════════════════════════════════════════
    // BOUTONS
    // ═══════════════════════════════════════════════════════════

    void OnContinueClicked()
    {
        PlayerProgress progress = PlayerProgress.Instance;
        bool hasProgress = progress.currentDay > 1;

        if (hasProgress)
        {
            // Partie en cours → va à l'appartement (ou lance le jeu si pas d'appartement)
            Debug.Log("Continuer - Vers l'appartement");
            Hide(() =>
            {
                if (ApartmentScreen.Instance != null)
                {
                    ApartmentScreen.Instance.Open();
                }
                else
                {
                    // Fallback: lance le jeu directement
                    Debug.LogWarning("ApartmentScreen non trouvé, lancement du jeu directement");
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.RejouerJour();
                    }
                }
            });
        }
        else
        {
            // Nouvelle partie → lance le jeu directement
            Debug.Log("Nouvelle partie - Lancement du jeu");
            Hide(() =>
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.RejouerJour();
                }
            });
        }
    }

    void OnPlayClicked()
    {
        Debug.Log("Recommencer - Retour au jour 1");

        Hide(() =>
        {
            // Recommence tout depuis le jour 1
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RedemarrerPartie();
            }
        });
    }

    // ═══════════════════════════════════════════════════════════
    // LEADERBOARD
    // ═══════════════════════════════════════════════════════════

    void OnLeaderboardClicked()
    {
        // Ferme le pseudoPanel s'il est ouvert
        if (pseudoPanel != null && pseudoPanel.activeSelf)
        {
            pseudoPanel.SetActive(false);
        }

        // Demande un pseudo si le joueur n'en a pas encore
        if (LeaderboardManager.Instance != null && !LeaderboardManager.Instance.HasPseudo())
        {
            ShowPseudoPanel();
            return;
        }

        OuvrirLeaderboard();
    }

    void OuvrirLeaderboard()
    {
        if (leaderboardUI != null)
        {
            leaderboardUI.Ouvrir();
        }
    }

    void ShowPseudoPanel()
    {
        if (pseudoPanel != null)
        {
            pseudoPanel.SetActive(true);
            if (pseudoInput != null)
                pseudoInput.text = "";
        }
    }

    void OnPseudoConfirmed()
    {
        if (pseudoInput == null) return;

        string pseudo = pseudoInput.text.Trim();
        if (string.IsNullOrEmpty(pseudo)) return;

        if (LeaderboardManager.Instance != null)
        {
            LeaderboardManager.Instance.SetPseudo(pseudo);
        }

        if (pseudoPanel != null)
            pseudoPanel.SetActive(false);

        // Met à jour l'affichage du pseudo dans le menu
        UpdateUI();

        // Ouvre le leaderboard après avoir défini le pseudo
        OuvrirLeaderboard();
    }
}
