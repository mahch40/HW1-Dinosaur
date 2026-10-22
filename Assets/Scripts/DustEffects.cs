using System;
using UnityEngine;

/// <summary>
/// Controls pixel-art dust particle effects for the player dino:
/// - Running dust: emits at feet while grounded and alive, scaling with world speed.
/// - Landing puff: outward burst along the ground when touching down after a jump.
/// - Jump puff: small burst at the feet upon takeoff.
/// </summary>
public class DustEffects : MonoBehaviour
{
    [Header("Component References")]
    [Tooltip("Reference to the PlayerController. Auto-detected if not set.")]
    [SerializeField] private PlayerController playerController;

    [Tooltip("Reference to PlayerHealth. Auto-detected if not set.")]
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Particle System References (Optional - auto-created if empty)")]
    [SerializeField] private ParticleSystem runDustSystem;
    [SerializeField] private ParticleSystem landDustSystem;
    [SerializeField] private ParticleSystem jumpDustSystem;

    [Header("Visual & Material Settings")]
    [Tooltip("Lit sprite material (Universal Render Pipeline/2D/Sprite-Lit-Default).")]
    [SerializeField] private Material dustMaterial;

    [Tooltip("Dust tint color. Mid-gray provides contrast on white day background, and brightens at night.")]
    [SerializeField] private Color dustColor = new Color(0.48f, 0.48f, 0.48f, 1f);

    [Tooltip("Sorting order for particle rendering. Dino is 10, pickups/obstacles 5.")]
    [SerializeField] private int sortingOrder = 9;

    [Tooltip("Local offset from the dino pivot (feet) where dust originates.")]
    [SerializeField] private Vector3 feetOffset = new Vector3(0f, 0.05f, 0f);

    [Header("Running Dust Settings")]
    [SerializeField] private float minRunSpeed = 6f;
    [SerializeField] private float maxRunSpeed = 14f;
    [SerializeField] private float minRunEmissionRate = 6f;
    [SerializeField] private float maxRunEmissionRate = 16f;
    [SerializeField] private float runParticleLifetime = 0.35f;
    [SerializeField] private float minRunParticleSize = 0.09f;
    [SerializeField] private float maxRunParticleSize = 0.14f;
    [SerializeField] private float runRiseSpeed = 0.35f;

    [Header("Landing Puff Settings")]
    [Range(4, 16)]
    [SerializeField] private int landingParticleCount = 8;
    [SerializeField] private float landingParticleLifetime = 0.4f;
    [SerializeField] private float minLandingParticleSize = 0.10f;
    [SerializeField] private float maxLandingParticleSize = 0.15f;
    [SerializeField] private float landingSpreadSpeed = 2.0f;
    [SerializeField] private float landingRiseSpeed = 0.5f;

    [Header("Jump Puff Settings")]
    [Range(2, 8)]
    [SerializeField] private int jumpParticleCount = 4;
    [SerializeField] private float jumpParticleLifetime = 0.35f;
    [SerializeField] private float minJumpParticleSize = 0.08f;
    [SerializeField] private float maxJumpParticleSize = 0.13f;
    [SerializeField] private float jumpRiseSpeed = 0.35f;

    [Header("Audio Settings")]
    [Tooltip("Sound played when landing and emitting dust puff.")]
    [SerializeField] private AudioClip landSound;

    private bool wasGrounded;
    private bool isInitialized;
    private ParticleSystem.EmitParams emitParams;

    private void Awake()
    {
        if (playerController == null)
            playerController = GetComponent<PlayerController>() ?? GetComponentInParent<PlayerController>();

        if (playerHealth == null)
            playerHealth = GetComponent<PlayerHealth>() ?? GetComponentInParent<PlayerHealth>();

        emitParams = new ParticleSystem.EmitParams();

        InitializeSystems();
    }

    private void Start()
    {
        if (playerController != null)
        {
            wasGrounded = playerController.IsGrounded;
            isInitialized = true;
        }
    }

    private void Update()
    {
        if (playerController == null) return;
        if (Time.timeScale <= 0f) return;

        bool isGrounded = playerController.IsGrounded;
        bool isAlive = playerHealth == null || !playerHealth.IsDead;

        if (!isInitialized)
        {
            wasGrounded = isGrounded;
            isInitialized = true;
        }

        // Detect grounded state transitions
        if (isAlive)
        {
            if (!wasGrounded && isGrounded)
            {
                TriggerLandingPuff();
            }
            else if (wasGrounded && !isGrounded)
            {
                TriggerJumpPuff();
            }
        }

        wasGrounded = isGrounded;

        // Running dust emission
        UpdateRunningDust(isGrounded && isAlive);
    }

    private void UpdateRunningDust(bool canRunDust)
    {
        if (runDustSystem == null) return;

        var emission = runDustSystem.emission;
        if (canRunDust && GameSpeed.Current > 0f)
        {
            float t = Mathf.InverseLerp(minRunSpeed, maxRunSpeed, GameSpeed.Current);
            float targetRate = Mathf.Lerp(minRunEmissionRate, maxRunEmissionRate, t);
            emission.rateOverTime = targetRate;

            var vel = runDustSystem.velocityOverLifetime;
            vel.x = new ParticleSystem.MinMaxCurve(-GameSpeed.Current);
            vel.y = new ParticleSystem.MinMaxCurve(runRiseSpeed);
            vel.z = new ParticleSystem.MinMaxCurve(0f);

            if (!runDustSystem.isPlaying)
            {
                runDustSystem.Play();
            }
        }
        else
        {
            emission.rateOverTime = 0f;
        }
    }

