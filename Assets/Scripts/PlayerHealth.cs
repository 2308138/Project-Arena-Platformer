using System;
using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] public int maxHealth = 0;
    [SerializeField] private int currentHealth;

    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("UI Settings")]
    [SerializeField] public HealthUI healthUI;
    [SerializeField] public static event Action OnPlayerDied;

    private void Start()
    {
        ResetHealth();
        spriteRenderer = GetComponent<SpriteRenderer>();

        GameController.OnReset += ResetHealth;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Enemy enemy = collision.GetComponent<Enemy>();
        if (enemy)
            TakeDamage(enemy.damage);
    }

    private void TakeDamage(int damage)
    {
        currentHealth -= damage;
        healthUI.UpdateHearts(currentHealth);

        StartCoroutine(FlashRed());

        if (currentHealth <= 0)
        {
            OnPlayerDied.Invoke();
        }
    }

    private IEnumerator FlashRed()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.2F);
        spriteRenderer.color = Color.white;
    }

    void ResetHealth()
    {
        currentHealth = maxHealth;
        healthUI.SetMaxHearts(maxHealth);
    }
}