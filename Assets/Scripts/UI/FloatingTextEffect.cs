using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;

/// <summary>
/// Affiche des textes flottants (+20 pts, -10 intégrité) avec icône sprite.
/// </summary>
public class FloatingTextEffect : MonoBehaviour
{
    public static FloatingTextEffect Instance;

    [Header("Configuration")]
    public Canvas parentCanvas;
    public TMP_FontAsset font;
    public int poolSize = 10;

    [Header("Sprites")]
    [Tooltip("Icône affichée à côté du score (ex: étoile)")]
    public Sprite scoreIcon;
    [Tooltip("Icône affichée à côté des dégâts (ex: coeur)")]
    public Sprite damageIcon;
    public float iconSize = 50f;

    [Header("Couleurs")]
    public Color scoreColor = new Color(0.2f, 1f, 0.4f);
    public Color damageColor = new Color(1f, 0.2f, 0.2f);

    [Header("Animation")]
    public float fontSize = 42f;
    public float floatDistance = 120f;
    public float duration = 1.2f;

    private List<FloatingEntry> pool = new List<FloatingEntry>();
    private RectTransform container;
    private bool isInitialized = false;

    struct FloatingEntry
    {
        public GameObject root;
        public TextMeshProUGUI text;
        public Image icon;
        public RectTransform rect;
        public CanvasGroup canvasGroup;
    }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        if (isInitialized) return;

        if (parentCanvas == null)
        {
            parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas == null)
                parentCanvas = FindObjectOfType<Canvas>();
        }

        if (parentCanvas == null) return;

        // Container
        GameObject containerObj = new GameObject("FloatingTextContainer");
        containerObj.transform.SetParent(parentCanvas.transform, false);
        container = containerObj.AddComponent<RectTransform>();
        container.anchorMin = Vector2.zero;
        container.anchorMax = Vector2.one;
        container.offsetMin = Vector2.zero;
        container.offsetMax = Vector2.zero;

        // Pool
        for (int i = 0; i < poolSize; i++)
        {
            // Root avec layout horizontal
            GameObject rootObj = new GameObject("FloatingItem_" + i);
            rootObj.transform.SetParent(container, false);
            RectTransform rootRect = rootObj.AddComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(400, 80);
            CanvasGroup cg = rootObj.AddComponent<CanvasGroup>();

            // Icône (enfant gauche)
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(rootObj.transform, false);
            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.sizeDelta = new Vector2(iconSize, iconSize);
            iconRect.anchoredPosition = new Vector2(-80f, 0);
            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.raycastTarget = false;
            iconImg.preserveAspect = true;

            // Texte (enfant droit)
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(rootObj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            if (font != null) tmp.font = font;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
            tmp.enableWordWrapping = false;

            rootObj.SetActive(false);

            pool.Add(new FloatingEntry
            {
                root = rootObj,
                text = tmp,
                icon = iconImg,
                rect = rootRect,
                canvasGroup = cg
            });
        }

        isInitialized = true;
    }

    public void ShowScore(int points)
    {
        SpawnFloating("+" + points, scoreColor, scoreIcon);
    }

    public void ShowDamage(int damage)
    {
        SpawnFloating("-" + damage + "%", damageColor, damageIcon);
    }

    void SpawnFloating(string text, Color color, Sprite icon)
    {
        if (!isInitialized) Initialize();

        FloatingEntry entry = GetAvailableEntry();
        if (entry.root == null) return;

        RectTransform rect = entry.rect;

        // Position aléatoire au centre de l'écran
        RectTransform canvasRect = parentCanvas.GetComponent<RectTransform>();
        float canvasW = canvasRect.rect.width;
        float canvasH = canvasRect.rect.height;

        float x = Random.Range(-canvasW * 0.4f, canvasW * 0.4f);
        float y = Random.Range(-canvasH * 0.3f, canvasH * 0.25f);
        rect.anchoredPosition = new Vector2(x, y);

        // Setup texte
        entry.text.text = text;
        entry.text.color = color;

        // Setup icône
        if (icon != null)
        {
            entry.icon.sprite = icon;
            entry.icon.color = Color.white;
            entry.icon.gameObject.SetActive(true);
        }
        else
        {
            entry.icon.gameObject.SetActive(false);
        }

        // Reset
        entry.canvasGroup.alpha = 1f;
        rect.localScale = Vector3.zero;
        rect.localRotation = Quaternion.Euler(0, 0, Random.Range(-8f, 8f));
        entry.root.SetActive(true);

        container.SetAsLastSibling();

        // Animation
        Sequence seq = DOTween.Sequence();

        // Pop in
        seq.Append(rect.DOScale(1.3f, 0.15f).SetEase(Ease.OutBack));
        seq.Append(rect.DOScale(1f, 0.1f));

        // Monte + fade
        float endY = y + floatDistance;
        seq.Append(rect.DOAnchorPosY(endY, duration * 0.7f).SetEase(Ease.OutQuad));
        seq.Join(entry.canvasGroup.DOFade(0f, duration * 0.4f).SetDelay(duration * 0.3f).SetEase(Ease.InQuad));

        seq.OnComplete(() =>
        {
            entry.root.SetActive(false);
            rect.localScale = Vector3.one;
        });
    }

    FloatingEntry GetAvailableEntry()
    {
        foreach (var entry in pool)
        {
            if (!entry.root.activeSelf)
                return entry;
        }
        return default;
    }
}
