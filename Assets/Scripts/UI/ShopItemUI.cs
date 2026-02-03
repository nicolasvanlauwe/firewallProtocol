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
        if (priceText != null) priceText.text = shopItem.price.ToString() + " coins";

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

        // État du bouton
        if (buyButton != null)
        {
            buyButton.interactable = canAfford && !alreadyOwned;

            // Change le texte du bouton
            TextMeshProUGUI btnText = buyButton.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
            {
                if (alreadyOwned)
                {
                    btnText.text = "Possédé";
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

        // Prix en rouge si pas assez
        if (priceText != null)
        {
            priceText.color = canAfford ? Color.black : Color.red;
        }
    }

    void OnBuyClicked()
    {
        if (shopUI != null && item != null)
        {
            shopUI.OnPurchase(item.id);
        }
    }
}
