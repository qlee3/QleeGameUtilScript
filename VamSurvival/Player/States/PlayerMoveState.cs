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
        // 공격/스킬 입력 우선 처리 (이동 중단)
        if (entity.Input.AttackPressed && entity.Combat.CanUseAttack)
        {
            entity.ChangeState(entity.AttackState);
            return;
        }
        if (entity.Input.Skill1Pressed && entity.Combat.CanUseSkill1)
        {
            entity.ChangeState(entity.Skill1State);
            return;
        }
        if (entity.Input.Skill2Pressed && entity.Combat.CanUseSkill2)
        {
            entity.ChangeState(entity.Skill2State);
            return;
        }

        if (entity.Input.MoveInput.sqrMagnitude <= 0.01f)
        {
            entity.ChangeState(entity.IdleState);
            return;
        }

        entity.Movement.Move(entity.Input.MoveInput);
    }

    protected override void OnExit(PlayerController entity) { }
}