    private void TriggerLandingPuff()
    {
        // Play landing dust sound
        if (landSound != null)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySfx(landSound);
        }
        else if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayLand();
        }

        if (landDustSystem == null) return;

        Vector3 origin = transform.position + feetOffset;
        float scroll = GameSpeed.Current;

        for (int i = 0; i < landingParticleCount; i++)
        {
            float side = (i % 2 == 0) ? -1f : 1f;
            float spread = UnityEngine.Random.Range(0.6f, landingSpreadSpeed) * side;
            float vx = spread - scroll * 0.7f;
            float vy = UnityEngine.Random.Range(0.15f, landingRiseSpeed);

            emitParams.position = origin + new Vector3(UnityEngine.Random.Range(-0.15f, 0.15f), UnityEngine.Random.Range(0f, 0.04f), 0f);
            emitParams.velocity = new Vector3(vx, vy, 0f);
            emitParams.startSize = UnityEngine.Random.Range(minLandingParticleSize, maxLandingParticleSize);
            emitParams.startLifetime = UnityEngine.Random.Range(landingParticleLifetime * 0.85f, landingParticleLifetime * 1.15f);
            emitParams.startColor = dustColor;
            emitParams.applyShapeToPosition = false;

            landDustSystem.Emit(emitParams, 1);
        }
    }

    private void TriggerJumpPuff()
    {
        if (jumpDustSystem == null) return;

        Vector3 origin = transform.position + feetOffset;
        float scroll = GameSpeed.Current;

        for (int i = 0; i < jumpParticleCount; i++)
        {
            float vx = UnityEngine.Random.Range(-1.0f, 0.3f) - scroll * 0.8f;
            float vy = UnityEngine.Random.Range(0.1f, jumpRiseSpeed);

            emitParams.position = origin + new Vector3(UnityEngine.Random.Range(-0.1f, 0.1f), 0f, 0f);
            emitParams.velocity = new Vector3(vx, vy, 0f);
            emitParams.startSize = UnityEngine.Random.Range(minJumpParticleSize, maxJumpParticleSize);
            emitParams.startLifetime = UnityEngine.Random.Range(jumpParticleLifetime * 0.85f, jumpParticleLifetime * 1.15f);
            emitParams.startColor = dustColor;
            emitParams.applyShapeToPosition = false;

            jumpDustSystem.Emit(emitParams, 1);
        }
    }

    private void InitializeSystems()
    {
        SetupSystem("RunDust", ref runDustSystem, runParticleLifetime, minRunParticleSize, maxRunParticleSize, 30, true);
        SetupSystem("LandDust", ref landDustSystem, landingParticleLifetime, minLandingParticleSize, maxLandingParticleSize, 16, false);
        SetupSystem("JumpDust", ref jumpDustSystem, jumpParticleLifetime, minJumpParticleSize, maxJumpParticleSize, 10, false);
    }

    private void SetupSystem(string childName, ref ParticleSystem ps, float lifetime, float minSize, float maxSize, int maxParticles, bool looping)
    {
        if (ps == null)
        {
            Transform existing = transform.Find(childName);
            if (existing != null)
            {
                ps = existing.GetComponent<ParticleSystem>();
            }

            if (ps == null)
            {
                GameObject go = new GameObject(childName);
                go.transform.SetParent(transform, false);
                go.transform.localPosition = feetOffset;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;
                ps = go.AddComponent<ParticleSystem>();
            }
        }

        // Configure main module
        var main = ps.main;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.useUnscaledTime = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(lifetime * 0.85f, lifetime * 1.15f);
        main.startSize = new ParticleSystem.MinMaxCurve(minSize, maxSize);
        main.startSpeed = looping ? new ParticleSystem.MinMaxCurve(0.1f, 0.3f) : new ParticleSystem.MinMaxCurve(0f);
        main.startColor = dustColor;
        main.maxParticles = maxParticles;
        main.playOnAwake = false;
        main.loop = looping;

        // Configure emission module
        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f;

        // Configure shape module
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(0.35f, 0.05f, 0.05f);
        shape.position = Vector3.zero;

        // Configure velocity over lifetime module
        var vel = ps.velocityOverLifetime;
        if (looping)
        {
            vel.enabled = true;
            vel.space = ParticleSystemSimulationSpace.World;
            vel.x = new ParticleSystem.MinMaxCurve(-minRunSpeed);
            vel.y = new ParticleSystem.MinMaxCurve(runRiseSpeed);
            vel.z = new ParticleSystem.MinMaxCurve(0f);
        }
        else
        {
            vel.enabled = false;
        }

        // Configure color over lifetime module (alpha fadeout)
        var col = ps.colorOverLifetime;
        col.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.85f, 0.6f), new GradientAlphaKey(0f, 1f) }
        );
        col.color = grad;

        // Configure size over lifetime module (shrink at end)
        var sol = ps.sizeOverLifetime;
        sol.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1f);
        sizeCurve.AddKey(0.6f, 1f);
        sizeCurve.AddKey(1f, 0.35f);
        sol.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        // Configure renderer
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        if (renderer != null)
        {
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.alignment = ParticleSystemRenderSpace.View;
            renderer.sortingOrder = sortingOrder;

            if (dustMaterial != null)
            {
                renderer.sharedMaterial = dustMaterial;
            }
            else if (renderer.sharedMaterial == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default");
                if (shader != null)
                {
                    renderer.sharedMaterial = new Material(shader);
                }
            }
        }
    }
}
