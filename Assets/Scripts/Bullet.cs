using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] public int bulletDamage = 0;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Enemy enemy = collision.GetComponent<Enemy>();

        if (enemy)
        {
            enemy.TakeDamage(bulletDamage);
            Destroy(gameObject);
        }
    }
}