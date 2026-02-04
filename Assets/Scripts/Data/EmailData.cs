using System;

/// <summary>
/// Structure de données pour un email.
/// Chargé depuis le fichier JSON emails.json
/// </summary>
[Serializable]
public class EmailData
{
    public string expediteurNom;
    public string expediteurEmail;
    public string objet;
    public string corpsDuMessage;
    public bool estFrauduleux;
    public string explicationErreur;
    public int pointsSiCorrect;
    public int degatsIntegrite;
}