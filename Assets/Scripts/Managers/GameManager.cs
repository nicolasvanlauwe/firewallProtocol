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

    private int emailActuelIndex = 0; // Quel email on affiche actuellement

    [Header("📊 Statistiques du Joueur")]
    public int integrite = 100;       // Points de vie (0 = Game Over)
    public int score = 0;             // Points accumulés

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

    [Tooltip("Titre du Game Over")]
    public TextMeshProUGUI gameOverTitle;

    [Tooltip("Message du Game Over")]
    public TextMeshProUGUI gameOverMessage;

    [Tooltip("Score affiché dans le Game Over")]
    public TextMeshProUGUI gameOverScore;

    [Tooltip("Panel affiché quand tous les emails sont traités")]
    public GameObject victoryPanel;

    [Tooltip("Titre de la victoire")]
    public TextMeshProUGUI victoryTitle;

    [Tooltip("Message de victoire")]
    public TextMeshProUGUI victoryMessage;

    [Tooltip("Score affiché dans la victoire")]
    public TextMeshProUGUI victoryScore;

    void Awake()
    {
        // Initialise le Singleton
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
        // Vérifie s'il reste des emails
        if (emailActuelIndex < emailsATraiter.Count)
        {
            // Demande à EmailCardUI d'afficher l'email
            emailCardUI.AfficherEmail(emailsATraiter[emailActuelIndex]);
        }
        else
        {
            // Plus d'emails = fin de la journée
            FinDeJournee();
        }
    }

    /// <summary>
    /// Appelée par EmailCardUI quand le joueur swipe.
    /// </summary>
    /// <param name="joueurApprouve">true = swipe droite, false = swipe gauche</param>
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
        // Récupère l'email actuel
        EmailData email = emailsATraiter[emailActuelIndex];

        // Vérifie si la décision est correcte
        // Correct = (approuver un vrai email) OU (rejeter un faux email)
        bool decisionCorrecte = (joueurApprouve && !email.estFrauduleux) ||
                                (!joueurApprouve && email.estFrauduleux);

        if (decisionCorrecte)
        {
            // ✅ BONNE RÉPONSE
            score += email.pointsSiCorrect;
            Debug.Log("✅ Bonne décision ! +" + email.pointsSiCorrect + " points");
            
            // Passe à l'email suivant
            emailActuelIndex++;
            MettreAJourUI();
            
            // Charge l'email suivant après un court délai
            Invoke("ChargerEmailSuivant", 0.5f);
        }
        else
        {
            // ❌ MAUVAISE RÉPONSE
            integrite -= email.degatsIntegrite;
            Debug.Log("❌ Erreur ! -" + email.degatsIntegrite + " intégrité");
            
            // Passe à l'email suivant (sera chargé après la popup)
            emailActuelIndex++;
            MettreAJourUI();

            // Vérifie si Game Over
            if (integrite <= 0)
            {
                integrite = 0; // Empêche les valeurs négatives
                // Affiche la popup, puis Game Over quand elle se ferme
                AfficherFeedback(email.explicationErreur, true); // true = Game Over après
            }
            else
            {
                // Affiche la popup, puis continue le jeu
                AfficherFeedback(email.explicationErreur, false);
            }
        }
        
        return decisionCorrecte;
    }

    /// <summary>
    /// Met à jour tous les textes de l'interface.
    /// Affiche uniquement les valeurs (les icônes font office de label)
    /// </summary>
    void MettreAJourUI()
    {
        // Affiche juste les valeurs, sans préfixe (les icônes sont les labels)
        integriteText.text = integrite + "%";
        scoreText.text = score + " pts";

        int emailsRestants = emailsATraiter.Count - emailActuelIndex;
        emailsRestantsText.text = emailsRestants.ToString();
    }

    /// <summary>
    /// Affiche le popup d'erreur avec un message pédagogique.
    /// Utilise le script FeedbackPopup pour adapter la taille.
    /// </summary>
    void AfficherFeedback(string message, bool isGameOver)
    {
        if (feedbackPopup != null)
        {
            // Stocke si c'est un Game Over pour après la fermeture
            pendingGameOver = isGameOver;
            pendingVictory = false;
            
            // Affiche la popup
            feedbackPopup.AfficherMessage(message);
        }
    }
    
    // Variables pour savoir quoi faire après la popup
    private bool pendingGameOver = false;
    private bool pendingVictory = false;
    
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
            // Continue le jeu normalement
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
        // Joue l'effet de victoire
        if (EffectsManager.Instance != null)
        {
            EffectsManager.Instance.PlayVictoryEffect();
        }
        
        // Affiche l'écran de victoire
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
            if (victoryTitle != null) victoryTitle.text = "JOURNÉE TERMINÉE !";
            if (victoryMessage != null) victoryMessage.text = "Vous avez protégé le réseau !";
            if (victoryScore != null) victoryScore.text = "Score final : " + score + " pts";
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

        // Joue l'effet de Game Over avec callback
        if (EffectsManager.Instance != null)
        {
            EffectsManager.Instance.PlayGameOverEffect(() => {
                // Appelé APRÈS l'effet de glitch
                AfficherEcranGameOver();
            });
        }
        else
        {
            // Pas d'effet, affiche directement
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
            gameOverPanel.SetActive(true);
            if (gameOverTitle != null) gameOverTitle.text = "GAME OVER";
            if (gameOverMessage != null) gameOverMessage.text = "Le réseau a été compromis !";
            if (gameOverScore != null) gameOverScore.text = "Score final : " + score + " pts";
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