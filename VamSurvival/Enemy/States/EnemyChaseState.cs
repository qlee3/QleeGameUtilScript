using UnityEngine;

/// <summary>
/// 적 추격 상태.
/// A* Pathfinding으로 플레이어를 지속 추격합니다.
/// 추격형(Chaser)은 일정 시간 후 간헐적으로 IdleState(정지)로 전환됩니다.
/// 대쉬형(Dasher)은 사거리 내 진입 시 대쉬 준비 상태로 전환됩니다.
/// </summary>
public class EnemyChaseState : EntityState<EnemyController>
{
    public override int AnimStateId => 1; // Move 애니메이션

    /// <summary>추격형: 다음 정지까지 남은 시간.</summary>
    private float nextPauseTime;

    protected override void OnEnter(EnemyController entity)
    {
        // 플레이어를 추격 타겟으로 설정
        entity.Movement.ChaseTarget(entity.Detection.PlayerTransform);

        // 추격형: 랜덤 정지 타이머 설정
        if (entity.Data.enemyType == EnemyType.Chaser)
        {
            nextPauseTime = Random.Range(entity.Data.chaseMinDuration, entity.Data.chaseMaxDuration);
        }
    }

    protected override void OnStep(EnemyController entity)
    {
        switch (entity.Data.enemyType)
        {
            case EnemyType.Chaser:
                StepChaser(entity);
                break;

            case EnemyType.Dasher:
                StepDasher(entity);
                break;

            // 향후 다른 타입은 여기에 추가하거나 별도 상태로 분리
            default:
                // 기본: 단순 추격
                break;
        }
    }

    protected override void OnExit(EnemyController entity)
    {
        entity.Movement.Stop();
    }

    // ── 추격형 로직 ──

    /// <summary>
    /// 추격형: 일정 시간 추격 후 잠깐 멈춤.
    /// </summary>
    private void StepChaser(EnemyController entity)
    {
        if (TimeSinceEntered >= nextPauseTime)
        {
            // 잠깐 정지 → IdleState
            entity.IdleState.SetDuration(entity.Data.chasePauseDuration);
            entity.ChangeState(entity.IdleState);
        }
    }

    // ── 대쉬형 로직 ──

    /// <summary>
    /// 대쉬형: 사거리 내 진입 시 대쉬 준비.
    /// (향후 DashPrepareState 추가 시 연결)
    /// </summary>
    private void StepDasher(EnemyController entity)
    {
        if (entity.Detection.IsPlayerInRange(entity.Data.dashTriggerRange))
        {
            // TODO: DashPrepareState로 전환 (Phase 2에서 구현)
            // entity.ChangeState(entity.DashPrepareState);
        }
    }
}
