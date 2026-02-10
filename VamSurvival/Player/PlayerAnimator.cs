using UnityEngine;

/// <summary>
/// 플레이어 애니메이션을 제어하는 컴포넌트.
/// 자식 오브젝트에 있는 Animator를 참조하며,
/// FSM 상태 전환 시 Integer 파라미터("State")를 설정하여 애니메이션을 전환합니다.
/// </summary>
public class PlayerAnimator : MonoBehaviour
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
}
