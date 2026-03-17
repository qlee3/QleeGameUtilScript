using UnityEngine;

/// <summary>
/// SpawnLayoutData를 작성할 때 사용하는 스폰 포인트 마커.
/// </summary>
public class SpawnPoint : MonoBehaviour
{
    [Tooltip("이 스폰 포인트의 용도 (Normal: 일반 적, Boss: 보스 전용)")]
    [SerializeField] private SpawnPointType type = SpawnPointType.Normal;
    [SerializeField, Min(0)] private int waveIndex;
    [SerializeField] private EnemyController enemyPrefab;
    [SerializeField] private EnemyData enemyData;
    [SerializeField, Min(1)] private int spawnCount = 1;
    [SerializeField, Min(0f)] private float spawnInterval = 0.3f;

    public SpawnPointType Type => type;
    public Vector3 Position => transform.position;
    public int WaveIndex => waveIndex;
    public EnemyController EnemyPrefab => enemyPrefab;
    public EnemyData EnemyData => enemyData;
    public int SpawnCount => spawnCount;
    public float SpawnInterval => spawnInterval;

    public SpawnPointDefinition ToDefinition()
    {
        return new SpawnPointDefinition
        {
            position = transform.position,
            type = type,
            waveIndex = waveIndex,
            enemyPrefab = enemyPrefab,
            enemyData = enemyData,
            spawnCount = spawnCount,
            spawnInterval = spawnInterval,
        };
    }

    public void ApplyDefinition(SpawnPointDefinition definition)
    {
        if (definition == null)
            return;

        transform.position = definition.position;
        type = definition.type;
        waveIndex = definition.waveIndex;
        enemyPrefab = definition.enemyPrefab;
        enemyData = definition.enemyData;
        spawnCount = definition.spawnCount;
        spawnInterval = definition.spawnInterval;
    }

#if UNITY_EDITOR
    [Header("Gizmo")]
    [SerializeField] private float gizmoRadius = 0.5f;

    private void OnDrawGizmos()
    {
        Gizmos.color = type == SpawnPointType.Boss
            ? new Color(1f, 0.2f, 0.2f, 0.7f)
            : new Color(0.2f, 0.8f, 1f, 0.7f);
        Gizmos.DrawWireSphere(transform.position, gizmoRadius);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = type == SpawnPointType.Boss
            ? new Color(1f, 0.2f, 0.2f, 0.3f)
            : new Color(0.2f, 0.8f, 1f, 0.3f);
        Gizmos.DrawSphere(transform.position, gizmoRadius);
    }
#endif
}
