/// <summary>
/// 플레이어 이동 상태.
/// 이동 입력이 없으면 IdleState로 전환합니다.
/// </summary>
public class PlayerMoveState : EntityState<PlayerController>
{
    public override int AnimStateId => 1;

    protected override void OnEnter(PlayerController entity) { }

    protected override void OnStep(PlayerController entity)
    {
        if (entity.Input.MoveInput.sqrMagnitude <= 0.01f)
        {
            entity.ChangeState(entity.IdleState);
            return;
        }

        entity.Movement.Move(entity.Input.MoveInput);
    }

    protected override void OnExit(PlayerController entity) { }
}
