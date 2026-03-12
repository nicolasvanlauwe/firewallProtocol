using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Gère toute la partie audio du jeu : musiques de fond et effets sonores.
/// Singleton persistant. Assigner les clips dans l'Inspector.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Sources Audio")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Musiques")]
    public AudioClip menuMusic;
    public AudioClip gameplayMusic;
    public AudioClip apartmentMusic;

    [Header("SFX - Réponses")]
    public AudioClip correctSfx;
    public AudioClip wrongSfx;

    [Header("SFX - Streak")]
    public AudioClip streakUpSfx;
    public AudioClip streakBreakSfx;

    [Header("SFX - Power-ups")]
    public AudioClip shieldActiveSfx;
    public AudioClip shieldBreakSfx;
    public AudioClip hintSfx;
    public AudioClip skipSfx;

    [Header("SFX - Boutique")]
    public AudioClip buySfx;
    public AudioClip cantBuySfx;

    [Header("SFX - UI")]
    public AudioClip buttonClickSfx;
    public AudioClip panelOpenSfx;
    public AudioClip panelCloseSfx;

    [Header("SFX - Fin de partie")]
    public AudioClip gameOverSfx;
    public AudioClip victorySfx;
    public AudioClip confettiSfx;

    [Header("SFX - Email")]
    public AudioClip emailSwipeSfx;
    public AudioClip emailArriveSfx;

    [Header("Volume")]
    [Range(0f, 1f)] public float musicVolume = 0.5f;
    [Range(0f, 1f)] public float sfxVolume = 0.8f;
    public float musicFadeDuration = 0.5f;

    [Header("Slider (optionnel)")]
    public Slider volumeSlider;

    [Header("Mute")]
    public Button muteButton;
    public Sprite muteOnSprite;
    public Sprite muteOffSprite;

    private bool isMuted = false;
    private float savedMusicVolume;
    private float savedSfxVolume;

    private const string MUSIC_VOL_KEY = "MusicVolume";
    private const string SFX_VOL_KEY = "SfxVolume";
    private const string MUTE_KEY = "IsMuted";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Charge les volumes sauvegardés
        musicVolume = PlayerPrefs.GetFloat(MUSIC_VOL_KEY, 0.5f);
        sfxVolume = PlayerPrefs.GetFloat(SFX_VOL_KEY, 0.8f);
        isMuted = PlayerPrefs.GetInt(MUTE_KEY, 0) == 1;

        if (isMuted)
        {
            if (musicSource != null) musicSource.volume = 0f;
            if (sfxSource != null) sfxSource.volume = 0f;
        }
        else
        {
            if (musicSource != null) musicSource.volume = musicVolume;
            if (sfxSource != null) sfxSource.volume = sfxVolume;
        }
    }

    void Start()
    {
        // Connecte le slider si assigné
        if (volumeSlider != null)
        {
            volumeSlider.value = musicVolume;
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }

        // Connecte le bouton mute
        if (muteButton != null)
        {
            muteButton.onClick.AddListener(ToggleMute);
            UpdateMuteIcon();
        }
    }

    // ═══════════════════════════════════════════════════════════
    // MUSIQUE
    // ═══════════════════════════════════════════════════════════

    public void PlayMenuMusic()
    {
        PlayMusic(menuMusic);
    }

    public void PlayGameplayMusic()
    {
        PlayMusic(gameplayMusic);
    }

    public void PlayApartmentMusic()
    {
        PlayMusic(apartmentMusic);
    }

    void PlayMusic(AudioClip clip)
    {
        if (musicSource == null || clip == null) return;

        // Si c'est déjà la même musique, ne fait rien
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        // Fade out puis change de musique
        musicSource.DOKill();
        musicSource.DOFade(0f, musicFadeDuration).OnComplete(() =>
        {
            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.Play();
            musicSource.DOFade(isMuted ? 0f : musicVolume, musicFadeDuration);
        });
    }

    public void StopMusic()
    {
        if (musicSource == null) return;
        musicSource.DOKill();
        musicSource.DOFade(0f, musicFadeDuration).OnComplete(() => musicSource.Stop());
    }

    // ═══════════════════════════════════════════════════════════
    // SFX
    // ═══════════════════════════════════════════════════════════

    public void PlaySfx(AudioClip clip)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    // Raccourcis pour les SFX courants
    public void PlayCorrect() => PlaySfx(correctSfx);
    public void PlayWrong() => PlaySfx(wrongSfx);
    public void PlayStreakUp() => PlaySfx(streakUpSfx);
    public void PlayStreakBreak() { if (sfxSource != null && streakBreakSfx != null) sfxSource.PlayOneShot(streakBreakSfx, sfxVolume * 7f); }
    public void PlayShieldActive() => PlaySfx(shieldActiveSfx);
    public void PlayShieldBreak() { if (sfxSource != null && shieldBreakSfx != null) sfxSource.PlayOneShot(shieldBreakSfx, sfxVolume * 3f); }
    public void PlayHint() => PlaySfx(hintSfx);
    public void PlaySkip()
    {
        if (sfxSource != null && skipSfx != null)
        {
            sfxSource.pitch = 3f;
            sfxSource.PlayOneShot(skipSfx, sfxVolume * 100f);
            Invoke("ResetSfxPitch", skipSfx.length / 3f);
        }
    }
    void ResetSfxPitch() { if (sfxSource != null) sfxSource.pitch = 1f; }
    public void PlayBuy() => PlaySfx(buySfx);
    public void PlayCantBuy() => PlaySfx(cantBuySfx);
    public void PlayButtonClick() => PlaySfx(buttonClickSfx);
    public void PlayPanelOpen() => PlaySfx(panelOpenSfx);
    public void PlayPanelClose() => PlaySfx(panelCloseSfx);
    public void PlayGameOver() => PlaySfx(gameOverSfx);
    public void PlayVictory() => PlaySfx(victorySfx);
    public void PlayConfetti() => PlaySfx(confettiSfx);
    public void PlayEmailSwipe() { if (sfxSource != null && emailSwipeSfx != null) sfxSource.PlayOneShot(emailSwipeSfx, sfxVolume * 4f); }
    public void PlayEmailArrive() => PlaySfx(emailArriveSfx);

    // ═══════════════════════════════════════════════════════════
    // VOLUME
    // ═══════════════════════════════════════════════════════════

    public void OnVolumeChanged(float value)
    {
        musicVolume = value;
        sfxVolume = value;

        if (musicSource != null) musicSource.volume = musicVolume;
        if (sfxSource != null) sfxSource.volume = sfxVolume;

        PlayerPrefs.SetFloat(MUSIC_VOL_KEY, musicVolume);
        PlayerPrefs.SetFloat(SFX_VOL_KEY, sfxVolume);
    }

    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);
        if (musicSource != null) musicSource.volume = musicVolume;
        PlayerPrefs.SetFloat(MUSIC_VOL_KEY, musicVolume);
    }

    public void SetSfxVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
        if (sfxSource != null) sfxSource.volume = sfxVolume;
        PlayerPrefs.SetFloat(SFX_VOL_KEY, sfxVolume);
    }

    // ═══════════════════════════════════════════════════════════
    // MUTE
    // ═══════════════════════════════════════════════════════════

    public void ToggleMute()
    {
        isMuted = !isMuted;
        PlayerPrefs.SetInt(MUTE_KEY, isMuted ? 1 : 0);

        if (isMuted)
        {
            if (musicSource != null) musicSource.volume = 0f;
            if (sfxSource != null) sfxSource.volume = 0f;
        }
        else
        {
            if (musicSource != null) musicSource.volume = musicVolume;
            if (sfxSource != null) sfxSource.volume = sfxVolume;
        }

        UpdateMuteIcon();
    }

    void UpdateMuteIcon()
    {
        if (muteButton == null) return;
        Image img = muteButton.GetComponent<Image>();
        if (img != null)
        {
            if (isMuted && muteOnSprite != null)
                img.sprite = muteOnSprite;
            else if (!isMuted && muteOffSprite != null)
                img.sprite = muteOffSprite;
        }
    }

    public bool IsMuted() => isMuted;
}
