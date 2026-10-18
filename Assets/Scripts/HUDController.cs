using TMPro;
using UnityEngine;

public class HUDController : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text highScoreText;

    private void Start()
    {
        var gm = GameManager.Instance;
        gm.OnScoreChanged += UpdateScore;
        gm.OnHighScoreChanged += UpdateHighScore;

        UpdateScore(gm.Score);
        UpdateHighScore(gm.HighScore);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.OnScoreChanged -= UpdateScore;
        GameManager.Instance.OnHighScoreChanged -= UpdateHighScore;
    }

    private void UpdateScore(int score)
    {
        scoreText.text = score.ToString("D5");
    }

    private void UpdateHighScore(int highScore)
    {
        highScoreText.text = "HI " + highScore.ToString("D5");
    }
}