using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 모든 엔티티 상태의 추상 베이스 클래스.
/// Template Method 패턴으로 공통 로직(이벤트, 타이머)을 처리하고,
/// 하위 클래스는 OnEnter/OnStep/OnExit만 구현하면 됩니다.
/// </summary>
public abstract class EntityState<T> where T : Entity<T>
{
    public UnityEvent onEnter;
    public UnityEvent onExit;

    /// <summary>
    /// 이 상태에 대응하는 Animator 파라미터 값 (Int).
    /// 상태 전환 시 자동으로 Animator의 "State" 파라미터가 이 값으로 설정됩니다.
    /// -1이면 애니메이션을 변경하지 않습니다.
    /// </summary>
    public virtual int AnimStateId => -1;

    /// <summary>현재 상태에 진입한 이후 경과 시간.</summary>
    public float TimeSinceEntered { get; protected set; }

    public void Enter(T entity)
    {
        TimeSinceEntered = 0f;
        onEnter?.Invoke();
        OnEnter(entity);
    }

    public void Exit(T entity)
    {
        onExit?.Invoke();
        OnExit(entity);
    }

    public void Step(T entity)
    {
        OnStep(entity);
        TimeSinceEntered += Time.deltaTime;
    }

    /// <summary>상태 진입 시 호출됩니다.</summary>
    protected abstract void OnEnter(T entity);

    /// <summary>상태가 다른 상태로 전환될 때 호출됩니다.</summary>
    protected abstract void OnExit(T entity);

    /// <summary>이 상태가 활성화된 동안 매 프레임 호출됩니다.</summary>
    protected abstract void OnStep(T entity);
}
