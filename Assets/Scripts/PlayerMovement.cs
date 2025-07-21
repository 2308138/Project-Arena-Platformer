using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Misc Settings")]
    [SerializeField] public Rigidbody2D playerRB;
    [SerializeField] public ParticleSystem smokeFX;
    [SerializeField] private bool isFacingRight = true;

    [Header("Movement Settings")]
    [SerializeField] public float moveSpeed = 0F;

    [Header("Jump Settings")]
    [SerializeField] public float jumpPower = 0F;
    [SerializeField] public int maxJumps = 0;
    [SerializeField] private int jumpsRemaining;

    [Header("Ground Check Settings")]
    [SerializeField] public Transform groundCheckPosition;
    [SerializeField] public Vector2 groundCheckSize = new Vector2(0F, 0F);
    [SerializeField] public LayerMask groundLayer;
    [SerializeField] private bool isGrounded;

    [Header("Gravity Settings")]
    [SerializeField] public float baseGravity = 0F;
    [SerializeField] public float maxFallSpeed = 0F;
    [SerializeField] public float fallSpeedMultiplier = 0F;

    [Header("Wall Check Settings")]
    [SerializeField] public Transform wallCheckPosition;
    [SerializeField] public Vector2 wallCheckSize = new Vector2(0F, 0F);
    [SerializeField] public LayerMask wallLayer;

    [Header("Wall Movement Settings")]
    [SerializeField] public float wallSlideSpeed = 0F;
    [SerializeField] private bool isWallSliding;

    [SerializeField] public Vector2 wallJumpPower = new Vector2(0F, 0F);
    [SerializeField] public float wallJumpTime = 0F;
    [SerializeField] private float wallJumpDirection;
    [SerializeField] private float wallJumpTimer;
    [SerializeField] private bool isWallJumping;

    [Header("Dash Settings")]
    [SerializeField] public float dashSpeed = 0F;
    [SerializeField] public float dashDuration = 0F;
    [SerializeField] public float dashCooldown = 0F;
    [SerializeField] private bool isDashing = false;
    [SerializeField] private bool canDash = true;
    [SerializeField] private TrailRenderer trailRenderer;

    [SerializeField][HideInInspector] private float horizontalMovement;

    private void Start()
    {
        trailRenderer = GetComponent<TrailRenderer>();
    }

    private void Update()
    {
        if (isDashing)
            return;
        
        GroundCheck();
        Gravity();
        WallSlide();
        WallJump();

        if (!isWallJumping)
        {
            playerRB.linearVelocity = new Vector2(horizontalMovement * moveSpeed, playerRB.linearVelocity.y);
            Flip();
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        horizontalMovement = context.ReadValue<Vector2>().x;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (jumpsRemaining > 0)
        {
            if (context.performed)
            {
                playerRB.linearVelocity = new Vector2(playerRB.linearVelocity.x, jumpPower);
                jumpsRemaining--;
                JumpFX();
            }
                
            else if (context.canceled && playerRB.linearVelocity.y > 0F)
            {
                playerRB.linearVelocity = new Vector2(playerRB.linearVelocity.x, playerRB.linearVelocity.y * 0.25F);
                jumpsRemaining--;
                JumpFX();
            } 
        }

        if (context.performed && wallJumpTimer > 0F)
        {
            isWallJumping = true;
            playerRB.linearVelocity = new Vector2(wallJumpDirection * wallJumpPower.x, wallJumpPower.y);
            wallJumpTimer = 0F;
            JumpFX();

            if (transform.localScale.x != wallJumpDirection)
            {
                isFacingRight = !isFacingRight;
                Vector3 ls = transform.localScale;
                ls.x *= -1F;
                transform.localScale = ls;
            }

            Invoke(nameof(CancelWallJump), wallJumpTime + 0.1F);
        }
    }

    private void JumpFX()
    {
        smokeFX.Play();
    }

    private void GroundCheck()
    {
        if (Physics2D.OverlapBox(groundCheckPosition.position, groundCheckSize, 0, groundLayer))
        {
            jumpsRemaining = maxJumps;
            isGrounded = true;
        }
        else
            isGrounded = false;
            
    }

    private bool WallCheck()
    {
        return Physics2D.OverlapBox(wallCheckPosition.position, wallCheckSize, 0, wallLayer);
    }

    private void Gravity()
    {
        if (playerRB.linearVelocity.y < 0F)
        {
            playerRB.gravityScale = baseGravity * fallSpeedMultiplier;
            playerRB.linearVelocity = new Vector2(playerRB.linearVelocity.x, Mathf.Max(playerRB.linearVelocity.y, -maxFallSpeed));
        }
        else
            playerRB.gravityScale = baseGravity;
    }

    private void Flip()
    {
        if (isFacingRight && horizontalMovement < 0F || !isFacingRight && horizontalMovement > 0F)
        {
            isFacingRight = !isFacingRight;
            Vector3 ls = transform.localScale;
            ls.x *= -1F;
            transform.localScale = ls;

            if (playerRB.linearVelocity.y == 0)
                smokeFX.Play();
        }
    }

    private void WallSlide()
    {
        if (!isGrounded & WallCheck() && horizontalMovement != 0F)
        {
            isWallSliding = true;
            playerRB.linearVelocity = new Vector2(playerRB.linearVelocity.x, Mathf.Max(playerRB.linearVelocity.y, -wallSlideSpeed));
        }
        else
            isWallSliding = false;
    }

    private void WallJump()
    {
        if (isWallSliding)
        {
            isWallJumping = false;
            wallJumpDirection = -transform.localScale.x;
            wallJumpTimer = wallJumpTime;

            CancelInvoke(nameof(CancelWallJump));
        }
        else if (wallJumpTimer > 0F)
        {
            wallJumpTimer -= Time.deltaTime;
        }
    }

    private void CancelWallJump()
    {
        isWallJumping = false;
    }

    public void Dash(InputAction.CallbackContext context)
    {
        if(context.performed && canDash)
        {
            StartCoroutine(DashCoroutine());
        }
    }

    private IEnumerator DashCoroutine()
    {
        canDash = false;
        isDashing = true;
        trailRenderer.emitting = true;
        float dashDirection = isFacingRight ? 1F : -1F;
        playerRB.linearVelocity = new Vector2(dashDirection * dashSpeed, playerRB.linearVelocity.y);

        yield return new WaitForSeconds(dashDuration);
        playerRB.linearVelocity = new Vector2(0F, playerRB.linearVelocity.y);
        isDashing = false;
        trailRenderer.emitting = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(groundCheckPosition.position, groundCheckSize);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(wallCheckPosition.position, wallCheckSize);
    }
}