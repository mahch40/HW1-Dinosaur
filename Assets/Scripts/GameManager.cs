using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Speed")]
    [SerializeField] private float startSpeed = 6f;
    [SerializeField] private float maxSpeed = 14f;
    [SerializeField] private float acceleration = 0.15f;   // speed gained per second

    [Header("Score")]
    [SerializeField] private float distanceScoreRate = 1.5f;   // points per world unit travelled

    public int Score { get; private set; }
    public event Action<int> OnScoreChanged;

    private float scoreAccumulator;

    private void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
        GameSpeed.Current = startSpeed;   // resets the world speed on every (re)start
    }

    private void Update()
    {
        if (GameSpeed.Current <= 0f) return;   // game over

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
        Score += amount;
        OnScoreChanged?.Invoke(Score);
    }
}