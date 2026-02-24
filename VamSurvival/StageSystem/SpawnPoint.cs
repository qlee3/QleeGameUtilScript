using UnityEngine;

/// <summary>
/// 맵 프리팹 내에 배치하는 적 스폰 위치 마커.
/// 빈 GameObject에 이 컴포넌트를 추가하면 StageWaveSpawner가 이 위치에 적을 스폰합니다.
/// </summary>
public class SpawnPoint : MonoBehaviour
{
    [Tooltip("이 스폰 포인트의 용도 (Normal: 일반 적, Boss: 보스 전용)")]
    [SerializeField] private SpawnPointType type = SpawnPointType.Normal;

    public SpawnPointType Type => type;
    public Vector3 Position => transform.position;
    public Quaternion Rotation => transform.rotation;

#if UNITY_EDITOR
    [Header("Gizmo")]
    [SerializeField] private float gizmoRadius = 0.5f;

    private void OnDrawGizmos()
    {
        Gizmos.color = type == SpawnPointType.Boss
            ? new Color(1f, 0.2f, 0.2f, 0.7f)
            : new Color(0.2f, 0.8f, 1f, 0.7f);
        Gizmos.DrawWireSphere(transform.position, gizmoRadius);
        Gizmos.DrawIcon(transform.position, "d_NavMeshAgent Icon", true);
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
