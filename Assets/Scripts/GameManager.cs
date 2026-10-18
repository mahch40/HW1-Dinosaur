using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private const string HighScoreKey = "HighScore";

    public static GameManager Instance { get; private set; }

    [Header("Speed")]
    [SerializeField] private float startSpeed = 6f;
    [SerializeField] private float maxSpeed = 14f;
    [SerializeField] private float acceleration = 0.15f;

    [Header("Score")]
    [SerializeField] private float distanceScoreRate = 1.5f;

    public int Score { get; private set; }
    public int HighScore { get; private set; }
    public bool IsGameOver { get; private set; }

    public event Action<int> OnScoreChanged;
    public event Action<int> OnHighScoreChanged;
    public event Action OnGameOver;

    private float scoreAccumulator;
    private PlayerHealth playerHealth;

    private void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
        GameSpeed.Current = startSpeed;
        HighScore = PlayerPrefs.GetInt(HighScoreKey, 0);
    }

    private void Start()
    {
        playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth != null)
            playerHealth.OnDied += HandleGameOver;
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.OnDied -= HandleGameOver;
    }

    private void Update()
    {
        if (IsGameOver) return;

        GameSpeed.Current = Mathf.Min(GameSpeed.Current + acceleration * Time.deltaTime, maxSpeed);

        scoreAccumulator += GameSpeed.Current * Time.deltaTime * distanceScoreRate;
        int whole = (int)scoreAccumulator;
        if (whole > 0)
        {
            scoreAccumulator -= whole;
            AddScore(whole);
        }
    }

    public void AddScore(int amount)
    {
        if (IsGameOver) return;
        Score += amount;
        OnScoreChanged?.Invoke(Score);
    }

    private void HandleGameOver()
    {
        IsGameOver = true;

        if (Score > HighScore)
        {
            HighScore = Score;
            PlayerPrefs.SetInt(HighScoreKey, HighScore);
            PlayerPrefs.Save();
            OnHighScoreChanged?.Invoke(HighScore);
        }

        OnGameOver?.Invoke();
    }
}