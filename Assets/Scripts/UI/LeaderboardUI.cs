using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using DG.Tweening;

/// <summary>
/// Interface du classement en ligne.
/// Affiche le top 8 des joueurs avec leur score et jour max.
/// </summary>
public class LeaderboardUI : MonoBehaviour
{
    [Header("Panel")]
    public GameObject leaderboardPanel;
    public CanvasGroup canvasGroup;

    [Header("Contenu")]
    public Transform entriesContainer;
    public GameObject entryPrefab;

    [Header("Textes")]
    public TextMeshProUGUI playerRankText;

    [Header("Boutons")]
    public Button closeButton;
    public Button refreshButton;

    [Header("Animation")]
    public float animationDuration = 0.3f;

    [Header("Entrées")]
    public float entryHeight = 30f;
    public float entrySpacing = 5f;

    [Header("Chargement")]
    public GameObject loadingIndicator;

    private List<GameObject> spawnedEntries = new List<GameObject>();

    void Start()
    {
        if (leaderboardPanel != null)
            leaderboardPanel.SetActive(false);

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        if (closeButton != null)
            closeButton.onClick.AddListener(Fermer);

        if (refreshButton != null)
            refreshButton.onClick.AddListener(Refresh);
    }

    /// <summary>
    /// Ouvre le leaderboard et charge les scores.
    /// </summary>
    public void Ouvrir()
    {
        if (leaderboardPanel == null) return;

        leaderboardPanel.SetActive(true);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
            canvasGroup.DOFade(1f, animationDuration);
        }

        Refresh();
    }

    /// <summary>
    /// Ferme le leaderboard.
    /// </summary>
    public void Fermer()
    {
        if (leaderboardPanel == null) return;

        if (canvasGroup != null)
        {
            canvasGroup.interactable = false;
            canvasGroup.DOFade(0f, animationDuration)
                .OnComplete(() =>
                {
                    canvasGroup.blocksRaycasts = false;
                    leaderboardPanel.SetActive(false);
                });
        }
        else
        {
            leaderboardPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Rafraîchit le classement.
    /// </summary>
    public void Refresh()
    {
        if (loadingIndicator != null)
            loadingIndicator.SetActive(true);

        foreach (GameObject entry in spawnedEntries)
        {
            Destroy(entry);
        }
        spawnedEntries.Clear();

        if (LeaderboardManager.Instance != null)
        {
            LeaderboardManager.Instance.GetTopScores(8, OnScoresReceived);
            LeaderboardManager.Instance.GetPlayerRank(OnRankReceived);
        }
    }

    void OnScoresReceived(List<LeaderboardEntry> entries)
    {
        if (loadingIndicator != null)
            loadingIndicator.SetActive(false);

        if (entryPrefab == null || entriesContainer == null) return;

        string currentPseudo = LeaderboardManager.Instance != null
            ? LeaderboardManager.Instance.GetPseudo()
            : "";

        for (int i = 0; i < entries.Count; i++)
        {
            GameObject entryObj = Instantiate(entryPrefab, entriesContainer);
            entryObj.SetActive(true);
            spawnedEntries.Add(entryObj);

            // Positionne l'entrée manuellement
            RectTransform entryRect = entryObj.GetComponent<RectTransform>();
            if (entryRect != null)
            {
                entryRect.anchoredPosition = new Vector2(0, -i * (entryHeight + entrySpacing));
            }

            TextMeshProUGUI[] texts = entryObj.GetComponentsInChildren<TextMeshProUGUI>();

            if (texts.Length >= 3)
            {
                texts[0].text = entries[i].pseudo;
                texts[1].text = entries[i].bestScore.ToString();
                texts[2].text = entries[i].highestDay.ToString();
            }

            if (!string.IsNullOrEmpty(currentPseudo) && entries[i].pseudo == currentPseudo)
            {
                Image bg = entryObj.GetComponent<Image>();
                if (bg != null)
                {
                    bg.color = new Color(0f, 1f, 1f, 0.2f);
                }
            }
        }
    }

    void OnRankReceived(int rank)
    {
        if (playerRankText != null)
        {
            if (rank > 0)
            {
                playerRankText.text = $"Ton classement: #{rank}";
            }
            else
            {
                playerRankText.text = "Pas encore classé";
            }
        }
    }
}
