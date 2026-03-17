using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// 유한 웨이브 기반 적 스포너.
/// SpawnLayoutData의 포인트 정의를 읽어 웨이브별 전멸 감지 후 다음 웨이브로 진행합니다.
/// 모든 웨이브 클리어 시 OnAllWavesCleared 이벤트를 발생시킵니다.
/// </summary>
public class StageWaveSpawner : MonoBehaviour
{
    public static StageWaveSpawner Instance { get; private set; }

    [Header("Pooling")]
    [SerializeField] private int poolDefaultCapacity = 8;
    [SerializeField] private int poolMaxSize = 64;

    // ── 상태 ──

    private SpawnLayoutData currentLayout;
    private int currentWaveIndex;
    private int totalWaveCount;
    private int aliveCount;
    private int spawnedCountInWave;
    private int totalSpawnCountInWave;
    private bool isRunning;

    private Transform playerTransform;
    private Coroutine spawnRoutine;
    private readonly Dictionary<int, List<SpawnPointDefinition>> pointsByWave = new();

    // ── 풀링 ──

    private readonly Dictionary<EnemyController, IObjectPool<EnemyController>> poolByInstance = new();
    private readonly Dictionary<EnemyController, IObjectPool<EnemyController>> poolByPrefab = new();

    // ── 프로퍼티 ──

    public int AliveCount => aliveCount;
    public int CurrentWaveIndex => currentWaveIndex;
    public int TotalWaveCount => totalWaveCount;
    public bool IsRunning => isRunning;

    // ── 이벤트 ──

    /// <summary>모든 웨이브가 클리어되었을 때 발생.</summary>
    public event Action OnAllWavesCleared;

    /// <summary>새 웨이브가 시작될 때 발생. (현재 웨이브 인덱스, 총 웨이브 수)</summary>
    public event Action<int, int> OnWaveStarted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>
    /// 플레이어 Transform을 수동으로 지정합니다.
    /// </summary>
    public void SetPlayerTransform(Transform player)
    {
        playerTransform = player;
    }

    /// <summary>
    /// 웨이브 스폰을 시작합니다.
    /// </summary>
    public void StartWaves(SpawnLayoutData layout)
    {
        if (layout == null || layout.points == null || layout.points.Length == 0)
        {
            Debug.LogWarning("[StageWaveSpawner] spawnLayout이 비어 있습니다.");
            return;
        }

        ResolvePlayerTransform();

        currentLayout = layout;
        currentWaveIndex = 0;
        aliveCount = 0;
        totalWaveCount = layout.TotalWaveCount;
        isRunning = true;

        if (totalWaveCount <= 0)
        {
            Debug.LogWarning("[StageWaveSpawner] 유효한 waveIndex가 없습니다.");
            isRunning = false;
            return;
        }

        Debug.Log("StartWaves");
        Debug.Log(currentWaveIndex);
        Debug.Log(totalWaveCount);

        CacheWavePoints(layout);
        StartWave(currentWaveIndex);
    }

    /// <summary>
    /// 현재 스폰을 중지하고 모든 살아있는 적을 풀로 반환합니다.
    /// </summary>
    public void StopAndClearAll()
    {
        isRunning = false;

        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }

        var toRelease = new List<EnemyController>(poolByInstance.Keys);
        foreach (var enemy in toRelease)
        {
            if (enemy != null && enemy.gameObject.activeSelf)
            {
                enemy.Deactivate();
            }
        }

