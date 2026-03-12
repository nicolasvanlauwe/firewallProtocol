using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Barre de power-ups affichée pendant le gameplay.
/// Affiche les boutons Indice et Passer avec leur quantité.
/// </summary>
public class PowerUpBarUI : MonoBehaviour
{
    public static PowerUpBarUI Instance;

    [Header("Bouton Indice")]
    public Button hintButton;
    public TextMeshProUGUI hintCountText;

    [Header("Bouton Passer")]
    public Button skipButton;
    public TextMeshProUGUI skipCountText;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        if (hintButton != null)
            hintButton.onClick.AddListener(OnHintClicked);

        if (skipButton != null)
            skipButton.onClick.AddListener(OnSkipClicked);

        RefreshUI();
    }

    /// <summary>
    /// Met à jour l'affichage des quantités et l'état des boutons.
    /// </summary>
    public void RefreshUI()
    {
        PlayerProgress progress = PlayerProgress.Instance;
        if (progress == null) return;

        // Indice
        int hints = progress.hints;
        if (hintCountText != null) hintCountText.text = "x" + hints;
        if (hintButton != null) hintButton.interactable = hints > 0;

        // Passer
        int skips = progress.skips;
        if (skipCountText != null) skipCountText.text = "x" + skips;
        if (skipButton != null) skipButton.interactable = skips > 0;

    }

    void OnHintClicked()
    {
        if (GameManager.Instance != null && GameManager.Instance.UtiliserIndice())
        {
            RefreshUI();
        }
    }

    void OnSkipClicked()
    {
        if (GameManager.Instance != null && GameManager.Instance.PasserEmail())
        {
            RefreshUI();
        }
    }
}
