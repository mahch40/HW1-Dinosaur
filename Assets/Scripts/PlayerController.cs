using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Jump")]
    [SerializeField] private float jumpForce = 11f;

    [Header("Ground check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 18f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private float returnSpeed = 6f;

    private Rigidbody2D rb;
    private Animator anim;
    private GameInput input;

    private bool isGrounded;
    private bool isDashing;
    private float dashTimer;
    private float cooldownTimer;
    private float homeX;

    public bool IsGrounded => isGrounded;
    public bool IsDashing => isDashing;
    public PlayerHealth Health { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        Health = GetComponent<PlayerHealth>();
        input = new GameInput();
        homeX = transform.position.x;
    }

    private void OnEnable()
    {
        input.Gameplay.Enable();
        input.Gameplay.Jump.performed += OnJump;
        input.Gameplay.Dash.performed += OnDash;
    }

    private void OnDisable()
    {
        input.Gameplay.Jump.performed -= OnJump;
        input.Gameplay.Dash.performed -= OnDash;
        input.Gameplay.Disable();
    }

    private void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        anim.SetBool("isGrounded", isGrounded);

        if (cooldownTimer > 0f)
            cooldownTimer -= Time.fixedDeltaTime;

        float vx = 0f;

        if (isDashing)
        {
            vx = dashSpeed;
            dashTimer -= Time.fixedDeltaTime;
            if (dashTimer <= 0f)
                isDashing = false;
        }
        else if (rb.position.x > homeX + 0.01f)
        {
            // drift back to the starting position without overshooting it
            float distance = rb.position.x - homeX;
            vx = -Mathf.Min(returnSpeed, distance / Time.fixedDeltaTime);
        }

        rb.linearVelocity = new Vector2(vx, rb.linearVelocity.y);
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        if (!isGrounded) return;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    private void OnDash(InputAction.CallbackContext ctx)
    {
        if (isDashing || cooldownTimer > 0f) return;

        isDashing = true;
        dashTimer = dashDuration;
        cooldownTimer = dashCooldown;
    }
}