        aliveCount = 0;
        currentWaveIndex = 0;
        spawnedCountInWave = 0;
        totalSpawnCountInWave = 0;
        currentLayout = null;
        totalWaveCount = 0;
        pointsByWave.Clear();
    }

    private void StartWave(int waveIndex)
    {
        if (currentLayout == null || waveIndex >= totalWaveCount)
            return;

        if (!pointsByWave.TryGetValue(waveIndex, out List<SpawnPointDefinition> wavePoints) || wavePoints.Count == 0)
        {
            AdvanceWave();
            return;
        }

        currentWaveIndex = waveIndex;
        spawnedCountInWave = 0;
        totalSpawnCountInWave = GetTotalSpawnCount(wavePoints);

        EnsurePools(wavePoints);

        OnWaveStarted?.Invoke(currentWaveIndex, totalWaveCount);

        if (spawnRoutine != null)
            StopCoroutine(spawnRoutine);
        spawnRoutine = StartCoroutine(SpawnWaveRoutine(wavePoints));
    }

    private IEnumerator SpawnWaveRoutine(List<SpawnPointDefinition> wavePoints)
    {
        for (int i = 0; i < wavePoints.Count; i++)
        {
            SpawnPointDefinition point = wavePoints[i];
            if (point == null || point.enemyPrefab == null || point.enemyData == null)
                continue;

            for (int spawnIndex = 0; spawnIndex < point.spawnCount; spawnIndex++)
            {
                if (!isRunning)
                    yield break;

                SpawnOne(point);
                spawnedCountInWave++;

                if (point.spawnInterval > 0f && spawnIndex < point.spawnCount - 1)
                    yield return new WaitForSeconds(point.spawnInterval);
            }
        }

        spawnRoutine = null;

        if (isRunning && aliveCount <= 0 && spawnedCountInWave >= totalSpawnCountInWave)
            AdvanceWave();
    }

    private void SpawnOne(SpawnPointDefinition point)
    {
        if (!poolByPrefab.TryGetValue(point.enemyPrefab, out IObjectPool<EnemyController> pool))
        {
            Debug.LogWarning("[StageWaveSpawner] 풀을 찾을 수 없습니다.");
            return;
        }

        EnemyController enemy = pool.Get();
        enemy.transform.SetPositionAndRotation(point.position, Quaternion.identity);
        poolByInstance[enemy] = pool;
        aliveCount++;

        enemy.Initialize(point.enemyData, playerTransform);
    }

    private void HandleEnemyDeactivated(EnemyController enemy)
    {
        if (enemy == null) return;

        aliveCount = Mathf.Max(0, aliveCount - 1);

        if (poolByInstance.TryGetValue(enemy, out var pool))
        {
            poolByInstance.Remove(enemy);
            pool.Release(enemy);
        }

        if (!isRunning) return;

        bool waveSpawnComplete = spawnRoutine == null && spawnedCountInWave >= totalSpawnCountInWave;
        if (waveSpawnComplete && aliveCount <= 0)
        {
            AdvanceWave();
        }
    }

    private void AdvanceWave()
    {
        int nextWave = currentWaveIndex + 1;

        if (currentLayout == null || nextWave >= totalWaveCount)
        {
            Debug.Log("AdvanceWave");
            Debug.Log(nextWave);
            Debug.Log(totalWaveCount);
            isRunning = false;
            OnAllWavesCleared?.Invoke();
            return;
        }
        Debug.Log("AdvanceWave 2");
        Debug.Log(nextWave);
        Debug.Log(totalWaveCount);

        StartWave(nextWave);
    }

    private void CacheWavePoints(SpawnLayoutData layout)
    {
        pointsByWave.Clear();

        for (int i = 0; i < layout.points.Length; i++)
        {
            SpawnPointDefinition point = layout.points[i];
            if (point == null)
                continue;

            if (!pointsByWave.TryGetValue(point.waveIndex, out List<SpawnPointDefinition> wavePoints))
            {
                wavePoints = new List<SpawnPointDefinition>();
                pointsByWave.Add(point.waveIndex, wavePoints);
            }

            wavePoints.Add(point);
        }
    }

    // ── 풀링 ──

    private void EnsurePools(List<SpawnPointDefinition> wavePoints)
    {
        for (int i = 0; i < wavePoints.Count; i++)
        {
            SpawnPointDefinition point = wavePoints[i];
            if (point == null || point.enemyPrefab == null)
                continue;
            if (poolByPrefab.ContainsKey(point.enemyPrefab))
                continue;

            EnemyController prefab = point.enemyPrefab;
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

            poolByPrefab[prefab] = pool;
        }
    }

    private static int GetTotalSpawnCount(List<SpawnPointDefinition> wavePoints)
    {
        int total = 0;

        for (int i = 0; i < wavePoints.Count; i++)
        {
            SpawnPointDefinition point = wavePoints[i];
            if (point == null || point.enemyPrefab == null || point.enemyData == null)
                continue;

            total += point.spawnCount;
        }

        return total;
    }

    private void ResolvePlayerTransform()
    {
        if (playerTransform != null) return;

        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
            playerTransform = player.transform;
    }
}
