using UnityEngine;
using System;
using System.Collections.Generic;
using Firebase.Firestore;
using Firebase.Extensions;

/// <summary>
/// Gère le leaderboard en ligne via Firebase Firestore.
/// Singleton accessible via LeaderboardManager.Instance.
/// </summary>
public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance;

    private FirebaseFirestore db;
    private bool isFirebaseReady = false;

    private const string COLLECTION_NAME = "leaderboard";
    private const string PSEUDO_KEY = "PlayerPseudo";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        InitializeFirebase();
    }

    void InitializeFirebase()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == Firebase.DependencyStatus.Available)
            {
                db = FirebaseFirestore.DefaultInstance;
                isFirebaseReady = true;
                Debug.Log("[Leaderboard] Firebase initialisé");
            }
            else
            {
                Debug.LogError($"[Leaderboard] Firebase non disponible: {task.Result}");
            }
        });
    }

    /// <summary>
    /// Récupère le pseudo sauvegardé localement.
    /// </summary>
    public string GetPseudo()
    {
        return PlayerPrefs.GetString(PSEUDO_KEY, "");
    }

    /// <summary>
    /// Sauvegarde le pseudo localement.
    /// </summary>
    public void SetPseudo(string pseudo)
    {
        PlayerPrefs.SetString(PSEUDO_KEY, pseudo);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Vérifie si le joueur a déjà un pseudo.
    /// </summary>
    public bool HasPseudo()
    {
        return !string.IsNullOrEmpty(GetPseudo());
    }

    /// <summary>
    /// Vérifie si un pseudo existe déjà dans Firebase.
    /// </summary>
    public void IsPseudoTaken(string pseudo, Action<bool> callback)
    {
        if (!isFirebaseReady)
        {
            callback?.Invoke(false);
            return;
        }

        db.Collection(COLLECTION_NAME).Document(pseudo).GetSnapshotAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    callback?.Invoke(false);
                    return;
                }

                callback?.Invoke(task.Result.Exists);
            });
    }

    /// <summary>
    /// Envoie le score au leaderboard si meilleur que l'ancien.
    /// </summary>
    public void SubmitScore(int score, int day)
    {
        if (!isFirebaseReady || !HasPseudo()) return;

        string pseudo = GetPseudo();
        DocumentReference docRef = db.Collection(COLLECTION_NAME).Document(pseudo);

        // Vérifie si le score existant est meilleur
        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"[Leaderboard] Erreur lecture: {task.Exception}");
                return;
            }

            DocumentSnapshot snapshot = task.Result;
            bool shouldUpdate = true;

            if (snapshot.Exists)
            {
                int existingScore = snapshot.GetValue<int>("bestScore");
                if (score <= existingScore)
                {
                    shouldUpdate = false;
                }
            }

            if (shouldUpdate)
            {
                var data = new Dictionary<string, object>
                {
                    { "pseudo", pseudo },
                    { "bestScore", score },
                    { "highestDay", day },
                    { "timestamp", FieldValue.ServerTimestamp }
                };

                docRef.SetAsync(data).ContinueWithOnMainThread(setTask =>
                {
                    if (setTask.IsFaulted)
                    {
                        Debug.LogError($"[Leaderboard] Erreur écriture: {setTask.Exception}");
                    }
                    else
                    {
                        Debug.Log($"[Leaderboard] Score soumis: {pseudo} = {score} pts, jour {day}");
                    }
                });
            }
        });
    }

    /// <summary>
    /// Récupère les meilleurs scores.
    /// </summary>
    public void GetTopScores(int limit, Action<List<LeaderboardEntry>> callback)
    {
        if (!isFirebaseReady)
        {
            callback?.Invoke(new List<LeaderboardEntry>());
            return;
        }

        db.Collection(COLLECTION_NAME)
            .OrderByDescending("bestScore")
            .Limit(limit)
            .GetSnapshotAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError($"[Leaderboard] Erreur récupération: {task.Exception}");
                    callback?.Invoke(new List<LeaderboardEntry>());
                    return;
                }

                List<LeaderboardEntry> entries = new List<LeaderboardEntry>();
                foreach (DocumentSnapshot doc in task.Result.Documents)
                {
                    LeaderboardEntry entry = new LeaderboardEntry
                    {
                        pseudo = doc.GetValue<string>("pseudo"),
                        bestScore = doc.GetValue<int>("bestScore"),
                        highestDay = doc.GetValue<int>("highestDay")
                    };
                    entries.Add(entry);
                }

                callback?.Invoke(entries);
            });
    }

    /// <summary>
    /// Récupère le rang du joueur actuel.
    /// </summary>
    public void GetPlayerRank(Action<int> callback)
    {
        if (!isFirebaseReady || !HasPseudo())
        {
            callback?.Invoke(-1);
            return;
        }

        string pseudo = GetPseudo();

        // Récupère le score du joueur
        db.Collection(COLLECTION_NAME).Document(pseudo).GetSnapshotAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || !task.Result.Exists)
                {
                    callback?.Invoke(-1);
                    return;
                }

                int playerScore = task.Result.GetValue<int>("bestScore");

                // Compte combien de joueurs ont un score supérieur
                db.Collection(COLLECTION_NAME)
                    .WhereGreaterThan("bestScore", playerScore)
                    .GetSnapshotAsync()
                    .ContinueWithOnMainThread(countTask =>
                    {
                        if (countTask.IsFaulted)
                        {
                            callback?.Invoke(-1);
                            return;
                        }

                        int rank = countTask.Result.Count + 1;
                        callback?.Invoke(rank);
                    });
            });
    }
}
