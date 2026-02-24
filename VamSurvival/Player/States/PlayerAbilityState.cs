using System;

/// <summary>
/// 기본공격/스킬1/스킬2에 공통으로 사용하는 FSM 상태.
/// PlayerController.Awake에서 AnimStateId와 어빌리티 데이터 선택자를 주입합니다.
///
/// 이동 차단:
///   - isMovementAbility = false (투사체/장판): OnEnter에서 Movement.Stop(), 이동 입력 무시
///   - isMovementAbility = true  (대시 등):    OnEnter에서 Stop 없이, SO의 Execute가 직접 이동 처리
///
/// 상태 종료:
///   - AnimationEventReceiver의 End 이벤트 → PlayerCombat.OnAbilityEnd() → AbilityEnded = true
///   - OnStep에서 AbilityEnded를 감지하면 IdleState로 전환
/// </summary>
public class PlayerAbilityState : EntityState<PlayerController>
{
    private readonly int configuredAnimStateId;
    private readonly Func<PlayerCombat, PlayerAbilityData> getAbilityData;

    public override int AnimStateId => configuredAnimStateId;

    public PlayerAbilityState(int animStateId, Func<PlayerCombat, PlayerAbilityData> dataSelector)
    {
        configuredAnimStateId = animStateId;
        getAbilityData = dataSelector;
    }

    protected override void OnEnter(PlayerController entity)
    {
        var data = getAbilityData(entity.Combat);
        entity.Combat.PrepareAbility(data);

        if (data == null || !data.isMovementAbility)
            entity.Movement.Stop();
    }

    protected override void OnStep(PlayerController entity)
    {
        if (entity.Combat.AbilityEnded)
            entity.ChangeState(entity.IdleState);
    }

    protected override void OnExit(PlayerController entity)
    {
        // isMovementAbility(대시형)의 경우에도 상태 종료 시 남은 대시 속도를 정리
        entity.Movement.Stop();
    }
}
