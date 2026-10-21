using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshPro))]
public class FloatingText : MonoBehaviour
{
    [Header("Motion")]
    [Tooltip("Speed at which the text floats upward (units per second)")]
    [SerializeField] private float floatSpeed = 1.5f;

    [Tooltip("Optional horizontal drift to the left matching world speed")]
    [SerializeField] private bool driftWithWorld = false;

    [Header("Fade & Lifetime")]
    [Tooltip("Duration in seconds before the text fades out and is destroyed")]
    [SerializeField] private float lifetime = 0.8f;

    [Header("Rendering")]
    [Tooltip("Sorting order in layer to render above sprites")]
    [SerializeField] private int sortingOrder = 20;

    [Tooltip("Default text color (dark gray matching the game palette)")]
    [SerializeField] private Color textColor = new Color(0.325f, 0.325f, 0.325f, 1f);

    private TextMeshPro tmp;
    private Color startColor;
    private float elapsed;

    private void Awake()
    {
        tmp = GetComponent<TextMeshPro>();
        if (tmp != null)
        {
            tmp.sortingOrder = sortingOrder;
            // Use TMP's own color if specified in inspector/prefab, otherwise fallback to textColor
            startColor = tmp.color.a > 0f ? tmp.color : textColor;
        }
        else
        {
            startColor = textColor;
        }
    }

    /// <summary>
    /// Initializes the floating text with the points earned.
    /// </summary>
    /// <param name="points">The points value to display.</param>
    public void Initialize(int points)
    {
        if (tmp == null)
            tmp = GetComponent<TextMeshPro>();

        if (tmp != null)
        {
            tmp.text = $"+{points}";
            tmp.sortingOrder = sortingOrder;
            startColor = tmp.color.a > 0f ? tmp.color : textColor;
            tmp.color = startColor;
        }
    }

    private void Update()
    {
        // Scaled time ensures freezing when paused (Time.timeScale == 0)
        float dt = Time.deltaTime;
        elapsed += dt;

        // Upward motion
        Vector3 move = Vector3.up * (floatSpeed * dt);

        // Optional drift to the left with the world speed
        if (driftWithWorld)
        {
            move += Vector3.left * (GameSpeed.Current * dt);
        }

        transform.position += move;

        // Smoothly fade alpha from 1 to 0 over the configured lifetime
        float progress = Mathf.Clamp01(elapsed / lifetime);
        if (tmp != null)
        {
            Color c = startColor;
            c.a = Mathf.Lerp(startColor.a, 0f, progress);
            tmp.color = c;
        }

        if (elapsed >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}
