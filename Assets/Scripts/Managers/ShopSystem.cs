using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Gère la boutique et les items achetables.
/// </summary>
public class ShopSystem : MonoBehaviour
{
    public static ShopSystem Instance;

    // Liste des items disponibles
    public static readonly List<ShopItem> AllItems = new List<ShopItem>
    {
        // ═══════════════════════════════════════════════════════════
        // CONSOMMABLES
        // ═══════════════════════════════════════════════════════════
        new ShopItem
        {
            id = "extra_life",
            name = "Vie Supplémentaire",
            description = "Restaure 25% d'intégrité quand vous tombez à 0",
            price = 100,
            category = ShopCategory.Consumable,
            icon = "heart",
            isConsumable = true
        },
        new ShopItem
        {
            id = "hint",
            name = "Indice",
            description = "Révèle si l'email est frauduleux ou non",
            price = 50,
            category = ShopCategory.Consumable,
            icon = "lightbulb",
            isConsumable = true
        },
        new ShopItem
        {
            id = "skip",
            name = "Passer",
            description = "Passe un email sans pénalité",
            price = 75,
            category = ShopCategory.Consumable,
            icon = "skip",
            isConsumable = true
        },
        new ShopItem
        {
            id = "shield",
            name = "Bouclier",
            description = "Réduit les dégâts de la prochaine erreur de 50%",
            price = 80,
            category = ShopCategory.Consumable,
            icon = "shield",
            isConsumable = true
        },

        // ═══════════════════════════════════════════════════════════
        // AMÉLIORATIONS PERMANENTES
        // ═══════════════════════════════════════════════════════════
        new ShopItem
        {
            id = "armor_1",
            name = "Pare-feu Basique",
            description = "Réduit tous les dégâts de 10% (permanent)",
            price = 500,
            category = ShopCategory.Upgrade,
            icon = "firewall",
            isConsumable = false
        },
        new ShopItem
        {
            id = "armor_2",
            name = "Pare-feu Avancé",
            description = "Réduit tous les dégâts de 20% (permanent)",
            price = 1000,
            category = ShopCategory.Upgrade,
            icon = "firewall_advanced",
            isConsumable = false,
            requiredItem = "armor_1"
        },
        new ShopItem
        {
            id = "coin_boost_1",
            name = "Bonus Coins +10%",
            description = "Gagnez 10% de coins en plus (permanent)",
            price = 300,
            category = ShopCategory.Upgrade,
            icon = "coin_boost",
            isConsumable = false
        },
        new ShopItem
        {
            id = "coin_boost_2",
            name = "Bonus Coins +25%",
            description = "Gagnez 25% de coins en plus (permanent)",
            price = 800,
            category = ShopCategory.Upgrade,
            icon = "coin_boost",
            isConsumable = false,
            requiredItem = "coin_boost_1"
        },
        new ShopItem
        {
            id = "integrity_boost",
            name = "Intégrité +10",
            description = "Commencez chaque jour avec +10 intégrité",
            price = 600,
            category = ShopCategory.Upgrade,
            icon = "integrity",
            isConsumable = false
        },
        new ShopItem
        {
            id = "streak_keeper",
            name = "Gardien de Série",
            description = "Une erreur ne reset plus votre série",
            price = 750,
            category = ShopCategory.Upgrade,
            icon = "streak",
            isConsumable = false
        }
    };

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

    /// <summary>
    /// Récupère un item par son ID
    /// </summary>
    public ShopItem GetItem(string itemId)
    {
        return AllItems.Find(i => i.id == itemId);
    }

    /// <summary>
    /// Récupère tous les items d'une catégorie
    /// </summary>
    public List<ShopItem> GetItemsByCategory(ShopCategory category)
    {
        return AllItems.FindAll(i => i.category == category);
    }

    /// <summary>
    /// Vérifie si un item peut être acheté
    /// </summary>
    public bool CanPurchase(string itemId)
    {
        ShopItem item = GetItem(itemId);
        if (item == null) return false;

        PlayerProgress progress = PlayerProgress.Instance;

        // Vérifie l'argent
        if (progress.coins < item.price) return false;

        // Vérifie si déjà possédé (pour les non-consommables)
        if (!item.isConsumable && progress.HasItem(itemId)) return false;

        // Vérifie les prérequis
        if (!string.IsNullOrEmpty(item.requiredItem) && !progress.HasItem(item.requiredItem))
            return false;

        return true;
    }

    /// <summary>
    /// Achète un item
    /// </summary>
    public bool Purchase(string itemId)
    {
        if (!CanPurchase(itemId)) return false;

        ShopItem item = GetItem(itemId);
        PlayerProgress progress = PlayerProgress.Instance;

        // Dépense les coins
        if (!progress.SpendCoins(item.price)) return false;

        // Applique l'achat selon le type
        if (item.isConsumable)
        {
            ApplyConsumable(item, progress);
        }
        else
        {
            progress.AddItem(itemId);
        }

        Debug.Log($"🛒 Achat: {item.name} pour {item.price} coins");
        return true;
    }

    /// <summary>
    /// Applique un item consommable
    /// </summary>
    void ApplyConsumable(ShopItem item, PlayerProgress progress)
    {
        ApplySingleConsumable(item.id, progress);
        progress.Save();
    }

    void ApplySingleConsumable(string itemId, PlayerProgress progress)
    {
        switch (itemId)
        {
            case "extra_life":
                progress.extraLives++;
                break;
            case "hint":
                progress.hints++;
                break;
            case "skip":
                progress.skips++;
                break;
            case "shield":
                // Géré différemment, peut être ajouté comme un compteur
                progress.AddItem("shield_active");
                break;
        }
    }

    /// <summary>
    /// Calcule le multiplicateur de dégâts basé sur les améliorations
    /// </summary>
    public float GetDamageMultiplier()
    {
        PlayerProgress progress = PlayerProgress.Instance;
        float multiplier = 1f;

        if (progress.HasItem("armor_1")) multiplier -= 0.1f;
        if (progress.HasItem("armor_2")) multiplier -= 0.1f; // Total 20%

        return Mathf.Max(multiplier, 0.5f); // Min 50% de dégâts
    }

    /// <summary>
    /// Calcule le multiplicateur de coins basé sur les améliorations
    /// </summary>
    public float GetCoinMultiplier()
    {
        PlayerProgress progress = PlayerProgress.Instance;
        float multiplier = 1f;

        if (progress.HasItem("coin_boost_1")) multiplier += 0.1f;
        if (progress.HasItem("coin_boost_2")) multiplier += 0.15f; // Total 25%

        return multiplier;
    }

    /// <summary>
    /// Récupère le bonus d'intégrité au démarrage
    /// </summary>
    public int GetIntegrityBonus()
    {
        if (PlayerProgress.Instance.HasItem("integrity_boost"))
            return 10;
        return 0;
    }
}

/// <summary>
/// Représente un item de la boutique
/// </summary>
public class ShopItem
{
    public string id;
    public string name;
    public string description;
    public int price;
    public ShopCategory category;
    public string icon;
    public bool isConsumable;
    public string requiredItem;  // Item requis pour débloquer
}

/// <summary>
/// Catégories de la boutique
/// </summary>
public enum ShopCategory
{
    Consumable,     // Consommables (vies, indices...)
    Upgrade         // Améliorations permanentes
}
