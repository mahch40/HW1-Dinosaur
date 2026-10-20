using TMPro;
using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private TMP_Text bestText;

    private void Start()
    {
        Time.timeScale = 1f;
        bestText.text = "BEST " + PlayerPrefs.GetInt("HighScore", 0).ToString("D5");
    }
}