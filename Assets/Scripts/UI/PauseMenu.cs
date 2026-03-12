using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Menu pause accessible pendant le gameplay.
/// Permet de continuer ou retourner au menu principal.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    public static PauseMenu Instance;

    [Header("Panel")]
    public GameObject pausePanel;
    public CanvasGroup canvasGroup;

    [Header("Boutons")]
    public Button pauseButton;
    public Button continueButton;
    public Button menuButton;

    [Header("Volume")]
    public Slider volumeSlider;

    [Header("Animation")]
    public float animationDuration = 0.2f;

    private bool isPaused = false;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (pauseButton != null)
            pauseButton.onClick.AddListener(TogglePause);

        if (continueButton != null)
            continueButton.onClick.AddListener(Continuer);

        if (menuButton != null)
            menuButton.onClick.AddListener(RetourMenu);

        // Connecte le slider de volume
        if (volumeSlider != null && AudioManager.Instance != null)
        {
            volumeSlider.value = AudioManager.Instance.musicVolume;
            volumeSlider.onValueChanged.AddListener(AudioManager.Instance.OnVolumeChanged);
        }
    }

    public void TogglePause()
    {
        if (isPaused)
            Continuer();
        else
            Pause();
    }

    public void Pause()
    {
        if (pausePanel == null) return;

        isPaused = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0f;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, animationDuration).SetUpdate(true);
        }
    }

    public void Continuer()
    {
        if (pausePanel == null) return;

        isPaused = false;
        Time.timeScale = 1f;

        if (canvasGroup != null)
        {
            canvasGroup.DOFade(0f, animationDuration).SetUpdate(true)
                .OnComplete(() => pausePanel.SetActive(false));
        }
        else
        {
            pausePanel.SetActive(false);
        }
    }

    public void RetourMenu()
    {
        isPaused = false;
        Time.timeScale = 1f;
        pausePanel.SetActive(false);

        // Cache l'UI de gameplay
        if (GameManager.Instance != null && GameManager.Instance.emailCardUI != null)
            GameManager.Instance.emailCardUI.gameObject.SetActive(false);

        // Affiche le menu principal
        if (MainMenu.Instance != null)
            MainMenu.Instance.Show();
    }
}
