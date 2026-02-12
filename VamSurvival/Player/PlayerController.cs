using UnityEngine;

/// <summary>
/// 플레이어의 중앙 컨트롤러.
/// Entity&lt;T&gt;를 상속하여 FSM이 내장되어 있으며,
/// 모든 플레이어 모듈(Input, Movement, Animator 등)의 참조를 보유합니다.
/// </summary>
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerAnimator))]
[RequireComponent(typeof(PlayerStats))]
[RequireComponent(typeof(PlayerHealth))]
[RequireComponent(typeof(PlayerLevel))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : Entity<PlayerController>
{
    // ── 모듈 참조 ──
    public PlayerInput Input { get; private set; }
    public PlayerMovement Movement { get; private set; }
    public PlayerAnimator Animator { get; private set; }
    public PlayerStats Stats { get; private set; }
    public PlayerHealth Health { get; private set; }
    public PlayerLevel Level { get; private set; }

    // ── 상태 인스턴스 (캐싱) ──
    public PlayerIdleState IdleState { get; private set; }
    public PlayerMoveState MoveState { get; private set; }
    public PlayerHurtState HurtState { get; private set; }

    private void Awake()
    {
        // 모듈 수집
        Input = GetComponent<PlayerInput>();
        Movement = GetComponent<PlayerMovement>();
        Animator = GetComponent<PlayerAnimator>();
        Stats = GetComponent<PlayerStats>();
        Health = GetComponent<PlayerHealth>();
        Level = GetComponent<PlayerLevel>();

        // 상태 인스턴스 생성
        IdleState = new PlayerIdleState();
        MoveState = new PlayerMoveState();
        HurtState = new PlayerHurtState();
    }

    private void Start()
    {
        Health.OnDamaged += HandleDamaged;
        // 초기 상태: Idle
        ChangeState(IdleState);
    }

    private void OnDestroy()
    {
        Health.OnDamaged -= HandleDamaged;
    }

    private void HandleDamaged(DamageInfo damage)
    {
        if (!Health.IsAlive) return;
        if (CurrentState is PlayerHurtState) return;

        HurtState.SetDamageInfo(damage);
        ChangeState(HurtState);
    }

    protected override void Update()
    {
        base.Update(); // Entity<T>.Update() → CurrentState.Step() 호출
    }

    /// <summary>
    /// 상태 전환 시 자동으로 해당 상태의 애니메이션을 재생합니다.
    /// </summary>
    protected override void OnStateChanged(EntityState<PlayerController> newState)
    {
        if (newState.AnimStateId >= 0)
        {
            Animator.Play(newState.AnimStateId);
        }
    }
}
