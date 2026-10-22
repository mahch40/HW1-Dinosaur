using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DayNightCycle : MonoBehaviour
{
    [Header("Cycle Configuration")]
    [Tooltip("Distance in world units for one full day/night cycle (approx. 90-120 seconds of gameplay)")]
    [SerializeField] private float cycleLengthUnits = 1200f;

    [Range(0f, 1f)]
    [Tooltip("Starting phase of the cycle: 0 = Day, 0.38 = Sunset, 0.6 = Night, 0.88 = Sunrise")]
    [SerializeField] private float startPhase = 0f;

    [Header("Visual Interpolation")]
    [Tooltip("Camera background color gradient across the cycle (Day -> Sunset -> Night -> Sunrise -> Day)")]
    [SerializeField] private Gradient backgroundGradient;

    [Tooltip("Global Light 2D color tint gradient across the cycle")]
    [SerializeField] private Gradient lightColorGradient;

    [Tooltip("Global Light 2D intensity curve (boosted to ~2.6-3.0 at night so dark sprites remain readable)")]
    [SerializeField] private AnimationCurve lightIntensityCurve;

    [Header("UI Readability")]
    [Tooltip("HUD text elements that dynamically adapt their color for readability across the cycle")]
    [SerializeField] private TMP_Text[] hudTexts;

    [Tooltip("Text color gradient for HUD texts (dark gray by day -> bright white by night)")]
    [SerializeField] private Gradient hudTextColorGradient;

    [Tooltip("HealthBarUI reference for dynamic heart/segment coloring")]
    [SerializeField] private HealthBarUI healthBarUI;

    [Tooltip("Color gradient for active/full health bar segments")]
    [SerializeField] private Gradient healthBarFullGradient;

    [Tooltip("Color gradient for empty health bar segments")]
    [SerializeField] private Gradient healthBarEmptyGradient;

    [Header("Scene References")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Light2D globalLight;

    private float distanceTraveled;

    /// <summary>
    /// Current normalized cycle phase in [0, 1).
    /// </summary>
    public float CurrentPhase
    {
        get
        {
            if (cycleLengthUnits <= 0f) return 0f;
            return Mathf.Repeat(distanceTraveled / cycleLengthUnits, 1f);
        }
    }

    public DayNightCycle()
    {
        backgroundGradient = CreateDefaultBackgroundGradient();
        lightColorGradient = CreateDefaultLightColorGradient();
        lightIntensityCurve = CreateDefaultLightIntensityCurve();
        hudTextColorGradient = CreateDefaultHudTextColorGradient();
        healthBarFullGradient = CreateDefaultHealthBarFullGradient();
        healthBarEmptyGradient = CreateDefaultHealthBarEmptyGradient();
    }

    private void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();
        }

        EnsureGlobalLight();

        if (healthBarUI == null)
        {
            healthBarUI = FindAnyObjectByType<HealthBarUI>();
        }

        if (hudTexts == null || hudTexts.Length == 0)
        {
            FindDefaultHudTexts();
        }

        // Initialize distance traveled matching startPhase
        distanceTraveled = Mathf.Repeat(startPhase, 1f) * cycleLengthUnits;
    }

    private void Start()
    {
        ApplyPhase(CurrentPhase);
    }

    private void Update()
    {
        float dt = Time.deltaTime; // Scaled time: freezes during pause (Time.timeScale == 0)

        // GameSpeed.Current drops to 0 when the game is over, freezing the cycle
        if (dt > 0f && GameSpeed.Current > 0f)
        {
            distanceTraveled += GameSpeed.Current * dt;
        }

        ApplyPhase(CurrentPhase);
    }

    /// <summary>
    /// Smoothly evaluates and applies all visual attributes for the normalized cycle phase t in [0, 1).
    /// </summary>
    public void ApplyPhase(float t)
    {
        t = Mathf.Repeat(t, 1f);

        // 1. Camera background color
        if (targetCamera != null && backgroundGradient != null)
        {
            targetCamera.backgroundColor = backgroundGradient.Evaluate(t);
        }

        // 2. Global Light 2D color and intensity
        if (globalLight != null)
        {
            if (lightColorGradient != null)
                globalLight.color = lightColorGradient.Evaluate(t);

            if (lightIntensityCurve != null)
                globalLight.intensity = lightIntensityCurve.Evaluate(t);
        }

        // 3. HUD text color adaptation
        if (hudTexts != null && hudTextColorGradient != null)
        {
            Color textColor = hudTextColorGradient.Evaluate(t);
            for (int i = 0; i < hudTexts.Length; i++)
            {
                if (hudTexts[i] != null)
                    hudTexts[i].color = textColor;
            }
        }

        // 4. Health bar colors adaptation
        if (healthBarUI != null && healthBarFullGradient != null && healthBarEmptyGradient != null)
        {
            healthBarUI.SetColors(healthBarFullGradient.Evaluate(t), healthBarEmptyGradient.Evaluate(t));
        }
    }

    private void EnsureGlobalLight()
    {
        if (globalLight == null)
        {
            var lights = FindObjectsByType<Light2D>();
            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i].lightType == Light2D.LightType.Global)
                {
                    globalLight = lights[i];
                    break;
                }
            }
        }

        if (globalLight == null)
        {
            GameObject lightGo = new GameObject("Global Light 2D");
            globalLight = lightGo.AddComponent<Light2D>();
            globalLight.lightType = Light2D.LightType.Global;
            globalLight.color = Color.white;
            globalLight.intensity = 1f;
        }

        // Ensure there is only one active Global Light 2D in the scene
        var allLights = FindObjectsByType<Light2D>();
        int globalCount = 0;
        for (int i = 0; i < allLights.Length; i++)
        {
            if (allLights[i].lightType == Light2D.LightType.Global)
            {
                globalCount++;
                if (globalCount > 1 && allLights[i] != globalLight)
                {
                    Debug.LogWarning($"[DayNightCycle] Duplicate Global Light 2D detected on '{allLights[i].gameObject.name}'. Disabling to prevent conflicts.");
                    allLights[i].enabled = false;
                }
            }
        }
    }

    private void FindDefaultHudTexts()
    {
        var hud = FindAnyObjectByType<HUDController>();
        if (hud != null)
        {
            var list = new List<TMP_Text>();
            foreach (var t in hud.GetComponentsInChildren<TMP_Text>(true))
            {
                if (t.name == "ScoreText" || t.name == "HighScoreText")
                {
                    list.Add(t);
                }
            }
            if (list.Count > 0)
            {
                hudTexts = list.ToArray();
            }
        }
    }

    #region Context Menu Testing Helpers

    [ContextMenu("Jump To Day (t=0.0)")]
    public void JumpToDay() => JumpToPhase(0.0f);

    [ContextMenu("Jump To Sunset (t=0.38)")]
    public void JumpToSunset() => JumpToPhase(0.38f);

    [ContextMenu("Jump To Night (t=0.60)")]
    public void JumpToNight() => JumpToPhase(0.60f);

    [ContextMenu("Jump To Sunrise (t=0.88)")]
    public void JumpToSunrise() => JumpToPhase(0.88f);

    /// <summary>
    /// Jumps the cycle to any normalized phase immediately for testing.
    /// </summary>
    public void JumpToPhase(float phase)
    {
        phase = Mathf.Repeat(phase, 1f);
        distanceTraveled = phase * cycleLengthUnits;
        ApplyPhase(phase);
    }

    #endregion

    #region Default Factory Helpers

    private static Gradient CreateDefaultBackgroundGradient()
    {
        Gradient g = new Gradient();
        g.colorKeys = new GradientColorKey[]
        {
            new GradientColorKey(new Color(1f, 1f, 1f), 0.0f),               // Day
            new GradientColorKey(new Color(1f, 1f, 1f), 0.25f),              // Day hold
            new GradientColorKey(new Color(0.957f, 0.635f, 0.380f), 0.38f),  // Sunset (#F4A261)
            new GradientColorKey(new Color(0.043f, 0.063f, 0.149f), 0.52f),  // Night start (#0B1026)
            new GradientColorKey(new Color(0.043f, 0.063f, 0.149f), 0.75f),  // Night hold
            new GradientColorKey(new Color(0.957f, 0.635f, 0.380f), 0.88f),  // Sunrise (#F4A261)
            new GradientColorKey(new Color(1f, 1f, 1f), 1.0f)                // Day
        };
        g.alphaKeys = new GradientAlphaKey[]
        {
            new GradientAlphaKey(1f, 0f),
            new GradientAlphaKey(1f, 1f)
        };
        return g;
    }

    private static Gradient CreateDefaultLightColorGradient()
    {
        Gradient g = new Gradient();
        g.colorKeys = new GradientColorKey[]
        {
            new GradientColorKey(Color.white, 0.0f),                          // Day (pure white)
            new GradientColorKey(Color.white, 0.25f),
            new GradientColorKey(new Color(1f, 0.89f, 0.776f), 0.38f),       // Sunset warm tint
            new GradientColorKey(new Color(0.706f, 0.824f, 1.0f), 0.52f),    // Night cool light blue
            new GradientColorKey(new Color(0.706f, 0.824f, 1.0f), 0.75f),
            new GradientColorKey(new Color(1f, 0.89f, 0.776f), 0.88f),       // Sunrise warm tint
            new GradientColorKey(Color.white, 1.0f)
        };
        g.alphaKeys = new GradientAlphaKey[]
        {
            new GradientAlphaKey(1f, 0f),
            new GradientAlphaKey(1f, 1f)
        };
        return g;
    }

    private static AnimationCurve CreateDefaultLightIntensityCurve()
    {
        AnimationCurve curve = new AnimationCurve(
            new Keyframe(0.00f, 1.0f),
            new Keyframe(0.25f, 1.0f),
            new Keyframe(0.38f, 1.4f),
            new Keyframe(0.52f, 2.8f),
            new Keyframe(0.75f, 2.8f),
            new Keyframe(0.88f, 1.4f),
            new Keyframe(1.00f, 1.0f)
        );

        for (int i = 0; i < curve.length; i++)
        {
            curve.SmoothTangents(i, 0f);
        }
        return curve;
    }

    private static Gradient CreateDefaultHudTextColorGradient()
    {
        Gradient g = new Gradient();
        g.colorKeys = new GradientColorKey[]
        {
            new GradientColorKey(new Color(0.325f, 0.325f, 0.325f), 0.0f),   // Day (#535353)
            new GradientColorKey(new Color(0.325f, 0.325f, 0.325f), 0.25f),
            new GradientColorKey(new Color(0.886f, 0.886f, 0.886f), 0.38f),  // Sunset transition
            new GradientColorKey(Color.white, 0.52f),                         // Night bright white
            new GradientColorKey(Color.white, 0.75f),
            new GradientColorKey(new Color(0.886f, 0.886f, 0.886f), 0.88f),  // Sunrise transition
            new GradientColorKey(new Color(0.325f, 0.325f, 0.325f), 1.0f)
        };
        g.alphaKeys = new GradientAlphaKey[]
        {
            new GradientAlphaKey(1f, 0f),
            new GradientAlphaKey(1f, 1f)
        };
        return g;
    }

    private static Gradient CreateDefaultHealthBarFullGradient()
    {
        // Full health segments match the text color transition for high readability
        return CreateDefaultHudTextColorGradient();
    }

    private static Gradient CreateDefaultHealthBarEmptyGradient()
    {
        Gradient g = new Gradient();
        g.colorKeys = new GradientColorKey[]
        {
            new GradientColorKey(new Color(0.851f, 0.851f, 0.851f), 0.0f),   // Day (#D9D9D9)
            new GradientColorKey(new Color(0.851f, 0.851f, 0.851f), 0.25f),
            new GradientColorKey(new Color(0.345f, 0.388f, 0.482f), 0.38f),  // Sunset transition
            new GradientColorKey(new Color(0.145f, 0.176f, 0.259f), 0.52f),  // Night dimmed navy
            new GradientColorKey(new Color(0.145f, 0.176f, 0.259f), 0.75f),
            new GradientColorKey(new Color(0.345f, 0.388f, 0.482f), 0.88f),  // Sunrise transition
            new GradientColorKey(new Color(0.851f, 0.851f, 0.851f), 1.0f)
        };
        g.alphaKeys = new GradientAlphaKey[]
        {
            new GradientAlphaKey(1f, 0f),
            new GradientAlphaKey(1f, 1f)
        };
        return g;
    }

    #endregion

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            distanceTraveled = Mathf.Repeat(startPhase, 1f) * cycleLengthUnits;
            if (targetCamera == null)
                targetCamera = Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();
            if (globalLight == null)
            {
                var lights = FindObjectsByType<Light2D>();
                for (int i = 0; i < lights.Length; i++)
                {
                    if (lights[i].lightType == Light2D.LightType.Global)
                    {
                        globalLight = lights[i];
                        break;
                    }
                }
            }
            ApplyPhase(CurrentPhase);
        }
    }
#endif
}
