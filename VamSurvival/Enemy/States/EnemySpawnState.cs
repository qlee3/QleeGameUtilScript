/// <summary>
/// 적 스폰 상태.
/// 스폰 연출 동안 무적 상태를 유지하며, 연출이 끝나면 타입에 맞는 첫 번째 상태로 전환합니다.
/// 스폰 무적 시간은 EnemyData.spawnInvincibleTime으로 설정합니다.
/// </summary>
public class EnemySpawnState : EntityState<EnemyController>
{
    public override int AnimStateId => 0; // Spawn/Idle 애니메이션

    protected override void OnEnter(EnemyController entity)
    {
        entity.Movement.Stop();
        entity.Health.SetInvincible(true);
    }

    protected override void OnStep(EnemyController entity)
    {
        // 스폰 무적 시간이 지나면 전투 상태로 전환
        if (TimeSinceEntered >= entity.Data.spawnInvincibleTime)
        {
            entity.Health.SetInvincible(false);
            TransitionToFirstCombatState(entity);
        }
    }

    protected override void OnExit(EnemyController entity) { }

    /// <summary>
    /// 적 타입에 따라 첫 번째 전투 상태를 결정합니다.
    /// </summary>
    private void TransitionToFirstCombatState(EnemyController entity)
    {
        switch (entity.Data.enemyType)
        {
            case EnemyType.Chaser:
            case EnemyType.Dasher:
            case EnemyType.Mindless:
            case EnemyType.Summoner:
            case EnemyType.Passive:
                entity.ChangeState(entity.ChaseState);
                break;

            case EnemyType.Ranged:
                // 원거리형은 접근 상태로 시작 (향후 ApproachState 추가 시 변경)
                entity.ChangeState(entity.ChaseState);
                break;

            default:
                entity.ChangeState(entity.ChaseState);
                break;
        }
    }
}
