using UnityEngine;

/// <summary>
/// Structure de données pour un email.
/// Ce ScriptableObject permet de créer des emails directement dans Unity sans coder.
/// </summary>
[CreateAssetMenu(fileName = "NouvelEmail", menuName = "FirewallProtocol/Email")]
public class EmailData : ScriptableObject
{
    [Header("📧 Informations de l'Email")]
    [Tooltip("Nom affiché de l'expéditeur (ex: 'Service Client')")]
    public string expediteurNom;

    [Tooltip("Adresse email complète (ex: 'support@amazon.com')")]
    public string expediteurEmail;

    [Tooltip("Sujet de l'email")]
    public string objet;

    [Tooltip("Contenu du message")]
    [TextArea(5, 15)]
    public string corpsDuMessage;

    [Header("🎮 Logique du Jeu")]
    [Tooltip("Cochez si cet email est une arnaque/virus")]
    public bool estFrauduleux;

    [Tooltip("Message affiché si le joueur se trompe")]
    [TextArea(3, 5)]
    public string explicationErreur;

    [Header("📊 Scoring")]
    [Tooltip("Points gagnés si bonne réponse")]
    public int pointsSiCorrect = 10;

    [Tooltip("Points de vie perdus si mauvaise réponse")]
    public int degatsIntegrite = 20;
}