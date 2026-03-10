using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class Projectile : MonoBehaviour
{
    private Rigidbody2D rb;
    private int damage;
    private float maxDistance;
    private Vector3 startPosition;
    private bool isPlayerProjectile;
    private bool isActive;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;

        var col = GetComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.1f;
    }

    public void Init(Vector2 direction, int damage, float speed, float range, bool fromPlayer)
    {
        this.damage = damage;
        this.maxDistance = range;
        this.isPlayerProjectile = fromPlayer;
        this.startPosition = transform.position;
        this.isActive = true;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        rb.linearVelocity = direction.normalized * speed;

        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (!isActive) return;

        if (Vector3.Distance(startPosition, transform.position) >= maxDistance)
        {
            Deactivate();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;

        if (isPlayerProjectile && other.CompareTag("Enemy"))
        {
            var enemy = other.GetComponent<EnemyBase>();
            if (enemy != null)
                enemy.TakeDamage(damage);
            Deactivate();
        }
        else if (!isPlayerProjectile && other.CompareTag("Player"))
        {
            var health = other.GetComponent<PlayerHealth>();
            if (health != null)
                health.TakeDamage(damage);
            Deactivate();
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            Deactivate();
        }
    }

    private void Deactivate()
    {
        isActive = false;
        rb.linearVelocity = Vector2.zero;
        gameObject.SetActive(false);
    }
}
