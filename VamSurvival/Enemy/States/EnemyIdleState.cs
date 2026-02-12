/// <summary>
/// 적 대기(정지) 상태.
/// 추격 중 간헐적 멈춤, 대쉬/공격 후 쿨다운 등 다양한 상황에서 공용으로 사용합니다.
/// 대기 시간은 상태 진입 전에 SetDuration()으로 설정합니다.
/// 대기 시간이 끝나면 ChaseState로 복귀합니다.
/// </summary>
public class EnemyIdleState : EntityState<EnemyController>
{
    public override int AnimStateId => 0; // Idle 애니메이션

    private float duration;

    /// <summary>
    /// 대기 시간을 설정합니다. ChangeState 호출 전에 반드시 설정하세요.
    /// </summary>
    public void SetDuration(float seconds)
    {
        duration = seconds;
    }

    protected override void OnEnter(EnemyController entity)
    {
        entity.Movement.Stop();
    }

    protected override void OnStep(EnemyController entity)
    {
        if (TimeSinceEntered >= duration)
        {
            // 대기 시간 종료 → 추격 상태로 복귀
            entity.ChangeState(entity.ChaseState);
        }
    }

    protected override void OnExit(EnemyController entity) { }
}
