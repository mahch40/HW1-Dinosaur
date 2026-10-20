using TMPro;
using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text finalScoreText;
    [SerializeField] private TMP_Text bestScoreText;

    private void Start()
    {
        panel.SetActive(false);
        GameManager.Instance.OnGameOver += Show;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameOver -= Show;
    }

    private void Show()
    {
        finalScoreText.text = "SCORE " + GameManager.Instance.Score.ToString("D5");
        bestScoreText.text = "BEST " + GameManager.Instance.HighScore.ToString("D5");
        panel.SetActive(true);
    }
}