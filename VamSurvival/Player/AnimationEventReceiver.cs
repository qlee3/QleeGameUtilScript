using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 자식 리깅 모델에 부착하는 Animation Event 브릿지 스크립트.
/// Animation 클립의 이벤트가 이 스크립트의 메서드를 호출하면,
/// UnityEvent를 통해 부모(PlayerController 등)에게 전달합니다.
///
/// 사용법:
/// 1. 이 스크립트를 Animator가 있는 자식 모델 오브젝트에 추가
/// 2. Animation 클립에서 이벤트 추가 → 아래 메서드명 지정
/// 3. Inspector에서 UnityEvent에 원하는 반응을 연결
/// </summary>
public class AnimationEventReceiver : MonoBehaviour
{
    [Header("Animation Events")]
    [Tooltip("발소리 등 발이 땅에 닿을 때 호출")]
    public UnityEvent onFootstep;

    [Tooltip("공격 판정이 시작될 때 호출")]
    public UnityEvent onAttackHit;

    [Tooltip("투사체(총알 등)를 발사할 때 호출")]
    public UnityEvent onFireProjectile;

    [Tooltip("범용 이벤트 - Animation Event의 string 파라미터로 구분")]
    public UnityEvent<string> onCustomEvent;

    // ── Animation Event에서 호출되는 메서드들 ──

    /// <summary>발소리 이벤트. Animation 클립에서 함수명: Footstep</summary>
    public void Footstep()
    {
        onFootstep?.Invoke();
    }

    /// <summary>공격 판정 이벤트. Animation 클립에서 함수명: AttackHit</summary>
    public void AttackHit()
    {
        onAttackHit?.Invoke();
    }

    /// <summary>투사체 발사 이벤트. Animation 클립에서 함수명: FireProjectile</summary>
    public void FireProjectile()
    {
        onFireProjectile?.Invoke();
    }

    /// <summary>
    /// 범용 이벤트. Animation 클립에서 함수명: CustomEvent, String 파라미터에 이벤트 이름 입력.
    /// 하나의 메서드로 다양한 이벤트를 처리할 수 있습니다.
    /// </summary>
    public void CustomEvent(string eventName)
    {
        onCustomEvent?.Invoke(eventName);
    }
}
