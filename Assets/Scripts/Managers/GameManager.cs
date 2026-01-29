using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Gère la logique globale du jeu : chargement des emails, score, intégrité.
/// </summary>
public class GameManager : MonoBehaviour
{
    // Pattern Singleton : on peut accéder au GameManager depuis n'importe où
    public static GameManager Instance;

    [Header("📧 Configuration des Emails")]
    [Tooltip("Utiliser le chargement JSON (sinon utilise la liste manuelle)")]
    public bool useJSONEmails = true;

    [Tooltip("Liste manuelle des emails (si useJSONEmails = false)")]
    public List<EmailData> emailsATraiter;

    private int emailActuelIndex = 0;

    [Header("📊 Statistiques du Joueur")]
    public int integrite = 100;
    public int score = 0;

    [Header("🎨 Références UI")]
    [Tooltip("Script qui affiche l'email à l'écran")]
    public EmailCardUI emailCardUI;

    [Tooltip("Texte qui affiche l'intégrité (ex: '80%')")]
    public TextMeshProUGUI integriteText;

    [Tooltip("Texte qui affiche le score")]
    public TextMeshProUGUI scoreText;

    [Tooltip("Texte qui affiche combien d'emails restent")]
    public TextMeshProUGUI emailsRestantsText;

    [Header("💬 Popup de Feedback")]
    [Tooltip("Script FeedbackPopup attaché à la popup")]
    public FeedbackPopup feedbackPopup;

    [Header("🎮 Écrans de Fin")]
    [Tooltip("Panel affiché quand l'intégrité atteint 0")]
    public GameObject gameOverPanel;

    [Tooltip("Panel affiché quand tous les emails sont traités")]
    public GameObject victoryPanel;

    // Variables pour savoir quoi faire après la popup
    private bool pendingGameOver = false;
    private bool pendingVictory = false;

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
        // Cache le popup au démarrage
        if (feedbackPopup != null)
            feedbackPopup.gameObject.SetActive(false);

        // Cache les écrans de fin au démarrage
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (victoryPanel != null)
            victoryPanel.SetActive(false);

        // Charge les emails depuis JSON si activé
        if (useJSONEmails && EmailLoader.Instance != null)
        {
            emailsATraiter = EmailLoader.Instance.PrepareNewGame();
            Debug.Log($"📧 {emailsATraiter.Count} emails chargés depuis JSON");
        }

        // Affiche le premier email
        ChargerEmailSuivant();

