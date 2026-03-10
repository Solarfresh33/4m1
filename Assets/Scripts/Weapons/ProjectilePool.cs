using UnityEngine;
using System.Collections.Generic;

public class ProjectilePool : MonoBehaviour
{
    public static ProjectilePool Instance { get; private set; }

    [SerializeField] private int poolSize = 30;
    [SerializeField] private Sprite projectileSprite;

    private List<Projectile> pool = new List<Projectile>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        for (int i = 0; i < poolSize; i++)
        {
            pool.Add(CreateProjectile());
        }
    }

    public Projectile GetProjectile()
    {
        foreach (var p in pool)
        {
            if (!p.gameObject.activeInHierarchy)
                return p;
        }

        var newP = CreateProjectile();
        pool.Add(newP);
        return newP;
    }

    private Projectile CreateProjectile()
    {
        var go = new GameObject("Projectile");
        go.transform.SetParent(transform);
        go.layer = LayerMask.NameToLayer("Default");

        var sr = go.AddComponent<SpriteRenderer>();
        if (projectileSprite != null)
            sr.sprite = projectileSprite;
        sr.sortingOrder = 5;

        go.AddComponent<Rigidbody2D>();
        go.AddComponent<CircleCollider2D>();
        var projectile = go.AddComponent<Projectile>();

        go.SetActive(false);
        return projectile;
    }
}
