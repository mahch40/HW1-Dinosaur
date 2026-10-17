using UnityEngine;

public class AirObstacle : Interactable
{
    [SerializeField] private int damage = 1;
    [SerializeField] private float bobAmplitude = 0.2f;
    [SerializeField] private float bobSpeed = 3f;

    private float baseY;

    private void Start()
    {
        baseY = transform.position.y;
    }

    protected override void Update()
    {
        base.Update();

        Vector3 p = transform.position;
        p.y = baseY + Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
        transform.position = p;
    }

    protected override void OnPlayerContact(PlayerController player)
    {
        player.Health.TakeDamage(damage);
    }
}