        // Met à jour l'interface
        MettreAJourUI();
    }

    /// <summary>
    /// Charge l'email suivant ou termine la partie s'il n'y en a plus.
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
    /// Appelée par EmailCardUI quand le joueur swipe.
    /// Retourne true si la décision était correcte (pour les effets visuels).
    /// </summary>
    public bool TraiterDecisionAvecRetour(bool joueurApprouve)
    {
        EmailData email = emailsATraiter[emailActuelIndex];

        bool decisionCorrecte = (joueurApprouve && !email.estFrauduleux) ||
                                (!joueurApprouve && email.estFrauduleux);

        if (decisionCorrecte)
        {
            // ✅ BONNE RÉPONSE
            score += email.pointsSiCorrect;
            Debug.Log("✅ Bonne décision ! +" + email.pointsSiCorrect + " points");

            emailActuelIndex++;
            MettreAJourUI();

            Invoke("ChargerEmailSuivant", 0.5f);
        }
        else
        {
            // ❌ MAUVAISE RÉPONSE
            integrite -= email.degatsIntegrite;
            if (integrite < 0) integrite = 0; // Empêche les valeurs négatives AVANT l'UI
            Debug.Log("❌ Erreur ! -" + email.degatsIntegrite + " intégrité");

            emailActuelIndex++;
            MettreAJourUI();

            if (integrite <= 0)
            {
                AfficherFeedback(email.explicationErreur, true);
            }
            else
            {
                AfficherFeedback(email.explicationErreur, false);
            }
        }

        return decisionCorrecte;
    }

    /// <summary>
    /// Met à jour tous les textes de l'interface.
    /// </summary>
    void MettreAJourUI()
    {
        integriteText.text = integrite + "%";
        scoreText.text = score + " pts";

        int emailsRestants = emailsATraiter.Count - emailActuelIndex;
        emailsRestantsText.text = emailsRestants.ToString();
    }

    /// <summary>
    /// Affiche le popup d'erreur avec un message pédagogique.
    /// </summary>
    void AfficherFeedback(string message, bool isGameOver)
    {
        if (feedbackPopup != null)
        {
            pendingGameOver = isGameOver;
            pendingVictory = false;
            feedbackPopup.AfficherMessage(message);
        }
    }

    /// <summary>
    /// Appelée par FeedbackPopup quand le joueur clique OK.
    /// </summary>
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

    /// <summary>
    /// Appelée quand tous les emails sont traités.
    /// </summary>
    void FinDeJournee()
    {
        Debug.Log("🎉 Journée terminée ! Score final : " + score);
        AfficherVictoire();
    }

    /// <summary>
    /// Affiche l'écran de victoire.
    /// </summary>
    void AfficherVictoire()
    {
        // Joue les confettis
        if (EffectsManager.Instance != null)
        {
            EffectsManager.Instance.PlayVictoryEffect();
        }

        // Affiche l'écran de victoire
        if (victoryPanel != null)
        {
            // Configure l'animator si présent
            EndScreenAnimator animator = victoryPanel.GetComponent<EndScreenAnimator>();
            if (animator != null)
            {
                animator.Setup(false, score, "MISSION ACCOMPLIE !", "Vous avez protégé le réseau !");
            }

            victoryPanel.SetActive(true);
        }

        // Cache l'email en cours
        if (emailCardUI != null)
            emailCardUI.gameObject.SetActive(false);
    }

    /// <summary>
    /// Appelée quand l'intégrité atteint 0.
    /// </summary>
    void GameOver()
    {
        Debug.Log("💀 GAME OVER - Le réseau est compromis !");

        // Cache l'email en cours
        if (emailCardUI != null)
            emailCardUI.gameObject.SetActive(false);

        // Joue l'effet de glitch puis affiche l'écran
        if (EffectsManager.Instance != null)
        {
            EffectsManager.Instance.PlayGameOverEffect(() => {
                AfficherEcranGameOver();
            });
        }
        else
        {
            AfficherEcranGameOver();
        }
    }

    /// <summary>
    /// Affiche le panel Game Over (appelé après l'effet de glitch)
    /// </summary>
    void AfficherEcranGameOver()
    {
        if (gameOverPanel != null)
        {
            // Configure l'animator si présent
            EndScreenAnimator animator = gameOverPanel.GetComponent<EndScreenAnimator>();
            if (animator != null)
            {
                animator.Setup(true, score, "GAME OVER", "Le réseau a été compromis !");
            }

            gameOverPanel.SetActive(true);
            gameOverPanel.transform.SetAsLastSibling();
        }
    }

    /// <summary>
    /// Redémarre la partie (appelée par les boutons Rejouer)
    /// </summary>
    public void RedemarrerPartie()
    {
        // Reset les effets visuels
        if (GlitchEffect.Instance != null)
        {
            GlitchEffect.Instance.StopGlitch();
        }

        if (ConfettiEffect.Instance != null)
        {
            ConfettiEffect.Instance.StopConfetti();
        }

        // Réinitialise les stats
        integrite = 100;
        score = 0;
        emailActuelIndex = 0;

        // Cache les écrans de fin
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);

        // Recharge des emails aléatoires depuis JSON
        if (useJSONEmails && EmailLoader.Instance != null)
        {
            emailsATraiter = EmailLoader.Instance.PrepareNewGame();
            Debug.Log($"📧 Nouvelle partie : {emailsATraiter.Count} emails chargés");
        }

        // Réaffiche la carte email
        if (emailCardUI != null)
            emailCardUI.gameObject.SetActive(true);

        // Recharge le premier email
        ChargerEmailSuivant();
        MettreAJourUI();

        Debug.Log("🔄 Partie redémarrée !");
    }
}
