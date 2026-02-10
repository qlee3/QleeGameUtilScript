using UnityEngine;

/// <summary>
/// FSM(유한 상태 머신)이 내장된 엔티티 베이스 클래스.
/// Player, Enemy 등 상태 기반 행동이 필요한 모든 엔티티가 상속합니다.
/// CRTP 패턴으로 하위 클래스 타입을 제네릭으로 전달합니다.
/// </summary>
public abstract class Entity<T> : MonoBehaviour where T : Entity<T>
{
    /// <summary>현재 활성화된 상태.</summary>
    public EntityState<T> CurrentState { get; private set; }

    /// <summary>
    /// 상태를 전환합니다. 현재 상태의 Exit → 새 상태의 Enter → OnStateChanged 순서로 호출됩니다.
    /// </summary>
    public void ChangeState(EntityState<T> newState)
    {
        CurrentState?.Exit((T)this);
        CurrentState = newState;
        CurrentState?.Enter((T)this);
        OnStateChanged(newState);
    }

    /// <summary>
    /// 상태가 전환된 직후 호출되는 콜백.
    /// 하위 클래스에서 오버라이드하여 애니메이션, 이펙트 등을 처리할 수 있습니다.
    /// </summary>
    protected virtual void OnStateChanged(EntityState<T> newState) { }

    protected virtual void Update()
    {
        CurrentState?.Step((T)this);
    }
}
