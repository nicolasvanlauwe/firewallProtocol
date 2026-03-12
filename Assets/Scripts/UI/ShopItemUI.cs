using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Script pour un item individuel de la boutique.
/// À attacher au prefab ShopItem.
/// </summary>
public class ShopItemUI : MonoBehaviour
{
    [Header("Références UI")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI priceText;
    public Button buyButton;
    public Image backgroundImage;

    [Header("Couleurs")]
    public Color affordableColor = new Color(0.9f, 0.95f, 0.9f);
    public Color unaffordableColor = new Color(0.95f, 0.9f, 0.9f);
    public Color ownedColor = new Color(0.85f, 0.85f, 0.85f);

    private ShopItem item;
    private ShopUI shopUI;

    /// <summary>
    /// Configure l'item avec les données
    /// </summary>
    public void Setup(ShopItem shopItem, ShopUI ui)
    {
        item = shopItem;
        shopUI = ui;

        // Textes
        if (nameText != null) nameText.text = shopItem.name;
        if (descriptionText != null) descriptionText.text = shopItem.description;
        if (priceText != null) priceText.text = shopItem.price.ToString() + " cryptos";

        // Bouton
        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(OnBuyClicked);
        }

        // Met à jour l'état visuel
        UpdateVisualState();
    }

    /// <summary>
    /// Met à jour l'apparence selon si on peut acheter ou non
    /// </summary>
    public void UpdateVisualState()
    {
        if (item == null) return;

        PlayerProgress progress = PlayerProgress.Instance;
        bool canAfford = progress.coins >= item.price;
        bool alreadyOwned = !item.isConsumable && progress.HasItem(item.id);
        bool shieldActive = item.id == "shield" && progress.HasItem("shield_active");

        // Couleur de fond
        if (backgroundImage != null)
        {
            if (alreadyOwned)
            {
                backgroundImage.color = ownedColor;
            }
            else if (canAfford)
            {
                backgroundImage.color = affordableColor;
            }
            else
            {
                backgroundImage.color = unaffordableColor;
            }
        }

        // Quantité possédée pour les consommables
        int ownedCount = 0;
        if (item.isConsumable)
        {
            switch (item.id)
            {
                case "extra_life": ownedCount = progress.extraLives; break;
                case "hint": ownedCount = progress.hints; break;
                case "skip": ownedCount = progress.skips; break;
                case "shield": ownedCount = progress.HasItem("shield_active") ? 1 : 0; break;
            }
        }

        // État du bouton
        if (buyButton != null)
        {
            buyButton.interactable = canAfford && !alreadyOwned && !shieldActive;

            TextMeshProUGUI btnText = buyButton.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
            {
                if (alreadyOwned)
                {
                    btnText.text = "Possédé";
                }
                else if (shieldActive)
                {
                    btnText.text = "Épuisé";
                }
                else if (canAfford)
                {
                    btnText.text = "Acheter";
                }
                else
                {
                    btnText.text = "Pas assez";
                }
            }
        }

        // Affiche la quantité pour les consommables
        if (item.isConsumable && nameText != null)
        {
            nameText.text = item.name + " (x" + ownedCount + ")";
        }

        // Prix en rouge si pas assez
        if (priceText != null)
        {
            priceText.color = canAfford ? Color.black : Color.red;
        }
    }

    void OnBuyClicked()
    {
        Debug.Log($"[Shop] Clic sur Acheter: {item?.id} | shopUI={shopUI != null}");
        if (shopUI != null && item != null)
        {
            shopUI.OnPurchase(item.id);
        }
    }
}
