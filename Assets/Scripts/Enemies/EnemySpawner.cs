using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject basicMeleePrefab;
    [SerializeField] private GameObject basicRangedPrefab;
    [SerializeField] private GameObject heavyMeleePrefab;
    [SerializeField] private GameObject bossPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private int baseEnemyCount = 4;
    [SerializeField] private int enemiesPerFloor = 2;

    public static EnemySpawner Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public List<EnemyBase> SpawnEnemiesInRoom(Room room, int floorNumber)
    {
        var enemies = new List<EnemyBase>();
        int enemyCount = baseEnemyCount + (floorNumber - 1) * enemiesPerFloor;
        Bounds bounds = room.GetSpawnBounds();

        for (int i = 0; i < enemyCount; i++)
        {
            Vector3 spawnPos = new Vector3(
                Random.Range(bounds.min.x + 1f, bounds.max.x - 1f),
                Random.Range(bounds.min.y + 1f, bounds.max.y - 1f),
                0f
            );

            GameObject prefab = ChooseEnemyPrefab(floorNumber);
            if (prefab == null) continue;

            var enemyGo = Instantiate(prefab, spawnPos, Quaternion.identity, room.transform);
            var enemy = enemyGo.GetComponent<EnemyBase>();
            if (enemy != null)
                enemies.Add(enemy);
        }

        return enemies;
    }

    public EnemyBase SpawnBoss(Room room, int floorNumber)
    {
        if (bossPrefab == null) return null;

        Vector3 spawnPos = room.transform.position + Vector3.up * 2f;
        var bossGo = Instantiate(bossPrefab, spawnPos, Quaternion.identity, room.transform);
        return bossGo.GetComponent<EnemyBase>();
    }

    private GameObject ChooseEnemyPrefab(int floorNumber)
    {
        float roll = Random.value;

        if (floorNumber >= 3 && roll < 0.2f && heavyMeleePrefab != null)
            return heavyMeleePrefab;
        if (roll < 0.5f && basicRangedPrefab != null)
            return basicRangedPrefab;
        return basicMeleePrefab;
    }
}
