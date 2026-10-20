using UnityEngine;

public class AirObstacle : Interactable
{
    [SerializeField] private int damage = 1;

    [Header("Bobbing (synced to the wing flap)")]
    [SerializeField] private float bobAmplitude = 0.25f;
    [Tooltip("Length of the PteroFly clip in seconds (18 frames at 60 = 0.3)")]
    [SerializeField] private float flapDuration = 0.3f;
    [Tooltip("How many wing flaps happen during one up-and-down bob")]
    [SerializeField] private int flapsPerBob = 2;
    [Tooltip("Shifts the bob relative to the wings, in degrees. Try 0, 90, 180, 270.")]
    [SerializeField, Range(0f, 360f)] private float phaseOffset = 0f;

    private float baseY;
    private float elapsed;   // time since this pterodactyl appeared

    private void Start()
    {
        baseY = transform.position.y;
    }

    protected override void Update()
    {
        base.Update();

        elapsed += Time.deltaTime;
        float bobPeriod = flapDuration * flapsPerBob;
        float phase = (elapsed / bobPeriod) * Mathf.PI * 2f + phaseOffset * Mathf.Deg2Rad;

        Vector3 p = transform.position;
        p.y = baseY + Mathf.Sin(phase) * bobAmplitude;
        transform.position = p;
    }

    protected override void OnPlayerContact(PlayerController player)
    {
        player.Health.TakeDamage(damage);
    }
}