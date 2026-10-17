using UnityEngine;

public class Coin : Interactable
{
    [SerializeField] private int points = 10;

    protected override void OnPlayerContact(PlayerController player)
    {
        GameManager.Instance.AddScore(points);
        Destroy(gameObject);
    }
}