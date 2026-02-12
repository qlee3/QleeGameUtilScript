using UnityEngine;

/// <summary>
/// 플레이어 이동을 처리하는 컴포넌트.
/// Rigidbody.MovePosition 기반으로 물리 충돌(벽 등)을 자동 처리합니다.
/// FSM 상태(MoveState)에서 Move()를 호출하면, 실제 이동은 FixedUpdate에서 수행됩니다.
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float rotationSpeed = 720f;

    [Header("Knockback")]
    [SerializeField] private float knockbackDecay = 8f;

    private PlayerController controller;
    private Rigidbody rb;

    /// <summary>FixedUpdate에서 처리할 이동 벡터 (방향 × 속도).</summary>
    private Vector3 pendingMove;

    /// <summary>넉백 속도. FixedUpdate에서 감쇠 적용.</summary>
    private Vector3 knockbackVelocity;

    /// <summary>현재 이동 중인지 여부.</summary>
    public bool IsMoving { get; private set; }

    private void Awake()
    {
        controller = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody>();

        ConfigureRigidbody();
    }

    /// <summary>
    /// Rigidbody를 뱀서라이크 탑다운 이동에 맞게 설정합니다.
    /// </summary>
    private void ConfigureRigidbody()
    {
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezePositionY
                       | RigidbodyConstraints.FreezeRotationX
                       | RigidbodyConstraints.FreezeRotationY
                       | RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    /// <summary>
    /// 입력 값을 받아 이동 벡터를 저장하고 회전을 처리합니다.
    /// MoveState에서 매 프레임 호출하세요. 실제 이동은 FixedUpdate에서 수행됩니다.
    /// </summary>
    public void Move(Vector2 input)
    {
        IsMoving = input.sqrMagnitude > 0.01f;

        if (!IsMoving)
        {
            pendingMove = Vector3.zero;
            return;
        }

        // XZ 평면 이동 (Y는 무시) - 이동속도는 PlayerStats에서 가져옴
        float speed = controller.Stats.MoveSpeed.Value;
        Vector3 moveDirection = new Vector3(input.x, 0f, input.y).normalized;
        pendingMove = moveDirection * speed;

        // 이동 방향으로 부드럽게 회전 (시각적 처리이므로 Update 타이밍에서 수행)
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
        pendingMove = Vector3.zero;
    }

    /// <summary>
    /// 피격 방향으로 넉백을 적용합니다.
    /// HurtState에서 호출합니다.
    /// </summary>
    public void ApplyKnockback(Vector3 direction, float force)
    {
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.001f) return;
        knockbackVelocity = direction.normalized * force;
    }

    private void FixedUpdate()
    {
        // 넉백 감쇠
        knockbackVelocity = Vector3.MoveTowards(
            knockbackVelocity,
            Vector3.zero,
            knockbackDecay * Time.fixedDeltaTime
        );

        Vector3 totalMove = pendingMove * Time.fixedDeltaTime + knockbackVelocity * Time.fixedDeltaTime;
        if (totalMove.sqrMagnitude > 0.001f)
        {
            rb.MovePosition(rb.position + totalMove);
        }
    }
}
