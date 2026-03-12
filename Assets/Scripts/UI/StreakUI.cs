using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;

/// <summary>
/// Affiche la série en cours avec animation, tremblement, flammes et étincelles.
/// </summary>
public class StreakUI : MonoBehaviour
{
    public static StreakUI Instance;

    [Header("Références")]
    public TextMeshProUGUI streakText;
    public RectTransform streakRect;
    public CanvasGroup canvasGroup;

    [Header("Flammes")]
    [Tooltip("Sprite utilisé pour les flammes (un carré blanc suffit)")]
    public Sprite flameSprite;
    public int flamePoolSize = 40;
    public int sparkPoolSize = 20;

    [Header("Couleurs par palier")]
    public Color normalColor = Color.white;
    public Color warmColor = new Color(1f, 0.8f, 0f);
    public Color hotColor = new Color(1f, 0.5f, 0f);
    public Color fireColor = new Color(1f, 0.2f, 0f);
    public Color legendaryColor = new Color(1f, 0f, 0.5f);

    [Header("Couleurs Flammes")]
    public Color flameYellow = new Color(1f, 0.9f, 0.2f, 0.9f);
    public Color flameOrange = new Color(1f, 0.5f, 0f, 0.9f);
    public Color flameRed = new Color(1f, 0.15f, 0f, 0.8f);
    public Color flameWhite = new Color(1f, 1f, 0.8f, 1f);

