using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

/// <summary>
/// Écran de chargement au lancement du jeu.
/// Anime des enveloppes qui tombent dans une boîte aux lettres.
/// </summary>
public class LoadingScreen : MonoBehaviour
{
    public static LoadingScreen Instance;

    [Header("Panel")]
    public GameObject loadingPanel;
    public CanvasGroup canvasGroup;

    [Header("Boîte aux lettres")]
    public Image mailboxImage;

    [Header("Enveloppe")]
    public Sprite envelopeSprite;
    public int maxEnvelopes = 5;

    [Header("Texte")]
    public TextMeshProUGUI loadingText;

    [Header("Durée minimum")]
    public float minimumDisplayTime = 3f;

    private float startTime;
    private bool isReady = false;
    private float nextSpawnTime = 0f;
    private float spawnInterval = 0.6f;
    private RectTransform panelRect;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        startTime = Time.time;

        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
            panelRect = loadingPanel.GetComponent<RectTransform>();
        }

        if (canvasGroup != null)
            canvasGroup.alpha = 1f;

        // Animation des points "Chargement..."
        if (loadingText != null)
            AnimateLoadingText();
    }

    void Update()
    {
        // Spawn des enveloppes en boucle
        if (!isReady && Time.time >= nextSpawnTime && envelopeSprite != null && mailboxImage != null)
        {
            SpawnEnvelope();
            nextSpawnTime = Time.time + spawnInterval;
        }

        // Attend le temps minimum
        if (!isReady && Time.time - startTime >= minimumDisplayTime)
        {
            isReady = true;
            Hide();
        }
    }

    void SpawnEnvelope()
    {
        // Crée une enveloppe UI
        GameObject envelopeObj = new GameObject("Envelope");
        envelopeObj.transform.SetParent(loadingPanel.transform, false);

        // Place l'enveloppe devant la boîte aux lettres
        if (mailboxImage != null)
            envelopeObj.transform.SetSiblingIndex(mailboxImage.transform.GetSiblingIndex() + 1);

        Image img = envelopeObj.AddComponent<Image>();
        img.sprite = envelopeSprite;
        img.preserveAspect = true;

        RectTransform rect = envelopeObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(160, 120);

        // Position de départ : aléatoire en haut
        float randomX = Random.Range(-150f, 150f);
        rect.anchoredPosition = new Vector2(randomX, 400f);

        // Rotation aléatoire de départ
        rect.rotation = Quaternion.Euler(0, 0, Random.Range(-30f, 30f));

        // Position cible : au-dessus de la boîte aux lettres
        Vector2 targetPos = mailboxImage.rectTransform.anchoredPosition + new Vector2(Random.Range(-20f, 20f), 80f);

        // Animation : tombe vers la boîte aux lettres
        Sequence seq = DOTween.Sequence();
        seq.Append(rect.DOAnchorPos(targetPos, 0.5f).SetEase(Ease.InQuad));
        seq.Join(rect.DORotate(Vector3.zero, 0.5f));
        seq.Append(rect.DOScale(0.3f, 0.2f).SetEase(Ease.InBack));
        seq.Join(img.DOFade(0f, 0.2f));
        seq.OnComplete(() => Destroy(envelopeObj));
    }

    void AnimateLoadingText()
    {
        if (loadingText == null) return;

        Sequence seq = DOTween.Sequence();
        seq.AppendCallback(() => loadingText.text = "Chargement");
        seq.AppendInterval(0.4f);
        seq.AppendCallback(() => loadingText.text = "Chargement.");
        seq.AppendInterval(0.4f);
        seq.AppendCallback(() => loadingText.text = "Chargement..");
        seq.AppendInterval(0.4f);
        seq.AppendCallback(() => loadingText.text = "Chargement...");
        seq.AppendInterval(0.4f);
        seq.SetLoops(-1);
    }

    public bool IsLoading()
    {
        return loadingPanel != null && loadingPanel.activeSelf;
    }

    /// <summary>
    /// Cache l'écran de chargement avec un fade out.
    /// </summary>
    public void Hide()
    {
        if (canvasGroup != null)
        {
            canvasGroup.DOFade(0f, 0.5f).OnComplete(() =>
            {
                if (loadingPanel != null)
                    loadingPanel.SetActive(false);

                // Lance la musique du menu après le loading
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlayMenuMusic();
            });
        }
        else if (loadingPanel != null)
        {
            loadingPanel.SetActive(false);
        }
    }
}
