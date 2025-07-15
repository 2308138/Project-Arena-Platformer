using JetBrains.Annotations;
using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] public float chaseSpeed = 0F;
    [SerializeField] public float jumpForce = 0F;
    [SerializeField] public LayerMask groundLayer;

    [SerializeField] public int damage;

    [SerializeField] public int maxHealth = 0;

    [SerializeField] private Transform player;
    [SerializeField] private Rigidbody2D enemyRB;
    [SerializeField] private bool isGrounded;
    [SerializeField] private bool shouldJump;

    [SerializeField] private int currentHealth;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color originalColor;

    private void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<Transform>();
        enemyRB = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        maxHealth = currentHealth;
        originalColor = spriteRenderer.color;
    }

    private void Update()
    {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, 1F, groundLayer);

        float direction = Mathf.Sign(player.position.x - transform.position.x);

        bool isPlayerAbove = Physics2D.Raycast(transform.position, Vector2.up, 5F, 1 << player.gameObject.layer);

        if (isGrounded)
        {
            enemyRB.linearVelocity = new Vector2(direction * chaseSpeed, enemyRB.linearVelocity.y);

            RaycastHit2D groundInFront = Physics2D.Raycast(transform.position, new Vector2(direction, 0F), 2F, groundLayer);
            RaycastHit2D gapAhead = Physics2D.Raycast(transform.position + new Vector3(direction, 0F, 0F), Vector2.down, 2F, groundLayer);
            RaycastHit2D platformAbove = Physics2D.Raycast(transform.position, Vector2.up, 5F, groundLayer);

            if (!groundInFront.collider && !gapAhead.collider)
                shouldJump = true;
            else if (isPlayerAbove && platformAbove.collider)
                shouldJump = true;
        }
    }

    private void FixedUpdate()
    {
        if (isGrounded && shouldJump)
        {
            shouldJump = false;
            Vector2 direction = (player.position - transform.position).normalized;

            Vector2 jumpDirection = direction * jumpForce;

            enemyRB.AddForce(new Vector2(jumpDirection.x, jumpForce), ForceMode2D.Impulse);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        StartCoroutine(FlashWhite());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator FlashWhite()
    {
        spriteRenderer.color = Color.white;
        yield return new WaitForSeconds(0.2F);
        spriteRenderer.color = originalColor;
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}