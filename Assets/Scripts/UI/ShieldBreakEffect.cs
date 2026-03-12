using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

/// <summary>
/// Effet visuel de bouclier qui se brise quand il est consommé.
/// </summary>
public class ShieldBreakEffect : MonoBehaviour
{
    public static ShieldBreakEffect Instance;

    [Header("Configuration")]
    public Canvas parentCanvas;
    public Sprite shieldSprite;
    public int fragmentCount = 8;
    public Color shieldColor = new Color(0f, 0.8f, 1f, 0.9f);

    private bool isInitialized = false;
    private RectTransform container;
    private Image shieldImage;
    private List<Image> fragments = new List<Image>();

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
        GameObject containerObj = new GameObject("ShieldBreakContainer");
        containerObj.transform.SetParent(parentCanvas.transform, false);
        container = containerObj.AddComponent<RectTransform>();
        container.anchorMin = new Vector2(0.5f, 0.5f);
        container.anchorMax = new Vector2(0.5f, 0.5f);
        container.sizeDelta = Vector2.zero;

        // Bouclier central
        GameObject shieldObj = new GameObject("ShieldIcon");
        shieldObj.transform.SetParent(container, false);
        RectTransform shieldRect = shieldObj.AddComponent<RectTransform>();
        shieldRect.sizeDelta = new Vector2(300, 300);
        shieldImage = shieldObj.AddComponent<Image>();
        if (shieldSprite != null) shieldImage.sprite = shieldSprite;
        shieldImage.color = shieldColor;
        shieldImage.raycastTarget = false;
        shieldObj.SetActive(false);

        // Fragments
        for (int i = 0; i < fragmentCount; i++)
        {
            GameObject fragObj = new GameObject("Fragment_" + i);
            fragObj.transform.SetParent(container, false);
            RectTransform fragRect = fragObj.AddComponent<RectTransform>();
            fragRect.sizeDelta = new Vector2(30, 30);
            Image fragImg = fragObj.AddComponent<Image>();
            if (shieldSprite != null) fragImg.sprite = shieldSprite;
            fragImg.color = shieldColor;
            fragImg.raycastTarget = false;
            fragObj.SetActive(false);
            fragments.Add(fragImg);
        }

        container.gameObject.SetActive(false);
        isInitialized = true;
    }

    /// <summary>
    /// Joue l'effet de bouclier brisé.
    /// </summary>
    public void Play()
    {
        if (!isInitialized) Initialize();
        if (container == null) return;

        container.gameObject.SetActive(true);
        container.SetAsLastSibling();

        // Bouclier apparaît avec scale up
        shieldImage.gameObject.SetActive(true);
        shieldImage.color = shieldColor;
        RectTransform shieldRect = shieldImage.rectTransform;
        shieldRect.localScale = Vector3.zero;

        Sequence seq = DOTween.Sequence();

        // Phase 1 : bouclier apparaît
        seq.Append(shieldRect.DOScale(1.2f, 0.35f).SetEase(Ease.OutBack));
        seq.Append(shieldRect.DOScale(1f, 0.15f));

        // Phase 2 : flash + brisure
        seq.AppendCallback(() =>
        {
            // Cache le bouclier entier
            shieldImage.gameObject.SetActive(false);

            // Spawn les fragments depuis le centre
            for (int i = 0; i < fragments.Count; i++)
            {
                Image frag = fragments[i];
                RectTransform rect = frag.rectTransform;

                frag.gameObject.SetActive(true);
                frag.color = shieldColor;
                rect.anchoredPosition = Vector2.zero;
                rect.localScale = Vector3.one;

                float size = Random.Range(40f, 80f);
                rect.sizeDelta = new Vector2(size, size);
                rect.localRotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

                // Direction explosive
                float angle = (360f / fragments.Count * i) + Random.Range(-20f, 20f);
                float rad = angle * Mathf.Deg2Rad;
                float distance = Random.Range(200f, 400f);
                Vector2 endPos = new Vector2(Mathf.Cos(rad) * distance, Mathf.Sin(rad) * distance);

                float lifetime = Random.Range(0.7f, 1.1f);

                Sequence fragSeq = DOTween.Sequence();
                fragSeq.Append(rect.DOAnchorPos(endPos, lifetime).SetEase(Ease.OutQuad));
                fragSeq.Join(frag.DOFade(0f, lifetime).SetEase(Ease.InQuad));
                fragSeq.Join(rect.DOScale(0f, lifetime).SetEase(Ease.InQuad));
                fragSeq.Join(rect.DORotate(new Vector3(0, 0, Random.Range(-180f, 180f)), lifetime, RotateMode.FastBeyond360));
                fragSeq.OnComplete(() => frag.gameObject.SetActive(false));
            }
        });

        // Phase 3 : nettoyage
        seq.AppendInterval(1f);
        seq.OnComplete(() => container.gameObject.SetActive(false));
    }
}
