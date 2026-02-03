using UnityEngine;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Charge les emails depuis un fichier JSON.
/// Supporte le système de niveaux avec difficulté progressive.
/// </summary>
public class EmailLoader : MonoBehaviour
{
    public static EmailLoader Instance;

    [Header("Configuration")]
    [Tooltip("Nom du fichier JSON dans Resources (sans extension)")]
    public string jsonFileName = "emails";

    [Tooltip("Charger automatiquement au démarrage")]
    public bool loadOnStart = true;

    [Header("Debug")]
    [Tooltip("Afficher les logs de chargement")]
    public bool debugMode = true;

    // Base de données chargée
    private EmailDatabase database;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        if (loadOnStart)
        {
            LoadEmails();
        }
    }

    /// <summary>
    /// Charge les emails depuis le fichier JSON dans Resources.
    /// </summary>
    public void LoadEmails()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>(jsonFileName);

        if (jsonFile == null)
        {
            Debug.LogError($"Fichier JSON non trouvé : Resources/{jsonFileName}.json");
            return;
        }

        database = EmailDatabase.LoadFromJSON(jsonFile.text);

        if (debugMode)
        {
            Debug.Log($"[EmailLoader] {database.emails.Count} emails chargés");
            LogEmailStats();
        }
    }

    void LogEmailStats()
    {
        int facile = database.emails.FindAll(e => e.difficulte == "facile").Count;
        int moyen = database.emails.FindAll(e => e.difficulte == "moyen").Count;
        int difficile = database.emails.FindAll(e => e.difficulte == "difficile").Count;
        int expert = database.emails.FindAll(e => e.difficulte == "expert").Count;

        Debug.Log($"[EmailLoader] Répartition: {facile} facile, {moyen} moyen, {difficile} difficile, {expert} expert");
    }

    /// <summary>
    /// Prépare une nouvelle partie basée sur le jour actuel.
    /// </summary>
    public List<EmailData> PrepareNewGame()
    {
        return PrepareGameForDay(PlayerProgress.Instance.currentDay);
    }

    /// <summary>
    /// Prépare une partie pour un jour spécifique.
    /// </summary>
    public List<EmailData> PrepareGameForDay(int day)
    {
        if (database == null)
        {
            Debug.LogError("Base de données non chargée !");
            return new List<EmailData>();
        }

        DayConfig config = PlayerProgress.GetDayConfig(day);
        List<EmailJSON> selectedEmails = SelectEmailsForDay(config);

        // Convertit en EmailData
        List<EmailData> emailDataList = new List<EmailData>();
        foreach (var email in selectedEmails)
        {
            emailDataList.Add(email.ToEmailData());
        }

        if (debugMode)
        {
            Debug.Log($"[EmailLoader] Jour {day} ({config.dayName}): {emailDataList.Count} emails préparés");
        }

        return emailDataList;
    }

    /// <summary>
    /// Sélectionne les emails selon la configuration du jour.
    /// </summary>
    List<EmailJSON> SelectEmailsForDay(DayConfig config)
    {
        List<EmailJSON> result = new List<EmailJSON>();

        // Récupère les emails par difficulté
        List<EmailJSON> easyEmails = Shuffle(database.GetEmailsByDifficulty("facile"));
        List<EmailJSON> mediumEmails = Shuffle(database.GetEmailsByDifficulty("moyen"));
        List<EmailJSON> hardEmails = Shuffle(database.GetEmailsByDifficulty("difficile"));
        List<EmailJSON> expertEmails = Shuffle(database.GetEmailsByDifficulty("expert"));

        // Calcule le nombre d'emails de chaque difficulté
        int easyCount = Mathf.RoundToInt(config.emailCount * config.easyPercent / 100f);
        int mediumCount = Mathf.RoundToInt(config.emailCount * config.mediumPercent / 100f);
        int hardCount = Mathf.RoundToInt(config.emailCount * config.hardPercent / 100f);
        int expertCount = config.emailCount - easyCount - mediumCount - hardCount;

        // Ajoute les emails
        AddEmailsToList(result, easyEmails, easyCount);
        AddEmailsToList(result, mediumEmails, mediumCount);
        AddEmailsToList(result, hardEmails, hardCount);
        AddEmailsToList(result, expertEmails, expertCount);

        // Si on n'a pas assez d'emails, complète avec des faciles/moyens
        while (result.Count < config.emailCount)
        {
            if (easyEmails.Count > 0)
            {
                result.Add(easyEmails[0]);
                easyEmails.RemoveAt(0);
            }
            else if (mediumEmails.Count > 0)
            {
                result.Add(mediumEmails[0]);
                mediumEmails.RemoveAt(0);
            }
            else
            {
                break;
            }
        }

        // Mélange final pour que les difficultés ne soient pas groupées
        return Shuffle(result);
    }

    void AddEmailsToList(List<EmailJSON> target, List<EmailJSON> source, int count)
    {
        int added = 0;
        foreach (var email in source)
        {
            if (added >= count) break;
            if (!target.Contains(email))
            {
                target.Add(email);
                added++;
            }
        }
    }

    /// <summary>
    /// Mélange une liste aléatoirement (Fisher-Yates).
    /// </summary>
    List<EmailJSON> Shuffle(List<EmailJSON> list)
    {
        List<EmailJSON> shuffled = new List<EmailJSON>(list);
        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            EmailJSON temp = shuffled[i];
            shuffled[i] = shuffled[j];
            shuffled[j] = temp;
        }
        return shuffled;
    }

    /// <summary>
    /// Récupère tous les emails de la base de données.
    /// </summary>
    public List<EmailJSON> GetAllEmails()
    {
        return database?.emails ?? new List<EmailJSON>();
    }

    /// <summary>
    /// Récupère le nombre total d'emails dans la base.
    /// </summary>
    public int GetTotalEmailCount()
    {
        return database?.emails.Count ?? 0;
    }
}
