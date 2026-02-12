using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// 시간 기반 웨이브 스포너.
/// EnemyWaveData를 읽어 플레이어 주변에 적을 스폰하고, 프리팹별 풀링으로 재사용합니다.
/// </summary>
public class EnemySpawnManager : MonoBehaviour
{
    public static EnemySpawnManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private EnemyWaveData waveData;
    [SerializeField] private Transform playerTransform;

    [Header("Spawn Area")]
    [SerializeField] private float minSpawnDistance = 12f;
    [SerializeField] private float maxSpawnDistance = 20f;
    [SerializeField] private int spawnPositionAttempts = 12;
    [SerializeField] private LayerMask blockedLayers;
    [SerializeField] private float blockedCheckRadius = 0.5f;

    [Header("Spawn Tick")]
    [Tooltip("한 프레임에서 처리할 최대 스폰 배치 수")]
    [SerializeField] private int maxSpawnBatchesPerTick = 2;

    [Header("Pooling")]
    [SerializeField] private int poolDefaultCapacity = 16;
    [SerializeField] private int poolMaxSize = 128;
    [SerializeField] private bool prewarmPools = true;
    [SerializeField] private int prewarmCountPerPrefab = 8;

    [Header("Runtime")]
    [SerializeField] private bool autoStartOnEnable = true;

    private float elapsedTime;
    private bool isRunning;

    private IObjectPool<EnemyController>[] pools;
    private int[] aliveCounts;
    private float[] nextSpawnAt;