    private int lastStreak = 0;
    private Tweener shakeTween;
    private Tweener pulseTween;
    private List<Image> flamePool = new List<Image>();
    private List<Image> sparkPool = new List<Image>();
    private bool flamesActive = false;
    private Sequence flameSequence;
    private Sequence sparkSequence;
    private Sequence legendarySequence;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        InitFlamePool();
        InitSparkPool();
    }

    void InitFlamePool()
    {
        for (int i = 0; i < flamePoolSize; i++)
        {
            GameObject flameObj = new GameObject("Flame_" + i);
            flameObj.transform.SetParent(streakRect != null ? streakRect : transform, false);

            RectTransform rect = flameObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(12, 12);

            Image img = flameObj.AddComponent<Image>();
            if (flameSprite != null)
                img.sprite = flameSprite;
            img.raycastTarget = false;
            img.color = new Color(1, 1, 1, 0);

            flameObj.SetActive(false);
            flamePool.Add(img);
        }
    }

    void InitSparkPool()
    {
        for (int i = 0; i < sparkPoolSize; i++)
        {
            GameObject sparkObj = new GameObject("Spark_" + i);
            sparkObj.transform.SetParent(streakRect != null ? streakRect : transform, false);

            RectTransform rect = sparkObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(4, 4);

            Image img = sparkObj.AddComponent<Image>();
            img.raycastTarget = false;
            img.color = new Color(1, 1, 1, 0);

            sparkObj.SetActive(false);
            sparkPool.Add(img);
        }
    }

    public void ShowStreak(int streak)
    {
        if (streakText == null || streakRect == null) return;

        if (streak <= 1)
        {
            StopAll();
            if (canvasGroup != null && canvasGroup.alpha > 0)
                canvasGroup.DOFade(0f, 0.3f);
            lastStreak = streak;
            return;
        }

        // Texte avec emoji selon palier
        if (streak >= 12)
            streakText.text = streak + " DE SUITE !!!";
        else if (streak >= 8)
            streakText.text = streak + " de suite !!";
        else
            streakText.text = streak + " de suite !";

        // Couleur
        Color targetColor = GetStreakColor(streak);
        streakText.color = targetColor;

        // Apparition
        if (canvasGroup != null)
        {
            canvasGroup.DOKill();
            canvasGroup.alpha = 1f;
        }

        // Punch scale - plus violent avec la streak
        streakRect.DOKill();
        float intensity = Mathf.Clamp01(streak / 12f);
        float punchScale = 0.3f + intensity * 0.7f;
        float duration = 0.4f - intensity * 0.15f;

        streakRect.localScale = Vector3.one;
        streakRect.DOPunchScale(Vector3.one * punchScale, duration, 1, 0.5f)
            .OnComplete(() =>
            {
                StartContinuousShake(streak);
                StartPulse(streak);
            });

        // Flammes à partir de 5
        if (streak >= 5)
        {
            StartFlames(streak);
            StartSparks(streak);
        }
        else
        {
            StopFlames();
            StopSparks();
        }

        // Mode légendaire à 12+
        if (streak >= 12)
            StartLegendaryMode();
        else
            StopLegendaryMode();

        lastStreak = streak;
    }

    public void HideStreak()
    {
        StopAll();

        if (canvasGroup != null)
        {
            canvasGroup.DOKill();
            canvasGroup.DOFade(0f, 0.5f);
        }
        lastStreak = 0;
    }

    void StopAll()
    {
        StopContinuousShake();
        StopPulse();
        StopFlames();
        StopSparks();
        StopLegendaryMode();
    }

    // ========== TREMBLEMENT CONTINU ==========

    void StartContinuousShake(int streak)
    {
        if (streak < 3 || streakRect == null) return;

        StopContinuousShake();

        float shakeStrength = Mathf.Lerp(1f, 12f, Mathf.Clamp01((streak - 3f) / 12f));
        int vibrato = (int)Mathf.Lerp(5, 25, Mathf.Clamp01((streak - 3f) / 12f));

        shakeTween = streakRect.DOShakeAnchorPos(0.5f, shakeStrength, vibrato, 90, false, true)
            .SetLoops(-1, LoopType.Restart)
            .SetUpdate(true);
    }

    void StopContinuousShake()
    {
        if (shakeTween != null && shakeTween.IsActive())
        {
            shakeTween.Kill();
            shakeTween = null;
        }

        if (streakRect != null)
            streakRect.anchoredPosition = Vector2.zero;
    }

    // ========== PULSE (RESPIRATION) ==========

    void StartPulse(int streak)
    {
        if (streak < 5 || streakRect == null) return;

        StopPulse();

        float pulseAmount = Mathf.Lerp(0.05f, 0.15f, Mathf.Clamp01((streak - 5f) / 10f));
        float pulseSpeed = Mathf.Lerp(0.8f, 0.4f, Mathf.Clamp01((streak - 5f) / 10f));

        pulseTween = streakRect.DOScale(1f + pulseAmount, pulseSpeed)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .SetUpdate(true);
    }

    void StopPulse()
    {
        if (pulseTween != null && pulseTween.IsActive())
        {
            pulseTween.Kill();
            pulseTween = null;
        }

        if (streakRect != null)
            streakRect.localScale = Vector3.one;
    }

    // ========== FLAMMES UI ==========

    void StartFlames(int streak)
    {
        if (flamesActive) StopFlames();
        flamesActive = true;

        int flamesPerWave = Mathf.Clamp((streak - 3) * 3, 3, flamePoolSize / 2);
        float interval = Mathf.Lerp(0.25f, 0.06f, Mathf.Clamp01((streak - 5f) / 10f));

        flameSequence = DOTween.Sequence();
        flameSequence.SetLoops(-1);
        flameSequence.SetUpdate(true);

        flameSequence.AppendCallback(() => SpawnFlameWave(flamesPerWave, streak));
        flameSequence.AppendInterval(interval);
    }

    void SpawnFlameWave(int count, int streak)
    {
        float textWidth = streakText != null ? streakText.preferredWidth : 200f;
        float halfWidth = textWidth * 0.7f;

        Color[] colors;
        if (streak >= 12)
            colors = new[] { flameWhite, flameYellow, flameOrange, flameRed };
        else if (streak >= 8)
            colors = new[] { flameYellow, flameOrange, flameRed };
        else
            colors = new[] { flameYellow, flameOrange };

        for (int i = 0; i < count; i++)
        {
            Image flame = GetAvailableFlame();
            if (flame == null) break;

            RectTransform rect = flame.rectTransform;

            // Position de départ : dessous et autour du texte
            float startX = Random.Range(-halfWidth, halfWidth);
            float startY = Random.Range(-25f, 10f);
            rect.anchoredPosition = new Vector2(startX, startY);

            // Taille - grosses flammes
            float baseSize = Mathf.Lerp(30f, 60f, Mathf.Clamp01((streak - 5f) / 10f));
            float size = baseSize * Random.Range(0.5f, 1.5f);
            rect.sizeDelta = new Vector2(size, size * Random.Range(1f, 1.3f));

            // Rotation
            rect.localRotation = Quaternion.Euler(0, 0, Random.Range(-40f, 40f));

            // Couleur
            Color color = colors[Random.Range(0, colors.Length)];
            color.a = Random.Range(0.7f, 1f);
            flame.color = color;

            flame.gameObject.SetActive(true);

            // Animation : monte haut + fade + rétrécit
            float lifetime = Random.Range(0.3f, 0.7f);
            float endY = startY + Random.Range(60f, 130f);
            float driftX = Random.Range(-30f, 30f);

            Sequence s = DOTween.Sequence();
            s.SetUpdate(true);
            s.Append(rect.DOAnchorPos(new Vector2(startX + driftX, endY), lifetime).SetEase(Ease.OutQuad));
            s.Join(flame.DOFade(0f, lifetime).SetEase(Ease.InCubic));
            s.Join(rect.DOScale(0.2f, lifetime).SetEase(Ease.InQuad));
            s.Join(rect.DORotate(new Vector3(0, 0, Random.Range(-60f, 60f)), lifetime, RotateMode.FastBeyond360));
            s.OnComplete(() =>
            {
                flame.gameObject.SetActive(false);
                rect.localScale = Vector3.one;
            });
        }
    }

    Image GetAvailableFlame()
    {
        foreach (var flame in flamePool)
        {
            if (!flame.gameObject.activeSelf)
                return flame;
        }
        return null;
    }

    void StopFlames()
    {
        flamesActive = false;

        if (flameSequence != null && flameSequence.IsActive())
        {
            flameSequence.Kill();
            flameSequence = null;
        }

        foreach (var flame in flamePool)
        {
            flame.DOKill();
            flame.rectTransform.DOKill();
            flame.gameObject.SetActive(false);
            flame.rectTransform.localScale = Vector3.one;
        }
    }

    // ========== ÉTINCELLES ==========

    void StartSparks(int streak)
    {
        StopSparks();

        float interval = Mathf.Lerp(0.2f, 0.08f, Mathf.Clamp01((streak - 5f) / 10f));
        int sparksPerWave = Mathf.Clamp(streak - 3, 2, sparkPoolSize / 3);

        sparkSequence = DOTween.Sequence();
        sparkSequence.SetLoops(-1);
        sparkSequence.SetUpdate(true);

        sparkSequence.AppendCallback(() => SpawnSparkWave(sparksPerWave, streak));
        sparkSequence.AppendInterval(interval);
    }

    void SpawnSparkWave(int count, int streak)
    {
        float textWidth = streakText != null ? streakText.preferredWidth : 200f;
        float halfWidth = textWidth * 0.5f;

        for (int i = 0; i < count; i++)
        {
            Image spark = GetAvailableSpark();
            if (spark == null) break;

            RectTransform rect = spark.rectTransform;

            // Petites étincelles qui partent dans toutes les directions
            float startX = Random.Range(-halfWidth, halfWidth);
            float startY = Random.Range(-10f, 10f);
            rect.anchoredPosition = new Vector2(startX, startY);

            float sparkSize = Random.Range(3f, 8f);
            rect.sizeDelta = new Vector2(sparkSize, sparkSize);
            rect.localRotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

            // Couleur vive
            Color color = streak >= 12
                ? Color.HSVToRGB(Random.Range(0f, 1f), 1f, 1f)
                : new Color(1f, Random.Range(0.5f, 1f), Random.Range(0f, 0.3f), 1f);
            spark.color = color;

            spark.gameObject.SetActive(true);

            // Direction aléatoire explosive
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float distance = Random.Range(60f, 160f);
            float endX = startX + Mathf.Cos(angle) * distance;
            float endY = startY + Mathf.Sin(angle) * distance;

            float lifetime = Random.Range(0.2f, 0.5f);

            Sequence s = DOTween.Sequence();
            s.SetUpdate(true);
            s.Append(rect.DOAnchorPos(new Vector2(endX, endY), lifetime).SetEase(Ease.OutQuad));
            s.Join(spark.DOFade(0f, lifetime).SetEase(Ease.InQuad));
            s.Join(rect.DOScale(0f, lifetime).SetEase(Ease.InQuad));
            s.OnComplete(() =>
            {
                spark.gameObject.SetActive(false);
                rect.localScale = Vector3.one;
            });
        }
    }

    Image GetAvailableSpark()
    {
        foreach (var spark in sparkPool)
        {
            if (!spark.gameObject.activeSelf)
                return spark;
        }
        return null;
    }

    void StopSparks()
    {
        if (sparkSequence != null && sparkSequence.IsActive())
        {
            sparkSequence.Kill();
            sparkSequence = null;
        }

        foreach (var spark in sparkPool)
        {
            spark.DOKill();
            spark.rectTransform.DOKill();
            spark.gameObject.SetActive(false);
            spark.rectTransform.localScale = Vector3.one;
        }
    }

    // ========== MODE LÉGENDAIRE (12+) ==========

    void StartLegendaryMode()
    {
        StopLegendaryMode();

        legendarySequence = DOTween.Sequence();
        legendarySequence.SetLoops(-1);
        legendarySequence.SetUpdate(true);

        // Cycle de couleurs sur le texte
        legendarySequence.Append(streakText.DOColor(flameYellow, 0.15f));
        legendarySequence.Append(streakText.DOColor(flameOrange, 0.15f));
        legendarySequence.Append(streakText.DOColor(flameRed, 0.15f));
        legendarySequence.Append(streakText.DOColor(legendaryColor, 0.15f));
        legendarySequence.Append(streakText.DOColor(flameWhite, 0.15f));
    }

    void StopLegendaryMode()
    {
        if (legendarySequence != null && legendarySequence.IsActive())
        {
            legendarySequence.Kill();
            legendarySequence = null;
        }
    }

    Color GetStreakColor(int streak)
    {
        if (streak >= 12) return legendaryColor;
        if (streak >= 8) return fireColor;
        if (streak >= 5) return hotColor;
        if (streak >= 3) return warmColor;
        return normalColor;
    }
}
