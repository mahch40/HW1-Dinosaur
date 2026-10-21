using UnityEngine;

public class HeartPickup : Interactable
{
    [SerializeField] private int healAmount = 1;

    protected override void OnPlayerContact(PlayerController player)
    {
        player.Health.Heal(healAmount);
        Destroy(gameObject);
    }
}