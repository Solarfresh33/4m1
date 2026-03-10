using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 1f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 lastMoveDirection = Vector2.down;

    private bool isDashing;
    private float dashTimer;
    private float dashCooldownTimer;
    private Vector2 dashDirection;

    private PlayerHealth playerHealth;
    private bool inputEnabled = true;

    public Vector2 LastMoveDirection => lastMoveDirection;
    public bool IsDashing => isDashing;
    public Vector2 MoveInput => moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerHealth = GetComponent<PlayerHealth>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void FixedUpdate()
    {
        if (!inputEnabled || (playerHealth != null && playerHealth.IsDead))
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (isDashing)
        {
            rb.linearVelocity = dashDirection * dashSpeed;
            dashTimer -= Time.fixedDeltaTime;
            if (dashTimer <= 0f)
            {
                isDashing = false;
            }
            return;
        }

        rb.linearVelocity = moveInput * moveSpeed;
    }

    private void Update()
    {
        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.deltaTime;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        if (moveInput.sqrMagnitude > 0.01f)
        {
            lastMoveDirection = moveInput.normalized;
        }
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (isDashing || dashCooldownTimer > 0f) return;
        if (moveInput.sqrMagnitude < 0.01f) return;

        isDashing = true;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;
        dashDirection = moveInput.normalized;

        if (playerHealth != null)
            playerHealth.SetInvincible(dashDuration);
    }

    public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled;
        if (!enabled)
        {
            moveInput = Vector2.zero;
            rb.linearVelocity = Vector2.zero;
        }
    }
}
