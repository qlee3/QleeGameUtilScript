using UnityEngine;

/// <summary>
/// 적 애니메이션을 제어하는 컴포넌트.
/// PlayerAnimator와 동일한 패턴으로, Integer 파라미터("State")를 통해 애니메이션을 전환합니다.
/// </summary>
public class EnemyAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;

    // Animator 파라미터 해시 캐싱
    private static readonly int HashState = Animator.StringToHash("State");

    /// <summary>
    /// Animator의 "State" 파라미터를 지정된 값으로 설정합니다.
    /// Animator Controller에서 해당 값에 맞는 Transition이 설정되어 있어야 합니다.
    /// </summary>
    public void Play(int stateId)
    {
        if (animator == null || stateId < 0) return;
        animator.SetInteger(HashState, stateId);
    }

    /// <summary>
    /// 런타임에 Animator를 교체합니다.
    /// 풀링 재사용 시 다른 모델로 교체할 때 사용합니다.
    /// </summary>
    public void SetAnimator(Animator newAnimator)
    {
        animator = newAnimator;
    }
}
