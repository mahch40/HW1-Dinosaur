using System;
using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxLives = 3;
    [SerializeField] private float invincibleTime = 1.5f;
    [SerializeField] private float flashInterval = 0.1f;
    [SerializeField] private Color hitColor = new Color(1f, 0.2f, 0.2f, 1f);

    public int CurrentLives { get; private set; }
    public int MaxLives => maxLives;
    public bool IsDead { get; private set; }
    public bool IsInvincible { get; private set; }

    // Other systems (UI, animation, game manager) subscribe to these.
    public event Action<int> OnLivesChanged;   // passes the new number of lives
    public event Action OnHit;                 // every time damage is taken
    public event Action OnDied;

    private SpriteRenderer sr;
    private Rigidbody2D rb;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        CurrentLives = maxLives;
    }

    private void Start()
    {
        // Lets the UI show the starting value.
        OnLivesChanged?.Invoke(CurrentLives);
    }

    public void TakeDamage(int amount = 1)
    {
        if (IsDead || IsInvincible) return;

        CurrentLives = Mathf.Max(0, CurrentLives - amount);
        Debug.Log($"Lives: {CurrentLives}");
        OnLivesChanged?.Invoke(CurrentLives);
        OnHit?.Invoke();

        if (CurrentLives == 0)
        {
            Die();
            return;
        }

        StartCoroutine(HitFeedback());
    }

    private IEnumerator HitFeedback()
    {
        IsInvincible = true;
        Color faded = new Color(1f, 1f, 1f, 0.35f);
        bool showRed = true;
        float timer = 0f;

        while (timer < invincibleTime)
        {
            sr.color = showRed ? hitColor : faded;
            showRed = !showRed;
            yield return new WaitForSeconds(flashInterval);
            timer += flashInterval;
        }

        sr.color = Color.white;
        IsInvincible = false;
    }

    private void Die()
    {
        IsDead = true;
        sr.color = Color.white;
        GameSpeed.Current = 0f;            // the world stops scrolling

        var controller = GetComponent<PlayerController>();
        if (controller != null) controller.enabled = false;   // no more input
        rb.linearVelocity = Vector2.zero;

        OnDied?.Invoke();
        Debug.Log("Game Over");
    }

    // Temporary test helper: use the component's "⋮" menu while playing.
    [ContextMenu("Test Damage")]
    private void TestDamage() => TakeDamage();
}