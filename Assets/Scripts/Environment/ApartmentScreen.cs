using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;

/// <summary>
/// Gère l'écran de l'appartement avec les zones cliquables.
/// Vue de dessus du chez-soi du joueur.
/// </summary>
public class ApartmentScreen : MonoBehaviour
{
    public static ApartmentScreen Instance;

    [Header("Panels")]
    public GameObject apartmentPanel;
    public CanvasGroup canvasGroup;

    [Header("Zones Cliquables")]
    public ApartmentZone computerZone;  // Ouvre la boutique
    public ApartmentZone bedZone;       // Dormir = jour suivant
    public ApartmentZone doorZone;      // Aller au travail (lancer le jeu)

    [Header("UI Elements")]
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI dayText;

    [Header("Références")]
    public ShopUI shopUI;

    [Header("Animation")]
    public float transitionDuration = 0.5f;

    [Header("Dialogue Lit")]
    public GameObject sleepDialogPanel;
    public Button sleepYesButton;
    public Button sleepNoButton;

    [Header("Tutoriel (première visite)")]
    public GameObject tutorialPanel;
    public TextMeshProUGUI tutorialText;
    public Button tutorialNextButton;
    public Button tutorialSkipButton;
    public Image tutorialHighlight;  // Highlight autour de la zone concernée

    // Étapes du tutoriel
    private int tutorialStep = 0;
    private readonly string[] tutorialMessages = new string[]
    {
        "<b>Bienvenue chez toi !</b>\n\nC'est ici que tu te reposes entre tes journées de travail.",
        "<b>L'ordinateur</b>\n\nClique dessus pour accéder à la boutique et améliorer tes compétences.",
        "<b>Le lit</b>\n\nDors pour passer au jour suivant et retourner au travail.",
        "<b>La porte</b>\n\nClique dessus pour retourner au menu principal."
    };

    private const string TUTORIAL_SEEN_KEY = "ApartmentTutorialSeen";

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
        // Cache tout au démarrage
        if (apartmentPanel != null)
            apartmentPanel.SetActive(false);

