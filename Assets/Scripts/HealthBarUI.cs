using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Image[] segments;

    [SerializeField] private Color fullColor = new Color32(0x53, 0x53, 0x53, 255);
    [SerializeField] private Color emptyColor = new Color32(0xD9, 0xD9, 0xD9, 255);

    private void Start()
    {
        playerHealth.OnLivesChanged += Refresh;
        Refresh(playerHealth.CurrentLives);   // show the starting value
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.OnLivesChanged -= Refresh;
    }

    private void Refresh(int lives)
    {
        for (int i = 0; i < segments.Length; i++)
            segments[i].color = i < lives ? fullColor : emptyColor;
    }
}