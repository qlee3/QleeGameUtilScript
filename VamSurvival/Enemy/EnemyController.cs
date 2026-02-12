using System;
using UnityEngine;

/// <summary>
/// 적의 중앙 컨트롤러.
/// Entity&lt;T&gt;를 상속하여 FSM이 내장되어 있으며,
/// 모든 적 모듈(Movement, Animator, Stats, Health, Detection)의 참조를 보유합니다.
///
/// 오브젝트 풀링을 지원하며, Initialize()로 초기화 / Deactivate()로 회수합니다.
/// Awake가 아닌 Initialize에서 초기화하는 이유:
/// 동일 프리팹이 다른 EnemyData로 재사용될 수 있기 때문입니다.
/// </summary>
[RequireComponent(typeof(EnemyMovement))]
[RequireComponent(typeof(EnemyAnimator))]
[RequireComponent(typeof(EnemyStats))]
[RequireComponent(typeof(EnemyHealth))]
[RequireComponent(typeof(EnemyDetection))]
public class EnemyController : Entity<EnemyController>
{
    // ── 데이터 ──

    /// <summary>현재 적용 중인 EnemyData SO.</summary>
    public EnemyData Data { get; private set; }

    // ── 모듈 참조 ──

    public EnemyMovement Movement { get; private set; }
    public EnemyAnimator Animator { get; private set; }
    public EnemyStats Stats { get; private set; }
    public EnemyHealth Health { get; private set; }
    public EnemyDetection Detection { get; private set; }
    public ContactDamage ContactDamage { get; private set; }

    // ── 공통 상태 인스턴스 (캐싱) ──

    public EnemySpawnState SpawnState { get; private set; }
    public EnemyDeathState DeathState { get; private set; }
    public EnemyIdleState IdleState { get; private set; }
    public EnemyChaseState ChaseState { get; private set; }

    // ── 이벤트 ──

    /// <summary>적 비활성화(풀 반환) 시 발생. 스폰 매니저가 구독.</summary>
    public event Action<EnemyController> OnDeactivated;

    private void Awake()
    {
        // 모듈 수집
        Movement = GetComponent<EnemyMovement>();
        Animator = GetComponent<EnemyAnimator>();
        Stats = GetComponent<EnemyStats>();
        Health = GetComponent<EnemyHealth>();
        Detection = GetComponent<EnemyDetection>();
        ContactDamage = GetComponent<ContactDamage>();

        // 공통 상태 인스턴스 생성
        SpawnState = new EnemySpawnState();
        DeathState = new EnemyDeathState();
        IdleState = new EnemyIdleState();
        ChaseState = new EnemyChaseState();
    }

    /// <summary>
    /// 오브젝트 풀에서 꺼낼 때 호출합니다.
    /// EnemyData로 모든 모듈을 초기화하고 FSM을 시작합니다.
    /// </summary>
    public void Initialize(EnemyData data, Transform playerTransform)
    {
        Data = data;

        // 모듈 초기화
        Stats.Initialize(data);
        Health.Initialize(Stats);
        Detection.Initialize(playerTransform);
        Movement.Initialize(data.moveSpeed);

        if (ContactDamage != null)
        {
            ContactDamage.Initialize(data.contactDamage, data.contactDamageInterval);
        }

        // 사망 이벤트 구독
        Health.OnDeath += HandleDeath;

        // FSM 시작: 스폰 상태
        ChangeState(SpawnState);
    }

    /// <summary>
    /// 오브젝트 풀로 반환 시 호출합니다.
    /// 이벤트 해제, 상태 초기화를 수행합니다.
    /// </summary>
    public void Deactivate()
    {
        Health.OnDeath -= HandleDeath;
        Movement.Stop();
        OnDeactivated?.Invoke(this);
        gameObject.SetActive(false);
    }

    protected override void Update()
    {
        base.Update(); // Entity<T>.Update() → CurrentState.Step() 호출
    }

    /// <summary>
    /// 상태 전환 시 자동으로 해당 상태의 애니메이션을 재생합니다.
    /// </summary>
    protected override void OnStateChanged(EntityState<EnemyController> newState)
    {
        if (newState.AnimStateId >= 0)
        {
            Animator.Play(newState.AnimStateId);
        }
    }

    /// <summary>
    /// 사망 처리. DeathState로 전환합니다.
    /// 경험치 젬 스폰은 DeathState에서 처리합니다.
    /// </summary>
    private void HandleDeath()
    {
        ChangeState(DeathState);
    }
}
