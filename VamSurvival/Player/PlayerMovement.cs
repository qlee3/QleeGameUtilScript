using UnityEngine;

/// <summary>
/// 플레이어 이동을 처리하는 컴포넌트.
/// 자체적으로 Update하지 않으며, FSM 상태(MoveState)에서 Move()를 호출해야 동작합니다.
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 720f;

    /// <summary>현재 이동 중인지 여부.</summary>
    public bool IsMoving { get; private set; }

    /// <summary>
    /// 입력 값을 받아 이동과 회전을 처리합니다.
    /// MoveState에서 매 프레임 호출하세요.
    /// </summary>
    public void Move(Vector2 input)
    {
        IsMoving = input.sqrMagnitude > 0.01f;

        if (!IsMoving) return;

        // XZ 평면 이동 (Y는 무시)
        Vector3 moveDirection = new Vector3(input.x, 0f, input.y).normalized;
        transform.position += moveDirection * (moveSpeed * Time.deltaTime);

        // 이동 방향으로 부드럽게 회전
        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    /// <summary>
    /// 이동을 즉시 멈춥니다. IdleState 진입 시 호출하세요.
    /// </summary>
    public void Stop()
    {
        IsMoving = false;
    }
}
