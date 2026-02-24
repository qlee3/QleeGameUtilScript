using UnityEngine;

/// <summary>
/// 플레이어 어빌리티 슬롯 관리, 쿨다운, 실행 허브 컴포넌트.
///
/// Inspector 연결 가이드:
///   - attackData / skill1Data / skill2Data: 원하는 PlayerAbilityData SO 장착
///   - AnimationEventReceiver의 각 Execute/End UnityEvent에 아래 메서드를 연결:
///       onDefaultAttackExecute → PlayerCombat.OnDefaultAttackExecute
///       onSkill1AttackExecute  → PlayerCombat.OnSkill1AttackExecute
///       onSkill2AttackExecute  → PlayerCombat.OnSkill2AttackExecute
///       onDefaultAttackEnd     → PlayerCombat.OnAbilityEnd
///       onSkill1AttackEnd      → PlayerCombat.OnAbilityEnd
///       onSkill2AttackEnd      → PlayerCombat.OnAbilityEnd
/// </summary>
public class PlayerCombat : MonoBehaviour
{
    [Header("Ability Slots")]
    [Tooltip("기본 공격 어빌리티. 어떤 타입의 PlayerAbilityData SO든 장착 가능합니다.")]
    [SerializeField] private PlayerAbilityData attackData;

    [Tooltip("스킬1 어빌리티. 어떤 타입의 PlayerAbilityData SO든 장착 가능합니다.")]
    [SerializeField] private PlayerAbilityData skill1Data;

    [Tooltip("스킬2 어빌리티. 어떤 타입의 PlayerAbilityData SO든 장착 가능합니다.")]
    [SerializeField] private PlayerAbilityData skill2Data;

    [Header("Fire Point")]
    [Tooltip("투사체 발사 위치/방향. 미설정 시 플레이어 위치·forward에서 발사합니다.")]
    [SerializeField] private Transform firePoint;

    public PlayerAbilityData AttackData => attackData;
    public PlayerAbilityData Skill1Data => skill1Data;
    public PlayerAbilityData Skill2Data => skill2Data;

    /// <summary>투사체 발사 위치. null이면 플레이어 transform 사용.</summary>
    public Transform FirePoint => firePoint;

    // 쿨다운 타이머 (남은 시간, 0 이하면 사용 가능)
    private float attackCooldownTimer;
    private float skill1CooldownTimer;
    private float skill2CooldownTimer;

    // 쿨다운 총 시간 (UI 비율 계산용)
    private float attackCooldownTotal;
    private float skill1CooldownTotal;
    private float skill2CooldownTotal;

    public bool CanUseAttack => attackData != null && attackCooldownTimer <= 0f;
    public bool CanUseSkill1 => skill1Data != null && skill1CooldownTimer <= 0f;
    public bool CanUseSkill2 => skill2Data != null && skill2CooldownTimer <= 0f;

    /// <summary>기본공격 쿨타임 비율 (0~1, 0이면 사용 가능)</summary>
    public float AttackCooldownRatio => attackData != null && attackCooldownTotal > 0f
        ? Mathf.Clamp01(attackCooldownTimer / attackCooldownTotal) : 0f;

    /// <summary>스킬1 쿨타임 비율 (0~1, 0이면 사용 가능)</summary>
    public float Skill1CooldownRatio => skill1Data != null && skill1CooldownTotal > 0f
        ? Mathf.Clamp01(skill1CooldownTimer / skill1CooldownTotal) : 0f;

    /// <summary>스킬2 쿨타임 비율 (0~1, 0이면 사용 가능)</summary>
    public float Skill2CooldownRatio => skill2Data != null && skill2CooldownTotal > 0f
        ? Mathf.Clamp01(skill2CooldownTimer / skill2CooldownTotal) : 0f;

    /// <summary>
    /// AnimationEventReceiver의 End 이벤트가 발생하면 true.
    /// PlayerAbilityState.OnStep에서 이 플래그를 확인하여 IdleState로 전환합니다.
    /// </summary>
    public bool AbilityEnded { get; private set; }

    private PlayerController controller;

    private void Awake()
    {
        controller = GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (attackCooldownTimer > 0f) attackCooldownTimer -= Time.deltaTime;
        if (skill1CooldownTimer > 0f) skill1CooldownTimer -= Time.deltaTime;
        if (skill2CooldownTimer > 0f) skill2CooldownTimer -= Time.deltaTime;
    }

    /// <summary>
    /// 어빌리티 실행 준비. PlayerAbilityState.OnEnter에서 호출합니다.
    /// AbilityEnded를 리셋하고 해당 슬롯의 쿨다운을 시작합니다.
    /// </summary>
    public void PrepareAbility(PlayerAbilityData data)
    {
        AbilityEnded = false;

        if (data == null) return;

        float cd = data.cooldown;
        if (data == attackData)
        {
            attackCooldownTotal = cd * controller.Stats.AttackCooldown.Value;
            attackCooldownTimer = attackCooldownTotal;
        }
        else if (data == skill1Data)
        {
            skill1CooldownTotal = cd * controller.Stats.SkillCooldown.Value;
            skill1CooldownTimer = skill1CooldownTotal;
        }
        else if (data == skill2Data)
        {
            skill2CooldownTotal = cd * controller.Stats.SkillCooldown.Value;
            skill2CooldownTimer = skill2CooldownTotal;
        }
    }

    // ── AnimationEventReceiver UnityEvent에 연결할 메서드들 ──

    /// <summary>기본공격 효과 발동 타이밍. AnimationEventReceiver.onDefaultAttackExecute에 연결하세요.</summary>
    public void OnDefaultAttackExecute() => attackData?.Execute(controller);

    /// <summary>스킬1 효과 발동 타이밍. AnimationEventReceiver.onSkill1AttackExecute에 연결하세요.</summary>
    public void OnSkill1AttackExecute() => skill1Data?.Execute(controller);

    /// <summary>스킬2 효과 발동 타이밍. AnimationEventReceiver.onSkill2AttackExecute에 연결하세요.</summary>
    public void OnSkill2AttackExecute() => skill2Data?.Execute(controller);

    /// <summary>
    /// 어빌리티 애니메이션 종료.
    /// AnimationEventReceiver의 End 이벤트 3개(Default/Skill1/Skill2) 모두에 연결하세요.
    /// </summary>
    public void OnAbilityEnd() => AbilityEnded = true;
}
