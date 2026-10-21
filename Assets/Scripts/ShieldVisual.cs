using UnityEngine;

// Tints the dino blue while the shield is active.
public class ShieldVisual : MonoBehaviour
{
    [SerializeField] private Color shieldColor = new Color(0.25f, 0.6f, 1f, 1f);
    [SerializeField] private float warningTime = 1.5f;      // blink in the last seconds
    [SerializeField] private float blinkInterval = 0.12f;

    private PlayerHealth health;
    private SpriteRenderer sr;
    private bool wasActive;

    private void Awake()
    {
        health = GetComponent<PlayerHealth>();
        sr = GetComponent<SpriteRenderer>();
    }

    // LateUpdate runs after the hit-blink coroutine, so the shield color wins.
    private void LateUpdate()
    {
        if (health.IsDead) return;

        if (health.HasShield)
        {
            bool warning = health.ShieldRemaining <= warningTime;
            bool blinkOff = warning && ((int)(Time.time / blinkInterval) % 2 == 1);
            sr.color = blinkOff ? Color.white : shieldColor;
            wasActive = true;
        }
        else if (wasActive)
        {
            wasActive = false;
            if (!health.IsInvincible)
                sr.color = Color.white;   // back to normal
        }
    }
}