        if (sleepDialogPanel != null)
            sleepDialogPanel.SetActive(false);

        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);

        // Configure les boutons du dialogue
        if (sleepYesButton != null)
            sleepYesButton.onClick.AddListener(OnSleepConfirmed);
        if (sleepNoButton != null)
            sleepNoButton.onClick.AddListener(CloseSleepDialog);

        // Configure les boutons du tutoriel
        if (tutorialNextButton != null)
            tutorialNextButton.onClick.AddListener(NextTutorialStep);
        if (tutorialSkipButton != null)
            tutorialSkipButton.onClick.AddListener(SkipTutorial);

        // Configure les zones
        SetupZones();
    }

    void SetupZones()
    {
        // Ordinateur -> Boutique
        if (computerZone != null)
        {
            computerZone.Setup(
                "Ordinateur",
                "Accéder à la boutique",
                OnComputerClicked,
                null,  // Pas de hover sur mobile
                null
            );
        }

        // Lit -> Dormir
        if (bedZone != null)
        {
            bedZone.Setup(
                "Lit",
                "Se reposer et passer au jour suivant",
                OnBedClicked,
                null,
                null
            );
        }

        // Porte -> Aller au travail
        if (doorZone != null)
        {
            doorZone.Setup(
                "Porte",
                "Aller au travail",
                OnDoorClicked,
                null,
                null
            );
        }
    }

    /// <summary>
    /// Ouvre l'écran appartement avec animation
    /// </summary>
    public void Open()
    {
        if (apartmentPanel == null) return;

        apartmentPanel.SetActive(true);
        UpdateUI();

        // Animation d'entrée
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, transitionDuration).SetEase(Ease.OutQuad);
        }

        // Anime les zones
        AnimateZonesEntry();

        // Affiche le tutoriel si première visite
        if (!HasSeenTutorial())
        {
            StartCoroutine(ShowTutorialDelayed());
        }
    }

    IEnumerator ShowTutorialDelayed()
    {
        yield return new WaitForSeconds(0.8f);
        StartTutorial();
    }

    /// <summary>
    /// Ferme l'écran appartement
    /// </summary>
    public void Close(System.Action onComplete = null)
    {
        if (canvasGroup != null)
        {
            canvasGroup.DOFade(0f, transitionDuration).SetEase(Ease.InQuad)
                .OnComplete(() =>
                {
                    apartmentPanel.SetActive(false);
                    onComplete?.Invoke();
                });
        }
        else
        {
            apartmentPanel.SetActive(false);
            onComplete?.Invoke();
        }
    }

    void AnimateZonesEntry()
    {
        float delay = 0.2f;

        if (computerZone != null)
            computerZone.AnimateEntry(delay);
        if (bedZone != null)
            bedZone.AnimateEntry(delay + 0.1f);
        if (doorZone != null)
            doorZone.AnimateEntry(delay + 0.2f);
    }

    void UpdateUI()
    {
        PlayerProgress progress = PlayerProgress.Instance;

        if (coinsText != null)
            coinsText.text = progress.coins.ToString();

        if (dayText != null)
        {
            // On affiche le jour qu'on vient de terminer (currentDay a déjà été incrémenté)
            int jourAffiche = Mathf.Max(1, progress.currentDay - 1);
            DayConfig config = PlayerProgress.GetDayConfig(jourAffiche);
            dayText.text = config.dayName;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // GESTION DES ZONES
    // ═══════════════════════════════════════════════════════════

    void OnComputerClicked()
    {
        Debug.Log("Ordinateur cliqué - Ouverture boutique");

        if (shopUI != null)
        {
            shopUI.Ouvrir();
        }
    }

    void OnBedClicked()
    {
        Debug.Log("Lit cliqué - Dialogue sommeil");
        ShowSleepDialog();
    }

    void OnDoorClicked()
    {
        Debug.Log("Porte cliquée - Retour au menu");

        // Ferme l'appartement et retourne au menu principal
        Close(() =>
        {
            if (MainMenu.Instance != null)
            {
                MainMenu.Instance.Show();
            }
        });
    }

    // ═══════════════════════════════════════════════════════════
    // TUTORIEL
    // ═══════════════════════════════════════════════════════════

    bool HasSeenTutorial()
    {
        return PlayerPrefs.GetInt(TUTORIAL_SEEN_KEY, 0) == 1;
    }

    void MarkTutorialAsSeen()
    {
        PlayerPrefs.SetInt(TUTORIAL_SEEN_KEY, 1);
        PlayerPrefs.Save();
    }

    void StartTutorial()
    {
        if (tutorialPanel == null) return;

        tutorialStep = 0;
        tutorialPanel.SetActive(true);
        ShowTutorialStep();

        // Animation d'entrée
        CanvasGroup cg = tutorialPanel.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 0f;
            cg.DOFade(1f, 0.3f);
        }

        RectTransform rect = tutorialPanel.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.localScale = Vector3.one * 0.8f;
            rect.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
        }
    }

    void ShowTutorialStep()
    {
        if (tutorialText != null && tutorialStep < tutorialMessages.Length)
        {
            tutorialText.text = tutorialMessages[tutorialStep];
        }

        // Highlight la zone correspondante
        HighlightZoneForStep(tutorialStep);

        // Change le texte du bouton pour la dernière étape
        if (tutorialNextButton != null)
        {
            TextMeshProUGUI btnText = tutorialNextButton.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
            {
                btnText.text = (tutorialStep >= tutorialMessages.Length - 1) ? "Compris !" : "Suivant";
            }
        }
    }

    void HighlightZoneForStep(int step)
    {
        if (tutorialHighlight == null) return;

        Transform targetZone = null;

        switch (step)
        {
            case 1: // Ordinateur
                if (computerZone != null) targetZone = computerZone.transform;
                break;
            case 2: // Lit
                if (bedZone != null) targetZone = bedZone.transform;
                break;
            case 3: // Porte
                if (doorZone != null) targetZone = doorZone.transform;
                break;
            default:
                tutorialHighlight.gameObject.SetActive(false);
                return;
        }

        if (targetZone != null)
        {
            tutorialHighlight.gameObject.SetActive(true);
            tutorialHighlight.rectTransform.position = targetZone.position;

            // Animation du highlight
            tutorialHighlight.DOKill();
            tutorialHighlight.color = new Color(1, 1, 0, 0);
            tutorialHighlight.DOFade(0.5f, 0.3f);
        }
    }

    void NextTutorialStep()
    {
        tutorialStep++;

        if (tutorialStep >= tutorialMessages.Length)
        {
            CloseTutorial();
        }
        else
        {
            ShowTutorialStep();
        }
    }

    void SkipTutorial()
    {
        CloseTutorial();
    }

    void CloseTutorial()
    {
        MarkTutorialAsSeen();

        if (tutorialHighlight != null)
            tutorialHighlight.gameObject.SetActive(false);

        if (tutorialPanel == null) return;

        RectTransform rect = tutorialPanel.GetComponent<RectTransform>();
        CanvasGroup cg = tutorialPanel.GetComponent<CanvasGroup>();

        Sequence seq = DOTween.Sequence();
        if (rect != null)
            seq.Append(rect.DOScale(0.8f, 0.2f).SetEase(Ease.InBack));
        if (cg != null)
            seq.Join(cg.DOFade(0f, 0.2f));

        seq.OnComplete(() => tutorialPanel.SetActive(false));
    }

    // ═══════════════════════════════════════════════════════════
    // DIALOGUE SOMMEIL
    // ═══════════════════════════════════════════════════════════

    void ShowSleepDialog()
    {
        if (sleepDialogPanel == null) return;

        sleepDialogPanel.SetActive(true);

        // Animation
        RectTransform rect = sleepDialogPanel.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.localScale = Vector3.one * 0.8f;
            rect.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
        }

        CanvasGroup cg = sleepDialogPanel.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 0f;
            cg.DOFade(1f, 0.3f);
        }
    }

    void CloseSleepDialog()
    {
        if (sleepDialogPanel == null) return;

        RectTransform rect = sleepDialogPanel.GetComponent<RectTransform>();
        CanvasGroup cg = sleepDialogPanel.GetComponent<CanvasGroup>();

        Sequence seq = DOTween.Sequence();
        if (rect != null)
            seq.Append(rect.DOScale(0.8f, 0.2f).SetEase(Ease.InBack));
        if (cg != null)
            seq.Join(cg.DOFade(0f, 0.2f));

        seq.OnComplete(() => sleepDialogPanel.SetActive(false));
    }

    void OnSleepConfirmed()
    {
        Debug.Log("Bonne nuit ! Passage au jour suivant...");
        CloseSleepDialog();

        // Animation de transition (fondu au noir)
        StartCoroutine(SleepTransition());
    }

    IEnumerator SleepTransition()
    {
        // Fondu au noir
        if (canvasGroup != null)
        {
            yield return canvasGroup.DOFade(0f, 0.5f).WaitForCompletion();
        }

        // Ferme l'appartement
        apartmentPanel.SetActive(false);

        // Lance le jour suivant
        if (GameManager.Instance != null)
        {
            GameManager.Instance.JourSuivant();
        }
    }
}
