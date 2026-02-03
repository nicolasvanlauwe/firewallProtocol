using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Gère la logique globale du jeu : emails, score, intégrité, progression.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Configuration")]
    [Tooltip("Utiliser le chargement JSON")]
    public bool useJSONEmails = true;

    [Tooltip("Liste manuelle des emails (si useJSONEmails = false)")]
    public List<EmailData> emailsATraiter;

    private int emailActuelIndex = 0;

    [Header("Statistiques de la partie")]
    public int integrite = 100;
    public int score = 0;
    public int coins = 0;
    public int correctAnswers = 0;
    public int wrongAnswers = 0;

    [Header("Références UI - Gameplay")]
    public EmailCardUI emailCardUI;
    public TextMeshProUGUI integriteText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI emailsRestantsText;
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI dayText;

    [Header("Popup de Feedback")]
    public FeedbackPopup feedbackPopup;

    [Header("Écrans de Fin")]
    public GameObject gameOverPanel;
    public GameObject victoryPanel;

    // État du jeu
    private bool pendingGameOver = false;
    private bool pendingVictory = false;
    private int currentDay = 1;
    private DayConfig currentDayConfig;

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
        // Cache les popups/panels et l'UI de jeu
        if (feedbackPopup != null) feedbackPopup.gameObject.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (emailCardUI != null) emailCardUI.gameObject.SetActive(false);

        // Ne lance PAS le jeu automatiquement
        // Le jeu démarre quand le joueur clique sur la porte de l'appartement
        // ou via RejouerJour() / JourSuivant()
    }

    /// <summary>
    /// Initialise une nouvelle partie basée sur le jour actuel du joueur.
    /// </summary>
    void InitialiserPartie()
    {
        // Récupère le jour actuel depuis la progression
        currentDay = PlayerProgress.Instance.currentDay;
        currentDayConfig = PlayerProgress.GetDayConfig(currentDay);

        // Initialise les stats avec la config du jour
        int integrityBonus = ShopSystem.Instance != null ? ShopSystem.Instance.GetIntegrityBonus() : 0;
        integrite = currentDayConfig.startingIntegrity + integrityBonus;
        score = 0;
        coins = 0;
        correctAnswers = 0;
        wrongAnswers = 0;
        emailActuelIndex = 0;

        // Charge les emails pour ce jour
        if (useJSONEmails && EmailLoader.Instance != null)
        {
            emailsATraiter = EmailLoader.Instance.PrepareGameForDay(currentDay);
            Debug.Log($"[GameManager] Jour {currentDay}: {emailsATraiter.Count} emails, {integrite}% intégrité");
        }

        // Affiche le premier email
        ChargerEmailSuivant();
        MettreAJourUI();
    }

    /// <summary>
    /// Charge l'email suivant ou termine la journée.
    /// </summary>
    public void ChargerEmailSuivant()
    {
        if (emailActuelIndex < emailsATraiter.Count)
        {
            emailCardUI.AfficherEmail(emailsATraiter[emailActuelIndex]);
        }
        else
        {
            FinDeJournee();
        }
    }

    /// <summary>
    /// Appelée par EmailCardUI quand le joueur swipe.
    /// </summary>
    public void TraiterDecision(bool joueurApprouve)
    {
        TraiterDecisionAvecRetour(joueurApprouve);
    }

    /// <summary>
    /// Traite la décision du joueur et retourne si c'était correct.
    /// </summary>
    public bool TraiterDecisionAvecRetour(bool joueurApprouve)
    {
        EmailData email = emailsATraiter[emailActuelIndex];

        bool decisionCorrecte = (joueurApprouve && !email.estFrauduleux) ||
                                (!joueurApprouve && email.estFrauduleux);

        if (decisionCorrecte)
        {
            // BONNE RÉPONSE
            correctAnswers++;
            PlayerProgress.Instance.UpdateStreak(true);

            // Calcul des points et coins
            score += email.pointsSiCorrect;
            int earnedCoins = CalculerCoins(email.pointsSiCorrect);
            coins += earnedCoins;

            Debug.Log($"✅ Correct! +{email.pointsSiCorrect} pts, +{earnedCoins} coins (série: {PlayerProgress.Instance.currentStreak})");

            emailActuelIndex++;
            MettreAJourUI();
            Invoke("ChargerEmailSuivant", 0.5f);
        }
        else
        {
            // MAUVAISE RÉPONSE
            wrongAnswers++;
            PlayerProgress.Instance.UpdateStreak(false);

            // Calcul des dégâts (avec réduction si upgrades)
            float damageMultiplier = ShopSystem.Instance != null ? ShopSystem.Instance.GetDamageMultiplier() : 1f;
            int damage = Mathf.RoundToInt(email.degatsIntegrite * damageMultiplier);
            integrite -= damage;

            if (integrite < 0) integrite = 0;
            Debug.Log($"❌ Erreur! -{damage} intégrité (base: {email.degatsIntegrite})");

            emailActuelIndex++;
            MettreAJourUI();

            if (integrite <= 0)
            {
                // Vérifie si le joueur a une vie supplémentaire
                if (PlayerProgress.Instance.UseExtraLife())
                {
                    integrite = 25; // Restaure 25% d'intégrité
                    MettreAJourUI();
                    Debug.Log("💚 Vie supplémentaire utilisée!");
                    AfficherFeedback(email.explicationErreur, false);
                }
                else
                {
                    AfficherFeedback(email.explicationErreur, true);
                }
            }
            else
            {
                AfficherFeedback(email.explicationErreur, false);
            }
        }

        return decisionCorrecte;
    }

    /// <summary>
    /// Calcule les coins gagnés avec les bonus.
    /// </summary>
    int CalculerCoins(int basePoints)
    {
        int streak = PlayerProgress.Instance.currentStreak;
        int earnedCoins = PlayerProgress.Instance.CalculateCoinsForCorrectAnswer(basePoints, currentDay, streak);

        // Applique le multiplicateur de la boutique
        if (ShopSystem.Instance != null)
        {
            earnedCoins = Mathf.RoundToInt(earnedCoins * ShopSystem.Instance.GetCoinMultiplier());
        }

        return earnedCoins;
    }

    /// <summary>
    /// Utilise un indice pour l'email actuel.
    /// </summary>
    public bool UtiliserIndice()
    {
        if (emailActuelIndex >= emailsATraiter.Count) return false;

        if (PlayerProgress.Instance.UseHint())
        {
            EmailData email = emailsATraiter[emailActuelIndex];
            string hint = email.estFrauduleux ? "Cet email est FRAUDULEUX" : "Cet email est LÉGITIME";
            Debug.Log($"💡 Indice: {hint}");
            // TODO: Afficher l'indice dans l'UI
            return true;
        }
        return false;
    }

    /// <summary>
    /// Passe l'email actuel sans pénalité.
    /// </summary>
    public bool PasserEmail()
    {
        if (emailActuelIndex >= emailsATraiter.Count) return false;

        if (PlayerProgress.Instance.UseSkip())
        {
            Debug.Log("⏭️ Email passé");
            emailActuelIndex++;
            MettreAJourUI();
            ChargerEmailSuivant();
            return true;
        }
        return false;
    }

    void MettreAJourUI()
    {
        if (integriteText != null) integriteText.text = integrite + "%";
        if (scoreText != null) scoreText.text = score + " pts";
        if (coinsText != null) coinsText.text = coins.ToString();
        if (dayText != null) dayText.text = currentDayConfig.dayName;

        if (emailsRestantsText != null)
        {
            int restants = emailsATraiter.Count - emailActuelIndex;
            emailsRestantsText.text = restants.ToString();
        }
    }

    void AfficherFeedback(string message, bool isGameOver)
    {
        if (feedbackPopup != null)
        {
            pendingGameOver = isGameOver;
            pendingVictory = false;
            feedbackPopup.AfficherMessage(message);
        }
    }

    public void OnPopupFermee()
    {
        if (pendingGameOver)
        {
            pendingGameOver = false;
            GameOver();
        }
        else if (pendingVictory)
        {
            pendingVictory = false;
            AfficherVictoire();
        }
        else
        {
            ChargerEmailSuivant();
        }
    }

    void FinDeJournee()
    {
        Debug.Log($"🎉 Jour {currentDay} terminé! Score: {score}, Coins: {coins}");

        // Calcule le bonus de fin de journée
        int bonus = PlayerProgress.Instance.CalculateDayCompletionBonus(
            currentDay, correctAnswers, emailsATraiter.Count, integrite);
        coins += bonus;

        Debug.Log($"🎁 Bonus de fin de journée: +{bonus} coins");

        // Sauvegarde la progression
        PlayerProgress.Instance.AddCoins(coins);
        PlayerProgress.Instance.RecordVictory(score, correctAnswers, wrongAnswers);
        PlayerProgress.Instance.AdvanceToNextDay();

        AfficherVictoire();
    }

    void AfficherVictoire()
    {
        if (EffectsManager.Instance != null)
        {
            EffectsManager.Instance.PlayVictoryEffect();
        }

        if (victoryPanel != null)
        {
            EndScreenAnimator animator = victoryPanel.GetComponent<EndScreenAnimator>();
            if (animator != null)
            {
                string title = $"JOUR {currentDay} TERMINÉ !";
                string message = $"Emails traités: {correctAnswers}/{emailsATraiter.Count}\nCoins gagnés: {coins}";
                animator.Setup(false, score, title, message);
            }
            victoryPanel.SetActive(true);
        }

        if (emailCardUI != null) emailCardUI.gameObject.SetActive(false);
    }

    void GameOver()
    {
        Debug.Log($"💀 Game Over au jour {currentDay}");

        // Sauvegarde les coins gagnés avant la défaite
        PlayerProgress.Instance.AddCoins(coins);
        PlayerProgress.Instance.RecordDefeat(score, correctAnswers, wrongAnswers);

        if (emailCardUI != null) emailCardUI.gameObject.SetActive(false);

        if (EffectsManager.Instance != null)
        {
            EffectsManager.Instance.PlayGameOverEffect(() => AfficherEcranGameOver());
        }
        else
        {
            AfficherEcranGameOver();
        }
    }

    void AfficherEcranGameOver()
    {
        if (gameOverPanel != null)
        {
            EndScreenAnimator animator = gameOverPanel.GetComponent<EndScreenAnimator>();
            if (animator != null)
            {
                string message = $"Jour {currentDay}\nCoins gagnés: {coins}";
                animator.Setup(true, score, "GAME OVER", message);
            }
            gameOverPanel.SetActive(true);
            gameOverPanel.transform.SetAsLastSibling();
        }
    }

    /// <summary>
    /// Recommence le jour actuel.
    /// </summary>
    public void RejouerJour()
    {
        if (GlitchEffect.Instance != null) GlitchEffect.Instance.StopGlitch();
        if (ConfettiEffect.Instance != null) ConfettiEffect.Instance.StopConfetti();

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (emailCardUI != null) emailCardUI.gameObject.SetActive(true);

        // Réinitialise la partie pour le même jour
        InitialiserPartie();

        Debug.Log($"🔄 Jour {currentDay} recommencé");
    }

    /// <summary>
    /// Passe au jour suivant (après une victoire).
    /// </summary>
    public void JourSuivant()
    {
        if (GlitchEffect.Instance != null) GlitchEffect.Instance.StopGlitch();
        if (ConfettiEffect.Instance != null) ConfettiEffect.Instance.StopConfetti();

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (emailCardUI != null) emailCardUI.gameObject.SetActive(true);

        // La progression est déjà sauvegardée, on initialise juste la nouvelle partie
        InitialiserPartie();

        Debug.Log($"➡️ Passage au jour {currentDay}");
    }

    /// <summary>
    /// Redémarre depuis le jour 1 (reset complet).
    /// </summary>
    public void RedemarrerPartie()
    {
        if (GlitchEffect.Instance != null) GlitchEffect.Instance.StopGlitch();
        if (ConfettiEffect.Instance != null) ConfettiEffect.Instance.StopConfetti();

        // Reset le jour à 1 (mais garde les coins/upgrades)
        PlayerProgress.Instance.currentDay = 1;
        PlayerProgress.Instance.Save();

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (emailCardUI != null) emailCardUI.gameObject.SetActive(true);

        InitialiserPartie();

        Debug.Log("🔄 Partie redémarrée depuis le jour 1");
    }
}
