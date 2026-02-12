using UnityEngine;

/// <summary>
/// 플레이어 감지 유틸리티 컴포넌트.
/// 플레이어와의 거리, 방향 등의 정보를 제공합니다.
/// FSM 상태에서 전환 조건 판단에 사용합니다.
/// </summary>
public class EnemyDetection : MonoBehaviour
{
    private Transform playerTransform;

    /// <summary>플레이어까지의 거리.</summary>
    public float DistanceToPlayer { get; private set; }

    /// <summary>플레이어를 향한 방향 벡터 (정규화, XZ 평면).</summary>
    public Vector3 DirectionToPlayer { get; private set; }

    /// <summary>플레이어 Transform 참조. 초기화 시 설정.</summary>
    public Transform PlayerTransform => playerTransform;

    /// <summary>
    /// 추적할 플레이어 Transform을 설정합니다.
    /// EnemyController.Initialize()에서 호출됩니다.
    /// </summary>
    public void Initialize(Transform player)
    {
        playerTransform = player;
    }

    private void Update()
    {
        if (playerTransform == null) return;

        Vector3 toPlayer = playerTransform.position - transform.position;
        toPlayer.y = 0f; // XZ 평면

        DistanceToPlayer = toPlayer.magnitude;
        DirectionToPlayer = DistanceToPlayer > 0.001f ? toPlayer / DistanceToPlayer : Vector3.zero;
    }

    /// <summary>
    /// 플레이어가 지정된 범위 이내에 있는지 확인합니다.
    /// </summary>
    public bool IsPlayerInRange(float range)
    {
        return playerTransform != null && DistanceToPlayer <= range;
    }
}
