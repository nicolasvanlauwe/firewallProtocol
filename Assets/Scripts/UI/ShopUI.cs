using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;

/// <summary>
/// Gère l'interface de la boutique.
/// Génère automatiquement les items depuis ShopSystem.
/// </summary>
public class ShopUI : MonoBehaviour
{
    public static ShopUI Instance;

    [Header("Références Panel")]
    public GameObject shopPanel;
    public CanvasGroup canvasGroup;
    public RectTransform windowRect;

    [Header("Références UI")]
    public TextMeshProUGUI coinsText;
    public Transform itemsContainer;
    public Button closeButton;

    [Header("Prefab Item")]
    public GameObject shopItemPrefab;

    [Header("Animation")]
    public float animationDuration = 0.3f;

    private List<GameObject> spawnedItems = new List<GameObject>();

    void Awake()
    {
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
        // Cache la boutique au démarrage
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }

        // Configure le bouton fermer
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Fermer);
        }
    }

    /// <summary>
    /// Ouvre la boutique avec animation
    /// </summary>
    public void Ouvrir()
    {
        if (shopPanel == null) return;

        shopPanel.SetActive(true);
        RefreshUI();

        // Animation d'ouverture
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, animationDuration);
        }

        if (windowRect != null)
        {
            windowRect.localScale = Vector3.one * 0.8f;
            windowRect.DOScale(1f, animationDuration).SetEase(Ease.OutBack);
        }
    }

    /// <summary>
    /// Ferme la boutique avec animation
    /// </summary>
    public void Fermer()
    {
        if (shopPanel == null) return;

        Sequence seq = DOTween.Sequence();

        if (windowRect != null)
        {
            seq.Append(windowRect.DOScale(0.8f, animationDuration).SetEase(Ease.InBack));
        }

        if (canvasGroup != null)
        {
            seq.Join(canvasGroup.DOFade(0f, animationDuration));
        }

        seq.OnComplete(() => shopPanel.SetActive(false));
    }

    /// <summary>
    /// Rafraîchit l'affichage (coins et items)
    /// </summary>
    public void RefreshUI()
    {
        // Met à jour les coins
        if (coinsText != null)
        {
            coinsText.text = PlayerProgress.Instance.coins.ToString();
        }

        // Génère les items si le prefab existe
        if (shopItemPrefab != null && itemsContainer != null)
        {
            GenerateItems();
        }
    }

    /// <summary>
    /// Génère les items de la boutique
    /// </summary>
    void GenerateItems()
    {
        // Supprime les anciens items
        foreach (var item in spawnedItems)
        {
            if (item != null) Destroy(item);
        }
        spawnedItems.Clear();

        // Crée les nouveaux items
        foreach (var shopItem in ShopSystem.AllItems)
        {
            GameObject itemGO = Instantiate(shopItemPrefab, itemsContainer);
            spawnedItems.Add(itemGO);

            // Configure l'item
            ShopItemUI itemUI = itemGO.GetComponent<ShopItemUI>();
            if (itemUI != null)
            {
                itemUI.Setup(shopItem, this);
            }
        }
    }

    /// <summary>
    /// Appelée quand un achat est effectué
    /// </summary>
    public void OnPurchase(string itemId)
    {
        if (ShopSystem.Instance != null && ShopSystem.Instance.Purchase(itemId))
        {
            RefreshUI();
            // Petit effet visuel
            if (coinsText != null)
            {
                coinsText.rectTransform.DOPunchScale(Vector3.one * 0.2f, 0.3f);
            }
        }
    }
}
