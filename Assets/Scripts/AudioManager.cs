using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

#pragma warning disable 0649
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Music Clips")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip gameMusic;

    [Header("Sound Effect Clips")]
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip coinSound;
    [SerializeField] private AudioClip gameOverSound;

    [Header("Optional Action Clips")]
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip dashSound;
#pragma warning restore 0649

    [Header("Volume Settings")]
    [Range(0f, 1f)] [SerializeField] private float musicVolume = 1f;
    [Range(0f, 1f)] [SerializeField] private float sfxVolume = 1f;
    [Range(0f, 1f)] [SerializeField] private float pauseMusicVolumeMultiplier = 0.3f;

    private bool isPaused;
    private GameManager currentGameManager;
    private PlayerHealth currentPlayerHealth;

    private void Awake()
    {
        // Singleton pattern: persist across scenes and destroy any duplicate instance
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureAudioSources();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        UnsubscribeFromGameEvents();
    }

    private void Start()
    {
        // When launched directly in editor, ensure active scene audio is initialized
        if (musicSource != null && !musicSource.isPlaying)
        {
            HandleSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }
    }

    private void EnsureAudioSources()
    {
        if (musicSource == null)
        {
            var sources = GetComponents<AudioSource>();
            if (sources.Length > 0)
                musicSource = sources[0];
            else
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.playOnAwake = false;
                musicSource.loop = true;
            }
        }

        if (sfxSource == null)
        {
            var sources = GetComponents<AudioSource>();
            if (sources.Length > 1)
                sfxSource = sources[1];
            else
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
                sfxSource.loop = false;
            }
        }

        musicSource.volume = musicVolume;
        sfxSource.volume = sfxVolume;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reset pause volume whenever any scene loads or reloads
        isPaused = false;
        if (musicSource != null)
            musicSource.volume = musicVolume;

        UnsubscribeFromGameEvents();

        if (scene.name == SceneLoader.MainMenuScene)
        {
            // Do not restart menu music if it is already playing
            PlayMusic(menuMusic, restartIfSame: false);
        }
        else if (scene.name == SceneLoader.GameScene)
        {
            // Always start game music cleanly from the beginning on new game / restart
            PlayMusic(gameMusic, restartIfSame: true);
            SubscribeToGameEvents();
        }
    }

    private void SubscribeToGameEvents()
    {
        currentGameManager = GameManager.Instance != null ? GameManager.Instance : FindAnyObjectByType<GameManager>();
        if (currentGameManager != null)
        {
            currentGameManager.OnGameOver += HandleGameOver;
        }

        currentPlayerHealth = FindAnyObjectByType<PlayerHealth>();
        if (currentPlayerHealth != null)
        {
            currentPlayerHealth.OnHit += HandlePlayerHit;
        }
    }

    private void UnsubscribeFromGameEvents()
    {
        if (currentGameManager != null)
        {
            currentGameManager.OnGameOver -= HandleGameOver;
            currentGameManager = null;
        }

        if (currentPlayerHealth != null)
        {
            currentPlayerHealth.OnHit -= HandlePlayerHit;
            currentPlayerHealth = null;
        }
    }

    public void PlayMusic(AudioClip clip, bool restartIfSame = false)
    {
        if (musicSource == null) return;

        if (clip == null)
        {
            musicSource.Stop();
            musicSource.clip = null;
            return;
        }

        if (musicSource.clip == clip && musicSource.isPlaying && !restartIfSame)
        {
            return;
        }

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.time = 0f;
        musicSource.volume = isPaused ? musicVolume * pauseMusicVolumeMultiplier : musicVolume;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    public void SetPaused(bool paused)
    {
        isPaused = paused;
        if (musicSource != null)
        {
            musicSource.volume = isPaused ? musicVolume * pauseMusicVolumeMultiplier : musicVolume;
        }
    }

    public void PlaySfx(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    public void PlayCoin()
    {
        PlaySfx(coinSound);
    }

    public void PlayHit()
    {
        PlaySfx(hitSound);
    }

    public void PlayGameOver()
    {
        PlaySfx(gameOverSound);
    }

    public void PlayJump()
    {
        PlaySfx(jumpSound);
    }

    public void PlayDash()
    {
        PlaySfx(dashSound);
    }

    private void HandlePlayerHit()
    {
        // Fatal hit: skip hit sound so Game Over sound plays exclusively
        if (currentPlayerHealth != null && currentPlayerHealth.CurrentLives <= 0)
        {
            return;
        }

        PlayHit();
    }

    private void HandleGameOver()
    {
        // Stop background game music first, then play Game Over sound once
        StopMusic();
        PlayGameOver();
    }
}
