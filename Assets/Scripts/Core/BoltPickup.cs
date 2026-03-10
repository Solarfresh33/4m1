using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class BoltPickup : MonoBehaviour
{
    [SerializeField] private int boltValue = 1;
    [SerializeField] private float magnetRange = 2f;
    [SerializeField] private float magnetSpeed = 8f;

    private Transform player;
    private bool isBeingCollected;

    private void Awake()
    {
        var col = GetComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.3f;

        var sr = GetComponent<SpriteRenderer>();
        if (sr == null)
            sr = gameObject.AddComponent<SpriteRenderer>();
        sr.color = new Color(0.8f, 0.6f, 0.1f, 1f);
        sr.sortingOrder = 3;
    }

    private void Start()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    private void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance <= magnetRange)
        {
            isBeingCollected = true;
            transform.position = Vector3.MoveTowards(
                transform.position,
                player.position,
                magnetSpeed * Time.deltaTime
            );
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
                GameManager.Instance.AddBolts(boltValue);
            Destroy(gameObject);
        }
    }

    public static void SpawnBolts(Vector3 position, int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            var boltGo = new GameObject("Bolt");
            boltGo.transform.position = position + (Vector3)(Random.insideUnitCircle * 0.5f);

            boltGo.AddComponent<SpriteRenderer>();
            boltGo.AddComponent<CircleCollider2D>();
            var bolt = boltGo.AddComponent<BoltPickup>();
        }
    }
}
