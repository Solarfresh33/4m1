using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class EnemyBase : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int maxHealth = 30;
    [SerializeField] private int contactDamage = 10;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private int boltDropAmount = 5;

    private int currentHealth;
    private Rigidbody2D rb;
    private bool isDead;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public float MoveSpeed => moveSpeed;
    public bool IsDead => isDead;
    public int ContactDamage => contactDamage;

    public event Action<EnemyBase> OnEnemyDeath;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        currentHealth = maxHealth;

        gameObject.tag = "Enemy";
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        isDead = true;
        OnEnemyDeath?.Invoke(this);

        if (boltDropAmount > 0)
        {
            BoltPickup.SpawnBolts(transform.position, boltDropAmount);
        }

        Destroy(gameObject);
    }

    public void SetVelocity(Vector2 velocity)
    {
        if (rb != null && !isDead)
            rb.linearVelocity = velocity;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            var health = collision.gameObject.GetComponent<PlayerHealth>();
            if (health != null)
                health.TakeDamage(contactDamage);
        }
    }
}
