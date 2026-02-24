using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// 유한 웨이브 기반 적 스포너.
/// SpawnPoint 위치에 적을 배치하며, 웨이브별 전멸 감지 후 다음 웨이브로 진행합니다.
/// 모든 웨이브 클리어 시 OnAllWavesCleared 이벤트를 발생시킵니다.
/// </summary>
public class StageWaveSpawner : MonoBehaviour
{
    public static StageWaveSpawner Instance { get; private set; }

    [Header("Pooling")]
    [SerializeField] private int poolDefaultCapacity = 8;
    [SerializeField] private int poolMaxSize = 64;

    // ── 상태 ──

    private WaveConfig[] currentWaves;
    private int currentWaveIndex;
    private int aliveCount;
    private int spawnedCountInWave;
    private int totalSpawnCountInWave;
    private bool isRunning;

    private IReadOnlyList<SpawnPoint> spawnPoints;
    private Transform playerTransform;

    private Coroutine spawnRoutine;

    // ── 풀링 ──

    private readonly Dictionary<EnemyController, IObjectPool<EnemyController>> poolByInstance = new();
    private readonly Dictionary<EnemyController, IObjectPool<EnemyController>> poolByPrefab = new();

    // ── 프로퍼티 ──

    public int AliveCount => aliveCount;
    public int CurrentWaveIndex => currentWaveIndex;
    public int TotalWaveCount => currentWaves != null ? currentWaves.Length : 0;
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
    /// SpawnPoint 목록을 설정합니다. MapManager가 맵 로드 후 호출합니다.
    /// </summary>
    public void SetSpawnPoints(IReadOnlyList<SpawnPoint> points)
    {
        spawnPoints = points;
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
    public void StartWaves(WaveConfig[] waves)
    {
        if (waves == null || waves.Length == 0)
        {
            Debug.LogWarning("[StageWaveSpawner] waves가 비어 있습니다.");
            return;
        }

        ResolvePlayerTransform();

        currentWaves = waves;
        currentWaveIndex = 0;
        aliveCount = 0;
        isRunning = true;

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
        currentWaves = null;
    }

    private void StartWave(int waveIndex)
    {
        if (currentWaves == null || waveIndex >= currentWaves.Length) return;

        currentWaveIndex = waveIndex;
        WaveConfig wave = currentWaves[waveIndex];
        spawnedCountInWave = 0;
        totalSpawnCountInWave = wave.TotalSpawnCount;

        EnsurePools(wave);

        OnWaveStarted?.Invoke(currentWaveIndex, currentWaves.Length);

        if (spawnRoutine != null)
            StopCoroutine(spawnRoutine);
        spawnRoutine = StartCoroutine(SpawnWaveRoutine(wave));
    }

    private IEnumerator SpawnWaveRoutine(WaveConfig wave)
    {
        if (wave.delayBeforeStart > 0f)
            yield return new WaitForSeconds(wave.delayBeforeStart);

        if (wave.groups == null) yield break;

        for (int g = 0; g < wave.groups.Length; g++)
        {
            WaveGroup group = wave.groups[g];
            if (group.enemyPrefab == null || group.enemyData == null) continue;

            for (int i = 0; i < group.spawnCount; i++)
            {
                if (!isRunning) yield break;

                Vector3 position = PickSpawnPosition(group);
                SpawnOne(group, position);
                spawnedCountInWave++;

                if (group.spawnInterval > 0f && i < group.spawnCount - 1)
                    yield return new WaitForSeconds(group.spawnInterval);
            }
        }

        spawnRoutine = null;
    }

    private void SpawnOne(WaveGroup group, Vector3 position)
    {
        if (!poolByPrefab.TryGetValue(group.enemyPrefab, out var pool))
        {
            Debug.LogWarning("[StageWaveSpawner] 풀을 찾을 수 없습니다.");
            return;
        }

        EnemyController enemy = pool.Get();
        enemy.transform.SetPositionAndRotation(position, Quaternion.identity);
        poolByInstance[enemy] = pool;
        aliveCount++;

        enemy.Initialize(group.enemyData, playerTransform);
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

        if (currentWaves == null || nextWave >= currentWaves.Length)
        {
            isRunning = false;
            OnAllWavesCleared?.Invoke();
            return;
        }

        StartWave(nextWave);
    }

    private Vector3 PickSpawnPosition(WaveGroup group)
    {
        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            Debug.LogWarning("[StageWaveSpawner] SpawnPoint가 없습니다. 원점을 사용합니다.");
            return Vector3.zero;
        }

        var filtered = ListPool<SpawnPoint>.Get();
        try
        {
            for (int i = 0; i < spawnPoints.Count; i++)
            {
                filtered.Add(spawnPoints[i]);
            }

            if (filtered.Count == 0)
            {
                for (int i = 0; i < spawnPoints.Count; i++)
                    filtered.Add(spawnPoints[i]);
            }

            SpawnPoint chosen = filtered[UnityEngine.Random.Range(0, filtered.Count)];
            return chosen.Position;
        }
        finally
        {
            ListPool<SpawnPoint>.Release(filtered);
        }
    }

    // ── 풀링 ──

    private void EnsurePools(WaveConfig wave)
    {
        if (wave.groups == null) return;

        for (int i = 0; i < wave.groups.Length; i++)
        {
            WaveGroup group = wave.groups[i];
            if (group.enemyPrefab == null) continue;
            if (poolByPrefab.ContainsKey(group.enemyPrefab)) continue;

            EnemyController prefab = group.enemyPrefab;
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

    private void ResolvePlayerTransform()
    {
        if (playerTransform != null) return;

        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
            playerTransform = player.transform;
    }
}
