using UnityEngine;

[RequireComponent(typeof(EnemyBase))]
public class EnemyAI : MonoBehaviour
{
    public enum EnemyType
    {
        BasicMelee,
        BasicRanged,
        HeavyMelee
    }

    [Header("AI Settings")]
    [SerializeField] private EnemyType enemyType = EnemyType.BasicMelee;
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float attackRange = 5f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float attackCooldownVariance = 0.5f;
    [SerializeField] private int rangedDamage = 5;
    [SerializeField] private float projectileSpeed = 6f;

    private EnemyBase enemyBase;
    private Transform player;
    private float attackTimer;
    private float currentCooldown;

    private void Awake()
    {
        enemyBase = GetComponent<EnemyBase>();
        RandomizeCooldown();
    }

    private void Start()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    private void Update()
    {
        if (enemyBase.IsDead || player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer > detectionRange)
        {
            enemyBase.SetVelocity(Vector2.zero);
            return;
        }

        Vector2 directionToPlayer = ((Vector2)player.position - (Vector2)transform.position).normalized;

        switch (enemyType)
        {
            case EnemyType.BasicMelee:
            case EnemyType.HeavyMelee:
                MoveTowardPlayer(directionToPlayer);
                break;

            case EnemyType.BasicRanged:
                if (distanceToPlayer > attackRange * 0.6f)
                    MoveTowardPlayer(directionToPlayer);
                else
                    enemyBase.SetVelocity(Vector2.zero);
                break;
        }

        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0f && distanceToPlayer <= attackRange)
        {
            Attack(directionToPlayer);
            RandomizeCooldown();
            attackTimer = currentCooldown;
        }
    }

    private void MoveTowardPlayer(Vector2 direction)
    {
        enemyBase.SetVelocity(direction * enemyBase.MoveSpeed);
    }

    private void Attack(Vector2 direction)
    {
        if (enemyType == EnemyType.BasicRanged)
        {
            var projectile = ProjectilePool.Instance.GetProjectile();
            Vector3 spawnPos = transform.position + (Vector3)(direction * 0.5f);
            projectile.transform.position = spawnPos;
            projectile.Init(direction, rangedDamage, projectileSpeed, 10f, false);
        }
    }

    private void RandomizeCooldown()
    {
        currentCooldown = attackCooldown + Random.Range(-attackCooldownVariance, attackCooldownVariance);
    }
}
