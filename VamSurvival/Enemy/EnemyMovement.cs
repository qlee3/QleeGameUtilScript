using Pathfinding;
using UnityEngine;

/// <summary>
/// A* Pathfinding Project의 AIPath를 래핑하는 적 이동 컴포넌트.
/// FSM 상태에서는 이 모듈의 메서드만 호출하며, AIPath API를 직접 사용하지 않습니다.
/// 대쉬 등 특수 이동 시에는 AIPath를 비활성화하고 직접 위치를 제어합니다.
/// </summary>
[RequireComponent(typeof(AIPath))]
[RequireComponent(typeof(Seeker))]
public class EnemyMovement : MonoBehaviour
{
    private AIPath aiPath;
    private Transform chaseTarget;

    /// <summary>AIPath의 목적지 도달 여부.</summary>
    public bool HasReachedDestination => aiPath.reachedDestination;

    /// <summary>목적지까지 남은 거리.</summary>
    public float RemainingDistance => aiPath.remainingDistance;

    /// <summary>현재 AIPath가 활성화되어 있는지 여부.</summary>
    public bool IsAIPathActive => aiPath.enabled;

    private void Awake()
    {
        aiPath = GetComponent<AIPath>();
    }

    /// <summary>
    /// 이동 속도를 설정합니다.
    /// EnemyController.Initialize()에서 EnemyData.moveSpeed로 호출됩니다.
    /// </summary>
    public void Initialize(float moveSpeed)
    {
        aiPath.maxSpeed = moveSpeed;
    }

    // ────────────────────────────────────────
    //  일반 이동 (AIPath 기반)
    // ────────────────────────────────────────

    /// <summary>
    /// 지정된 타겟을 지속 추격합니다.
    /// Update에서 자동으로 타겟 위치를 destination에 반영합니다.
    /// </summary>
    public void ChaseTarget(Transform target)
    {
        chaseTarget = target;
        aiPath.isStopped = false;
    }

    /// <summary>
    /// 특정 월드 좌표로 이동합니다. 도달 여부는 HasReachedDestination으로 확인합니다.
    /// </summary>
    public void MoveTo(Vector3 position)
    {
        chaseTarget = null;
        aiPath.destination = position;
        aiPath.isStopped = false;
    }

    /// <summary>
    /// 이동을 멈춥니다. AIPath는 활성 상태를 유지하지만 정지합니다.
    /// </summary>
    public void Stop()
    {
        chaseTarget = null;
        aiPath.isStopped = true;
    }

    /// <summary>
    /// 정지 상태에서 이동을 재개합니다.
    /// </summary>
    public void Resume()
    {
        aiPath.isStopped = false;
    }

    /// <summary>
    /// AIPath의 이동 속도를 변경합니다.
    /// </summary>
    public void SetSpeed(float speed)
    {
        aiPath.maxSpeed = speed;
    }

    // ────────────────────────────────────────
    //  대쉬 모드 (AIPath 우회)
    // ────────────────────────────────────────

    /// <summary>
    /// AIPath를 활성화/비활성화합니다.
    /// 대쉬 시작 시 false로, 대쉬 종료 시 true로 호출합니다.
    /// 재활성화 시 현재 위치로 Teleport하여 AIPath 내부 상태를 동기화합니다.
    /// </summary>
    public void SetAIPathEnabled(bool enabled)
    {
        if (enabled && !aiPath.enabled)
        {
            aiPath.enabled = true;
            aiPath.Teleport(transform.position, true);
        }
        else if (!enabled)
        {
            chaseTarget = null;
            aiPath.enabled = false;
        }
    }

    /// <summary>
    /// 대쉬 상태에서 직접 위치를 제어할 때 사용합니다.
    /// AIPath가 비활성화된 상태에서만 호출하세요.
    /// </summary>
    public void SetPositionDirect(Vector3 position)
    {
        transform.position = position;
    }

    /// <summary>
    /// 대쉬 상태에서 이동 방향으로 회전합니다.
    /// </summary>
    public void SetRotation(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.001f) return;
        direction.y = 0f;
        transform.rotation = Quaternion.LookRotation(direction);
    }

    // ────────────────────────────────────────
    //  Update
    // ────────────────────────────────────────

    private void Update()
    {
        // 타겟 추격 모드: 매 프레임 destination 갱신
        if (chaseTarget != null && aiPath.enabled)
        {
            aiPath.destination = chaseTarget.position;
        }
    }
}
