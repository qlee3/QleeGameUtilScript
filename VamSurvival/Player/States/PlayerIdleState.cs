/// <summary>
/// 플레이어 대기 상태.
/// 이동 입력이 들어오면 MoveState로 전환합니다.
/// </summary>
public class PlayerIdleState : EntityState<PlayerController>
{
    public override int AnimStateId => 0;

    protected override void OnEnter(PlayerController entity)
    {
        entity.Movement.Stop();
    }

    protected override void OnStep(PlayerController entity)
    {
        if (entity.Input.MoveInput.sqrMagnitude > 0.01f)
        {
            entity.ChangeState(entity.MoveState);
        }
    }

    protected override void OnExit(PlayerController entity) { }
}
