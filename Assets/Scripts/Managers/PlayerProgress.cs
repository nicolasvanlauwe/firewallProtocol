using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Gère la progression du joueur : niveau, argent, statistiques.
/// Sauvegarde automatique via PlayerPrefs.
/// </summary>
[Serializable]
public class PlayerProgress
{
    // Singleton
    private static PlayerProgress _instance;
    public static PlayerProgress Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Load();
            }
            return _instance;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // DONNÉES DE PROGRESSION
    // ═══════════════════════════════════════════════════════════

    [Header("Progression")]
    public int currentDay = 1;              // Jour actuel (niveau)
    public int highestDayReached = 1;       // Plus haut niveau atteint
    public int coins = 0;                   // Monnaie du joueur

    [Header("Statistiques globales")]
    public int totalEmailsProcessed = 0;    // Total d'emails traités
    public int totalCorrectAnswers = 0;     // Total de bonnes réponses
    public int totalWrongAnswers = 0;       // Total de mauvaises réponses
    public int totalGamesPlayed = 0;        // Nombre de parties jouées
    public int totalGamesWon = 0;           // Nombre de victoires
    public int bestScore = 0;               // Meilleur score
    public int currentStreak = 0;           // Série actuelle de bonnes réponses
    public int bestStreak = 0;              // Meilleure série

    [Header("Boutique")]
    public List<string> ownedItems = new List<string>();  // Items achetés
    public int extraLives = 0;              // Vies supplémentaires disponibles
    public int hints = 0;                   // Indices disponibles
    public int skips = 0;                   // Sauts d'email disponibles

    // ═══════════════════════════════════════════════════════════
    // CONFIGURATION DES NIVEAUX
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Retourne la configuration pour un jour donné
    /// </summary>
    public static DayConfig GetDayConfig(int day)
    {
        DayConfig config = new DayConfig();

        // Nombre d'emails selon le jour
        config.emailCount = Mathf.Min(5 + day, 15); // 6, 7, 8... max 15

        // Intégrité de départ (diminue avec le temps)
        config.startingIntegrity = Mathf.Max(100 - (day - 1) * 5, 60); // 100, 95, 90... min 60

        // Répartition des difficultés selon le jour
        if (day <= 2)
        {
            // Jours 1-2 : Facile seulement
            config.easyPercent = 100;
            config.mediumPercent = 0;
            config.hardPercent = 0;
            config.expertPercent = 0;
        }
        else if (day <= 4)
        {
            // Jours 3-4 : Facile + Moyen
            config.easyPercent = 60;
            config.mediumPercent = 40;
            config.hardPercent = 0;
            config.expertPercent = 0;
        }
        else if (day <= 6)
        {
            // Jours 5-6 : Facile + Moyen + Difficile
            config.easyPercent = 30;
            config.mediumPercent = 50;
            config.hardPercent = 20;
            config.expertPercent = 0;
        }
        else if (day <= 8)
        {
            // Jours 7-8 : Moyen + Difficile
            config.easyPercent = 10;
            config.mediumPercent = 40;
            config.hardPercent = 40;
            config.expertPercent = 10;
        }
        else
        {
            // Jour 9+ : Tout, majorité difficile/expert
            config.easyPercent = 5;
            config.mediumPercent = 25;
            config.hardPercent = 40;
            config.expertPercent = 30;
        }

        // Multiplicateur de récompense
        config.coinMultiplier = 1f + (day - 1) * 0.2f; // 1x, 1.2x, 1.4x...

        // Nom du jour
        config.dayName = GetDayName(day);

        return config;
    }

    static string GetDayName(int day)
    {
        string[] jours = { "Lundi", "Mardi", "Mercredi", "Jeudi", "Vendredi" };
        int semaine = (day - 1) / 5 + 1;
        int jourDeSemaine = (day - 1) % 5;
        return $"{jours[jourDeSemaine]} (Semaine {semaine})";
    }

    // ═══════════════════════════════════════════════════════════
    // GESTION DE LA MONNAIE
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Calcule les coins gagnés pour une bonne réponse
    /// </summary>
    public int CalculateCoinsForCorrectAnswer(int basePoints, int day, int streak)
    {
        DayConfig config = GetDayConfig(day);

        // Base * multiplicateur du jour
        float coins = basePoints * config.coinMultiplier;

        // Bonus de série (10% par réponse consécutive, max 50%)
        float streakBonus = 1f + Mathf.Min(streak * 0.1f, 0.5f);
        coins *= streakBonus;

        return Mathf.RoundToInt(coins);
    }

    /// <summary>
    /// Calcule le bonus de fin de journée
    /// </summary>
    public int CalculateDayCompletionBonus(int day, int correctAnswers, int totalEmails, int remainingIntegrity)
    {
        int bonus = 0;

        // Bonus de base pour avoir terminé
        bonus += day * 20;

        // Bonus perfection (100% correct)
        if (correctAnswers == totalEmails)
        {
            bonus += 100;
        }

        // Bonus intégrité restante
        bonus += remainingIntegrity;

        return bonus;
    }

    /// <summary>
    /// Ajoute des coins au joueur
    /// </summary>
    public void AddCoins(int amount)
    {
        coins += amount;
        Save();
    }

    /// <summary>
    /// Dépense des coins (retourne false si pas assez)
    /// </summary>
    public bool SpendCoins(int amount)
    {
        if (coins >= amount)
        {
            coins -= amount;
            Save();
            return true;
        }
        return false;
    }

    // ═══════════════════════════════════════════════════════════
    // PROGRESSION
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Passe au jour suivant
    /// </summary>
    public void AdvanceToNextDay()
    {
        currentDay++;
        if (currentDay > highestDayReached)
        {
            highestDayReached = currentDay;
        }
        Save();
    }

    /// <summary>
    /// Enregistre une victoire
    /// </summary>
    public void RecordVictory(int score, int correctAnswers, int wrongAnswers)
    {
        totalGamesPlayed++;
        totalGamesWon++;
        totalCorrectAnswers += correctAnswers;
        totalWrongAnswers += wrongAnswers;
        totalEmailsProcessed += correctAnswers + wrongAnswers;

        if (score > bestScore)
        {
            bestScore = score;
        }

        Save();
    }

    /// <summary>
    /// Enregistre une défaite
    /// </summary>
    public void RecordDefeat(int score, int correctAnswers, int wrongAnswers)
    {
        totalGamesPlayed++;
        totalCorrectAnswers += correctAnswers;
        totalWrongAnswers += wrongAnswers;
        totalEmailsProcessed += correctAnswers + wrongAnswers;

        if (score > bestScore)
        {
            bestScore = score;
        }

        // Reset la série
        currentStreak = 0;

        Save();
    }

    /// <summary>
    /// Met à jour la série de bonnes réponses
    /// </summary>
    public void UpdateStreak(bool correct)
    {
        if (correct)
        {
            currentStreak++;
            if (currentStreak > bestStreak)
            {
                bestStreak = currentStreak;
            }
        }
        else
        {
            currentStreak = 0;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // BOUTIQUE
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Vérifie si le joueur possède un item
    /// </summary>
    public bool HasItem(string itemId)
    {
        return ownedItems.Contains(itemId);
    }

    /// <summary>
    /// Ajoute un item au joueur
    /// </summary>
    public void AddItem(string itemId)
    {
        if (!ownedItems.Contains(itemId))
        {
            ownedItems.Add(itemId);
            Save();
        }
    }

    /// <summary>
    /// Utilise une vie supplémentaire
    /// </summary>
    public bool UseExtraLife()
    {
        if (extraLives > 0)
        {
            extraLives--;
            Save();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Utilise un indice
    /// </summary>
    public bool UseHint()
    {
        if (hints > 0)
        {
            hints--;
            Save();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Utilise un skip
    /// </summary>
    public bool UseSkip()
    {
        if (skips > 0)
        {
            skips--;
            Save();
            return true;
        }
        return false;
    }

    // ═══════════════════════════════════════════════════════════
    // SAUVEGARDE / CHARGEMENT
    // ═══════════════════════════════════════════════════════════

    private const string SAVE_KEY = "PlayerProgress";

    /// <summary>
    /// Sauvegarde la progression
    /// </summary>
    public void Save()
    {
        string json = JsonUtility.ToJson(this);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Charge la progression
    /// </summary>
    public static PlayerProgress Load()
    {
        if (PlayerPrefs.HasKey(SAVE_KEY))
        {
            string json = PlayerPrefs.GetString(SAVE_KEY);
            return JsonUtility.FromJson<PlayerProgress>(json);
        }
        return new PlayerProgress();
    }

    /// <summary>
    /// Réinitialise toute la progression
    /// </summary>
    public void Reset()
    {
        _instance = new PlayerProgress();
        _instance.Save();
    }
}

/// <summary>
/// Configuration d'un jour/niveau
/// </summary>
[Serializable]
public class DayConfig
{
    public string dayName;
    public int emailCount;
    public int startingIntegrity;
    public int easyPercent;
    public int mediumPercent;
    public int hardPercent;
    public int expertPercent;
    public float coinMultiplier;
}
