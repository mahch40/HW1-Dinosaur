using UnityEngine;

public class Coin : Interactable
{
    [SerializeField] private int points = 10;
    [SerializeField] private FloatingText floatingTextPrefab;
    [SerializeField] private Vector3 textSpawnOffset = new Vector3(0f, 0.4f, 0f);

    protected override void OnPlayerContact(PlayerController player)
    {
        GameManager.Instance.AddScore(points);
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayCoin();

        if (floatingTextPrefab != null)
        {
            FloatingText popup = Instantiate(floatingTextPrefab, transform.position + textSpawnOffset, Quaternion.identity);
            popup.Initialize(points);
        }

        Destroy(gameObject);
    }
}