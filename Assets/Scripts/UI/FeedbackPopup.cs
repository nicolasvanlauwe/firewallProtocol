using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// Gère la popup de feedback avec taille adaptative au contenu.
/// Se ferme en touchant n'importe où sur l'écran.
/// Attacher ce script à Feedback_Popup.
/// </summary>
public class FeedbackPopup : MonoBehaviour
{
    [Header("📝 Références UI")]
    [Tooltip("Le texte du message de feedback")]
    public TextMeshProUGUI messageText;
    
    [Tooltip("Le RectTransform de la popup (ce GameObject)")]
    public RectTransform popupRect;
    
    [Tooltip("Fond cliquable qui couvre tout l'écran")]
    public Button backgroundButton;
    
    // Paramètres de taille
    private float popupWidth = 700f;
    private float minHeight = 200f;
    private float maxHeight = 700f;
    private float paddingVertical = 170f;
    private float paddingHorizontal = 150f;
    
    [Header("💡 Indication")]
    [Tooltip("Texte qui indique de toucher pour fermer (optionnel)")]
    public TextMeshProUGUI touchToCloseText;

    void Awake()
    {
        if (popupRect == null)
            popupRect = GetComponent<RectTransform>();
            
        // Configure le bouton de fond pour fermer la popup
        if (backgroundButton != null)
        {
            backgroundButton.onClick.AddListener(Fermer);
        }
    }

    /// <summary>
    /// Affiche la popup avec un message et adapte sa taille.
    /// </summary>
    public void AfficherMessage(string message)
    {
        // Active d'abord la popup pour que le layout se calcule
        gameObject.SetActive(true);
        
        // Met à jour le texte
        messageText.text = message;
        
        // Affiche l'indication "Toucher pour continuer"
        if (touchToCloseText != null)
        {
            touchToCloseText.text = "Toucher pour continuer...";
        }
        
        // Attend une frame puis ajuste la taille
        StartCoroutine(AjusterTailleApresFrame());
    }
    
    /// <summary>
    /// Attend une frame pour que le texte soit rendu, puis ajuste la taille.
    /// </summary>
    private System.Collections.IEnumerator AjusterTailleApresFrame()
    {
        // Attend la fin de la frame pour que TextMeshPro calcule sa taille
        yield return null;
        
        // Force la mise à jour du layout
        messageText.ForceMeshUpdate();
        
        // Calcule la hauteur nécessaire pour le texte
        float textHeight = messageText.preferredHeight;
        
        // Calcule la hauteur totale de la popup
        float totalHeight = textHeight + paddingVertical;
        
        // Clamp entre min et max
        totalHeight = Mathf.Clamp(totalHeight, minHeight, maxHeight);
        
        // Applique la nouvelle taille
        popupRect.sizeDelta = new Vector2(popupWidth, totalHeight);
    }
    
    /// <summary>
    /// Ferme la popup.
    /// </summary>
    public void Fermer()
    {
        gameObject.SetActive(false);
        
        // Informe le GameManager que la popup est fermée
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPopupFermee();
        }
    }
}