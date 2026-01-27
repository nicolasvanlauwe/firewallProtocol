using UnityEngine;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Charge les emails depuis un fichier JSON.
/// Peut charger depuis Resources ou depuis un fichier externe.
/// </summary>
public class EmailLoader : MonoBehaviour
{
    public static EmailLoader Instance;

    [Header("📁 Configuration")]
    [Tooltip("Nom du fichier JSON dans Resources (sans extension)")]
    public string jsonFileName = "emails";
    
    [Tooltip("Charger automatiquement au démarrage")]
    public bool loadOnStart = true;

    [Header("🎮 Paramètres de jeu")]
    [Tooltip("Nombre d'emails par partie")]
    public int emailsPerGame = 10;
    
    [Tooltip("Difficulté (vide = toutes)")]
    public string difficulteFiltre = "";
    
    [Header("📊 Debug")]
    [Tooltip("Afficher les logs de chargement")]
    public bool debugMode = true;

    // Base de données chargée
    private EmailDatabase database;
    
    // Emails de la partie en cours
    private List<EmailJSON> currentGameEmails;

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
            Debug.LogError($"❌ Fichier JSON non trouvé : Resources/{jsonFileName}.json");
            return;
        }
        
        database = EmailDatabase.LoadFromJSON(jsonFile.text);
        
        if (debugMode)
        {
            Debug.Log($"✅ {database.emails.Count} emails chargés depuis {jsonFileName}.json");
        }
    }

    /// <summary>
    /// Charge les emails depuis un chemin de fichier externe.
    /// Utile pour modding ou mise à jour sans rebuild.
    /// </summary>
    public void LoadEmailsFromPath(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError($"❌ Fichier non trouvé : {filePath}");
            return;
        }
        
        string jsonContent = File.ReadAllText(filePath);
        database = EmailDatabase.LoadFromJSON(jsonContent);
        
        if (debugMode)
        {
            Debug.Log($"✅ {database.emails.Count} emails chargés depuis {filePath}");
        }
    }

    /// <summary>
    /// Prépare une nouvelle partie avec des emails aléatoires.
    /// </summary>
    public List<EmailData> PrepareNewGame()
    {
        if (database == null)
        {
            Debug.LogError("❌ Base de données non chargée !");
            return new List<EmailData>();
        }
        
        // Récupère des emails aléatoires
        currentGameEmails = database.GetRandomEmails(emailsPerGame, difficulteFiltre);
        
        // Convertit en EmailData pour compatibilité avec GameManager
        List<EmailData> emailDataList = new List<EmailData>();
        foreach (var email in currentGameEmails)
        {
            emailDataList.Add(email.ToEmailData());
        }
        
        if (debugMode)
        {
            Debug.Log($"🎮 Partie préparée avec {emailDataList.Count} emails");
        }
        
        return emailDataList;
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

    /// <summary>
    /// Ajoute un email à la base de données (runtime).
    /// </summary>
    public void AddEmail(EmailJSON email)
    {
        if (database == null)
        {
            database = new EmailDatabase { emails = new List<EmailJSON>() };
        }
        database.emails.Add(email);
    }

    /// <summary>
    /// Exporte la base de données en JSON (pour debug/sauvegarde).
    /// </summary>
    public string ExportToJSON()
    {
        return database?.ToJSON() ?? "{}";
    }
}
