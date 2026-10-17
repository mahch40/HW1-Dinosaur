using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private float startSpeed = 6f;

    public int Score { get; private set; }
    public event Action<int> OnScoreChanged;

    private void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
        GameSpeed.Current = startSpeed;   // resets the world speed on every (re)start
    }

    public void AddScore(int amount)
    {
        Score += amount;
        OnScoreChanged?.Invoke(Score);
    }
}