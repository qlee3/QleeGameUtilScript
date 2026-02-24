using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 자식 리깅 모델에 부착하는 Animation Event 브릿지 스크립트.
/// Animation 클립의 이벤트가 이 스크립트의 메서드를 호출하면,
/// UnityEvent를 통해 부모(PlayerCombat 등)에게 전달합니다.
///
/// 사용법:
/// 1. 이 스크립트를 Animator가 있는 자식 모델 오브젝트에 추가
/// 2. Animation 클립에서 이벤트 추가 → 아래 메서드명 지정
/// 3. Inspector에서 UnityEvent에 PlayerCombat의 대응 메서드를 연결
///
/// 슬롯별 이벤트 매핑:
///   기본공격 클립: 효과 프레임 → DefaultAttackExecute  /  마지막 프레임 → DefaultAttackEnd
///   스킬1 클립:   효과 프레임 → Skill1AttackExecute   /  마지막 프레임 → Skill1AttackEnd
///   스킬2 클립:   효과 프레임 → Skill2AttackExecute   /  마지막 프레임 → Skill2AttackEnd
/// </summary>
public class AnimationEventReceiver : MonoBehaviour
{
    [Header("Footstep")]
    [Tooltip("발이 땅에 닿을 때 호출")]
    public UnityEvent onFootstep;

    [Header("Default Attack")]
    [Tooltip("기본공격 효과 발동 타이밍. PlayerCombat.OnDefaultAttackExecute 연결")]
    public UnityEvent onDefaultAttackExecute;

    [Tooltip("기본공격 애니메이션 종료. PlayerCombat.OnAbilityEnd 연결")]
    public UnityEvent onDefaultAttackEnd;

    [Header("Skill 1")]
    [Tooltip("스킬1 효과 발동 타이밍. PlayerCombat.OnSkill1AttackExecute 연결")]
    public UnityEvent onSkill1AttackExecute;

    [Tooltip("스킬1 애니메이션 종료. PlayerCombat.OnAbilityEnd 연결")]
    public UnityEvent onSkill1AttackEnd;

    [Header("Skill 2")]
    [Tooltip("스킬2 효과 발동 타이밍. PlayerCombat.OnSkill2AttackExecute 연결")]
    public UnityEvent onSkill2AttackExecute;

    [Tooltip("스킬2 애니메이션 종료. PlayerCombat.OnAbilityEnd 연결")]
    public UnityEvent onSkill2AttackEnd;

    [Header("Custom")]
    [Tooltip("범용 이벤트. Animation Event의 string 파라미터로 구분")]
    public UnityEvent<string> onCustomEvent;

    // ── Animation Clip에서 함수명으로 직접 호출하는 메서드들 ──

    /// <summary>발소리 이벤트. 함수명: Footstep</summary>
    public void Footstep() => onFootstep?.Invoke();

    /// <summary>기본공격 효과 발동. 함수명: DefaultAttackExecute</summary>
    public void DefaultAttackExecute() => onDefaultAttackExecute?.Invoke();

    /// <summary>기본공격 애니메이션 종료. 함수명: DefaultAttackEnd</summary>
    public void DefaultAttackEnd() => onDefaultAttackEnd?.Invoke();

    /// <summary>스킬1 효과 발동. 함수명: Skill1AttackExecute</summary>
    public void Skill1AttackExecute() => onSkill1AttackExecute?.Invoke();

    /// <summary>스킬1 애니메이션 종료. 함수명: Skill1AttackEnd</summary>
    public void Skill1AttackEnd() => onSkill1AttackEnd?.Invoke();

    /// <summary>스킬2 효과 발동. 함수명: Skill2AttackExecute</summary>
    public void Skill2AttackExecute() => onSkill2AttackExecute?.Invoke();

    /// <summary>스킬2 애니메이션 종료. 함수명: Skill2AttackEnd</summary>
    public void Skill2AttackEnd() => onSkill2AttackEnd?.Invoke();

    /// <summary>범용 이벤트. 함수명: CustomEvent (String 파라미터에 이벤트 이름 입력)</summary>
    public void CustomEvent(string eventName) => onCustomEvent?.Invoke(eventName);
}
