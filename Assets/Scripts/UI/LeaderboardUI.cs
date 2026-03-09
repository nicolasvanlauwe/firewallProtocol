using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using DG.Tweening;

/// <summary>
/// Interface du classement en ligne.
/// Affiche le top 50 des joueurs avec leur score et jour max.
/// </summary>
public class LeaderboardUI : MonoBehaviour
{
    [Header("Panel")]
    public GameObject leaderboardPanel;
    public CanvasGroup canvasGroup;

    [Header("Contenu")]
    public Transform entriesContainer;
    public GameObject entryPrefab;
    public ScrollRect scrollRect;

    [Header("Textes")]
    public TextMeshProUGUI playerRankText;

    [Header("Boutons")]
    public Button closeButton;
    public Button refreshButton;

    [Header("Animation")]
    public float animationDuration = 0.3f;

    [Header("ScrollView")]
    public float entryHeight = 40f;
    public float maxScrollViewHeight = 400f;

    [Header("Chargement")]
    public GameObject loadingIndicator;

    private List<GameObject> spawnedEntries = new List<GameObject>();
    private System.Action onCloseCallback;

    void Start()
    {
        if (leaderboardPanel != null)
            leaderboardPanel.SetActive(false);

        // Désactive les raycasts quand le panel est fermé
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        // Configure le layout du conteneur pour empiler les entrées
        if (entriesContainer != null)
        {
            VerticalLayoutGroup layout = entriesContainer.GetComponent<VerticalLayoutGroup>();
            if (layout == null)
            {
                layout = entriesContainer.gameObject.AddComponent<VerticalLayoutGroup>();
                layout.spacing = 8f;
                layout.padding = new RectOffset(5, 5, 5, 5);
                layout.childForceExpandWidth = true;
                layout.childForceExpandHeight = false;
                layout.childControlWidth = true;
                layout.childControlHeight = true;
            }

            ContentSizeFitter fitter = entriesContainer.GetComponent<ContentSizeFitter>();
            if (fitter == null)
            {
                fitter = entriesContainer.gameObject.AddComponent<ContentSizeFitter>();
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
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

        // Remet le scroll en haut
        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 1f;

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
        // Affiche le chargement
        if (loadingIndicator != null)
            loadingIndicator.SetActive(true);

        // Nettoie les anciennes entrées
        foreach (GameObject entry in spawnedEntries)
        {
            Destroy(entry);
        }
        spawnedEntries.Clear();

        // Charge le top 50
        if (LeaderboardManager.Instance != null)
        {
            LeaderboardManager.Instance.GetTopScores(50, OnScoresReceived);
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

            // Cherche les textes dans le prefab
            TextMeshProUGUI[] texts = entryObj.GetComponentsInChildren<TextMeshProUGUI>();

            if (texts.Length >= 4)
            {
                texts[0].text = "#" + (i + 1);              // Rang
                texts[1].text = entries[i].pseudo;            // Pseudo
                texts[2].text = entries[i].bestScore + " pts"; // Score
                texts[3].text = "Jour " + entries[i].highestDay; // Jour max
            }

            // Highlight si c'est le joueur actuel
            if (!string.IsNullOrEmpty(currentPseudo) && entries[i].pseudo == currentPseudo)
            {
                Image bg = entryObj.GetComponent<Image>();
                if (bg != null)
                {
                    bg.color = new Color(0f, 1f, 1f, 0.2f); // Cyan transparent
                }
            }
        }

        // Ajuste la taille du ScrollView selon le contenu
        StartCoroutine(AjusterTailleScrollViewCoroutine());
    }

    System.Collections.IEnumerator AjusterTailleScrollViewCoroutine()
    {
        // Attend une frame pour que le layout se recalcule
        yield return null;

        if (scrollRect == null || entriesContainer == null) yield break;

        RectTransform scrollRectTransform = scrollRect.GetComponent<RectTransform>();
        RectTransform contentRect = entriesContainer as RectTransform;
        if (scrollRectTransform == null || contentRect == null) yield break;

        // Force le recalcul du layout
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);

        // Utilise la vraie taille du contenu, plafonnée à maxScrollViewHeight
        float contentHeight = contentRect.rect.height + 20f;
        float newHeight = Mathf.Min(contentHeight, maxScrollViewHeight);

        scrollRectTransform.sizeDelta = new Vector2(scrollRectTransform.sizeDelta.x, newHeight);

        // Remet le scroll en haut
        scrollRect.verticalNormalizedPosition = 1f;
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
