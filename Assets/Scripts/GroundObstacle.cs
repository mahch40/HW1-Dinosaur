using UnityEngine;

public class GroundObstacle : Interactable
{
    [SerializeField] private int damage = 1;

    protected override void OnPlayerContact(PlayerController player)
    {
        player.Health.TakeDamage(damage);
    }
}