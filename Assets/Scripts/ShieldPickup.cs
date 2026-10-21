using UnityEngine;

public class ShieldPickup : Interactable
{
    [SerializeField] private float duration = 5f;

    protected override void OnPlayerContact(PlayerController player)
    {
        player.Health.ActivateShield(duration);
        Destroy(gameObject);
    }
}