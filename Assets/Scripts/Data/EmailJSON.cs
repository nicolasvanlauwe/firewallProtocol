using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Représente un email chargé depuis JSON.
/// Version simplifiée sans ScriptableObject.
/// </summary>
[Serializable]
public class EmailJSON
{
    public string expediteurNom;
    public string expediteurEmail;
    public string objet;
    [TextArea(3, 10)]
    public string corpsDuMessage;
    public bool estFrauduleux;
    public string explicationErreur;
    public int pointsSiCorrect;
    public int degatsIntegrite;
    public string categorie;        // "phishing", "spam", "legitime", "urgence"
    public string difficulte;       // "facile", "moyen", "difficile"
    
    /// <summary>
    /// Convertit en EmailData (ScriptableObject) pour compatibilité
    /// </summary>
    public EmailData ToEmailData()
    {
        EmailData data = ScriptableObject.CreateInstance<EmailData>();
        data.expediteurNom = expediteurNom;
        data.expediteurEmail = expediteurEmail;
        data.objet = objet;
        data.corpsDuMessage = corpsDuMessage;
        data.estFrauduleux = estFrauduleux;
        data.explicationErreur = explicationErreur;
        data.pointsSiCorrect = pointsSiCorrect;
        data.degatsIntegrite = degatsIntegrite;
        return data;
    }
}

/// <summary>
/// Container pour la liste d'emails (nécessaire pour JsonUtility)
/// </summary>
[Serializable]
public class EmailDatabase
{
    public List<EmailJSON> emails;
    
    /// <summary>
    /// Charge la base de données depuis un fichier JSON
    /// </summary>
    public static EmailDatabase LoadFromJSON(string jsonContent)
    {
        return JsonUtility.FromJson<EmailDatabase>(jsonContent);
    }
    
    /// <summary>
    /// Sauvegarde la base de données en JSON
    /// </summary>
    public string ToJSON()
    {
        return JsonUtility.ToJson(this, true);
    }
    
    /// <summary>
    /// Récupère des emails aléatoires selon la difficulté
    /// </summary>
    public List<EmailJSON> GetRandomEmails(int count, string difficulte = null)
    {
        List<EmailJSON> filtered = emails;
        
        // Filtre par difficulté si spécifiée
        if (!string.IsNullOrEmpty(difficulte))
        {
            filtered = emails.FindAll(e => e.difficulte == difficulte);
        }
        
        // Mélange et prend les N premiers
        List<EmailJSON> shuffled = new List<EmailJSON>(filtered);
        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            EmailJSON temp = shuffled[i];
            shuffled[i] = shuffled[j];
            shuffled[j] = temp;
        }
        
        return shuffled.GetRange(0, Mathf.Min(count, shuffled.Count));
    }
    
    /// <summary>
    /// Récupère des emails par catégorie
    /// </summary>
    public List<EmailJSON> GetEmailsByCategory(string categorie)
    {
        return emails.FindAll(e => e.categorie == categorie);
    }

    /// <summary>
    /// Récupère des emails par difficulté
    /// </summary>
    public List<EmailJSON> GetEmailsByDifficulty(string difficulte)
    {
        return emails.FindAll(e => e.difficulte == difficulte);
    }
}
