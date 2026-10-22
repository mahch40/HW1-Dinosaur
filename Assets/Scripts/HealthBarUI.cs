using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Image[] segments;

    [SerializeField] private Color fullColor = new Color32(0x53, 0x53, 0x53, 255);
    [SerializeField] private Color emptyColor = new Color32(0xD9, 0xD9, 0xD9, 255);

    private int currentLives;

    private void Start()
    {
        if (playerHealth != null)
        {
            playerHealth.OnLivesChanged += Refresh;
            currentLives = playerHealth.CurrentLives;
        }
        else
        {
            currentLives = segments != null ? segments.Length : 3;
        }

        Refresh(currentLives);   // show the starting value
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.OnLivesChanged -= Refresh;
    }

    private void Refresh(int lives)
    {
        currentLives = lives;
        UpdateVisuals();
    }

    /// <summary>
    /// Sets the dynamic theme colors for full and empty segments (e.g. for day/night transitions).
    /// </summary>
    public void SetColors(Color full, Color empty)
    {
        fullColor = full;
        emptyColor = empty;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (segments == null) return;
        for (int i = 0; i < segments.Length; i++)
        {
            if (segments[i] != null)
                segments[i].color = i < currentLives ? fullColor : emptyColor;
        }
    }
}