    private readonly List<int> dueEntries = new();
    private readonly Dictionary<EnemyController, int> activeEntryByEnemy = new();
    private readonly Dictionary<EnemyController, int> activePoolByEnemy = new();
    private readonly Dictionary<EnemyController, int> poolIndexByPrefab = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        ResolvePlayerTransform();
        BuildPoolsAndState();
    }

    private void OnEnable()
    {
        if (autoStartOnEnable)
        {
            StartSpawning(true);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// 스폰을 시작합니다.
    /// </summary>
    /// <param name="resetTime">true면 경과 시간을 0으로 리셋합니다.</param>
    public void StartSpawning(bool resetTime)
    {
        if (waveData == null || waveData.Count == 0) return;

        if (resetTime)
        {
            elapsedTime = 0f;

            for (int i = 0; i < waveData.Count; i++)
            {
                aliveCounts[i] = 0;
                nextSpawnAt[i] = Mathf.Max(0f, waveData.GetEntry(i).startTime);
            }
        }

        isRunning = true;
    }

    /// <summary>
    /// 스폰을 중지합니다.
    /// </summary>
    public void StopSpawning()
    {
        isRunning = false;
    }

    /// <summary>
    /// 플레이어 Transform을 수동으로 지정합니다.
    /// </summary>
    public void SetPlayerTransform(Transform player)
    {
        playerTransform = player;
    }

    private void Update()
    {
        if (!isRunning) return;
        if (waveData == null || waveData.Count == 0) return;

        if (playerTransform == null)
        {
            ResolvePlayerTransform();
            if (playerTransform == null) return;
        }

        elapsedTime += Time.deltaTime;
        CollectDueEntries();

        int batches = Mathf.Min(maxSpawnBatchesPerTick, dueEntries.Count);
        for (int i = 0; i < batches; i++)
        {
            if (dueEntries.Count == 0) break;

            int entryIndex = PickEntryByWeight(dueEntries);
            TrySpawnBatch(entryIndex);

            // 같은 프레임에서 동일 엔트리를 반복 처리하지 않도록 제거.
            dueEntries.Remove(entryIndex);
        }
    }

    private void BuildPoolsAndState()
    {
        if (waveData == null || waveData.Count == 0)
        {
            Debug.LogWarning("[EnemySpawnManager] waveData가 비어 있습니다.");
            pools = System.Array.Empty<IObjectPool<EnemyController>>();
            aliveCounts = System.Array.Empty<int>();
            nextSpawnAt = System.Array.Empty<float>();
            return;
        }

        int entryCount = waveData.Count;
        aliveCounts = new int[entryCount];
        nextSpawnAt = new float[entryCount];

        for (int i = 0; i < entryCount; i++)
        {
            nextSpawnAt[i] = Mathf.Max(0f, waveData.GetEntry(i).startTime);
        }

        poolIndexByPrefab.Clear();
        var poolList = new List<IObjectPool<EnemyController>>();

        for (int i = 0; i < entryCount; i++)
        {
            EnemySpawnEntry entry = waveData.GetEntry(i);
            if (entry.enemyPrefab == null || entry.enemyData == null)
            {
                Debug.LogWarning($"[EnemySpawnManager] entries[{i}] prefab/data 참조가 비어 있습니다.");
                continue;
            }

            if (entry.endTime > 0f && entry.endTime < entry.startTime)
            {
                Debug.LogWarning($"[EnemySpawnManager] entries[{i}] endTime < startTime 입니다.");
            }

            if (poolIndexByPrefab.ContainsKey(entry.enemyPrefab))
            {
                continue;
            }

            EnemyController prefab = entry.enemyPrefab;
            int poolIndex = poolList.Count;

            var pool = new ObjectPool<EnemyController>(
                createFunc: () =>
                {
                    EnemyController enemy = Instantiate(prefab, transform);
                    enemy.gameObject.SetActive(false);
                    enemy.OnDeactivated += HandleEnemyDeactivated;
                    return enemy;
                },
                actionOnGet: enemy => enemy.gameObject.SetActive(true),
                actionOnRelease: enemy => enemy.gameObject.SetActive(false),
                actionOnDestroy: enemy =>
                {
                    if (enemy != null)
                    {
                        enemy.OnDeactivated -= HandleEnemyDeactivated;
                        Destroy(enemy.gameObject);
                    }
                },
                collectionCheck: false,
                defaultCapacity: poolDefaultCapacity,
                maxSize: poolMaxSize
            );

            poolList.Add(pool);
            poolIndexByPrefab.Add(prefab, poolIndex);

            if (prewarmPools && prewarmCountPerPrefab > 0)
            {
                for (int warm = 0; warm < prewarmCountPerPrefab; warm++)
                {
                    EnemyController enemy = pool.Get();
                    pool.Release(enemy);
                }
            }
        }

        pools = poolList.ToArray();
    }

    private void CollectDueEntries()
    {
        dueEntries.Clear();

        for (int i = 0; i < waveData.Count; i++)
        {
            EnemySpawnEntry entry = waveData.GetEntry(i);
            if (entry.enemyPrefab == null || entry.enemyData == null) continue;
            if (!poolIndexByPrefab.ContainsKey(entry.enemyPrefab)) continue;
            if (!IsEntryActive(entry)) continue;
            if (aliveCounts[i] >= entry.maxAlive) continue;
            if (elapsedTime < nextSpawnAt[i]) continue;

            dueEntries.Add(i);
        }
    }

    private bool IsEntryActive(EnemySpawnEntry entry)
    {
        if (elapsedTime < entry.startTime) return false;
        if (entry.endTime > 0f && elapsedTime > entry.endTime) return false;
        return true;
    }

    private int PickEntryByWeight(List<int> indices)
    {
        float totalWeight = 0f;

        for (int i = 0; i < indices.Count; i++)
        {
            EnemySpawnEntry entry = waveData.GetEntry(indices[i]);
            totalWeight += Mathf.Max(0f, entry.weight);
        }

        if (totalWeight <= 0f)
        {
            return indices[Random.Range(0, indices.Count)];
        }

        float roll = Random.value * totalWeight;
        float cumulative = 0f;

        for (int i = 0; i < indices.Count; i++)
        {
            EnemySpawnEntry entry = waveData.GetEntry(indices[i]);
            cumulative += Mathf.Max(0f, entry.weight);
            if (roll <= cumulative)
            {
                return indices[i];
            }
        }

        return indices[indices.Count - 1];
    }

    private void TrySpawnBatch(int entryIndex)
    {
        EnemySpawnEntry entry = waveData.GetEntry(entryIndex);
        float interval = Mathf.Max(0.05f, entry.spawnInterval);
        nextSpawnAt[entryIndex] = elapsedTime + interval;

        for (int i = 0; i < entry.countPerSpawn; i++)
        {
            if (aliveCounts[entryIndex] >= entry.maxAlive) break;

            if (!TryGetSpawnPosition(out Vector3 spawnPosition))
            {
                break;
            }

            SpawnOne(entryIndex, spawnPosition);
        }
    }

    private void SpawnOne(int entryIndex, Vector3 position)
    {
        EnemySpawnEntry entry = waveData.GetEntry(entryIndex);
        if (entry.enemyPrefab == null || entry.enemyData == null) return;

        if (!poolIndexByPrefab.TryGetValue(entry.enemyPrefab, out int poolIndex))
        {
            Debug.LogWarning("[EnemySpawnManager] prefab에 연결된 풀이 없습니다.");
            return;
        }

        if (poolIndex < 0 || poolIndex >= pools.Length) return;

        EnemyController enemy = pools[poolIndex].Get();
        enemy.transform.SetPositionAndRotation(position, Quaternion.identity);

        activeEntryByEnemy[enemy] = entryIndex;
        activePoolByEnemy[enemy] = poolIndex;
        aliveCounts[entryIndex]++;

        enemy.Initialize(entry.enemyData, playerTransform);
    }

    private void HandleEnemyDeactivated(EnemyController enemy)
    {
        if (enemy == null) return;

        if (activeEntryByEnemy.TryGetValue(enemy, out int entryIndex))
        {
            if (entryIndex >= 0 && entryIndex < aliveCounts.Length)
            {
                aliveCounts[entryIndex] = Mathf.Max(0, aliveCounts[entryIndex] - 1);
            }
            activeEntryByEnemy.Remove(enemy);
        }

        if (activePoolByEnemy.TryGetValue(enemy, out int poolIndex))
        {
            activePoolByEnemy.Remove(enemy);
            if (poolIndex >= 0 && poolIndex < pools.Length)
            {
                pools[poolIndex].Release(enemy);
            }
        }
    }

    private bool TryGetSpawnPosition(out Vector3 position)
    {
        position = Vector3.zero;
        if (playerTransform == null) return false;

        Vector3 center = playerTransform.position;
        float minDistance = Mathf.Min(minSpawnDistance, maxSpawnDistance);
        float maxDistance = Mathf.Max(minSpawnDistance, maxSpawnDistance);

        for (int i = 0; i < spawnPositionAttempts; i++)
        {
            Vector2 direction2D = Random.insideUnitCircle;
            if (direction2D.sqrMagnitude < 0.001f) continue;
            direction2D.Normalize();

            float distance = Random.Range(minDistance, maxDistance);
            Vector3 candidate = center + new Vector3(direction2D.x, 0f, direction2D.y) * distance;
            candidate.y = center.y;

            if (blockedLayers.value != 0)
            {
                bool blocked = Physics.CheckSphere(
                    candidate,
                    blockedCheckRadius,
                    blockedLayers,
                    QueryTriggerInteraction.Ignore
                );
                if (blocked) continue;
            }

            position = candidate;
            return true;
        }

        return false;
    }

    private void ResolvePlayerTransform()
    {
        if (playerTransform != null) return;

        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }
}
