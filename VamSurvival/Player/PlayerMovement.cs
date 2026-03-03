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

    [Header("Camera")]
    [SerializeField] private ThirdPersonCameraController cameraController;

    [Header("Knockback")]
    [SerializeField] private float knockbackDecay = 8f;

    private PlayerController controller;
    private Rigidbody rb;

    /// <summary>FixedUpdate에서 처리할 이동 벡터 (방향 × 속도).</summary>
    private Vector3 pendingMove;

    /// <summary>넉백 속도. FixedUpdate에서 감쇠 적용.</summary>
    private Vector3 knockbackVelocity;

    /// <summary>대시 이동 방향 × 속도.</summary>
    private Vector3 dashVelocity;

    /// <summary>대시 남은 거리. 0 이하가 되면 대시 종료.</summary>
    private float dashRemainingDistance;

    /// <summary>현재 대시 중인지 여부.</summary>
    public bool IsDashing => dashRemainingDistance > 0f;

    /// <summary>현재 이동 중인지 여부.</summary>
    public bool IsMoving { get; private set; }

    private void Awake()
    {
        controller = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody>();

        if (cameraController == null)
        {
            cameraController = FindObjectOfType<ThirdPersonCameraController>();
        }

        ConfigureRigidbody();
    }

    /// <summary>
    /// Rigidbody를 뱀서라이크 탑다운 이동에 맞게 설정합니다.
    /// </summary>
    private void ConfigureRigidbody()
    {
        rb.useGravity = false;
        // 3인칭 이동을 위해 Y축 회전은 허용하고, 나머지 회전/높이만 고정합니다.
        rb.constraints = RigidbodyConstraints.FreezePositionY
                       | RigidbodyConstraints.FreezeRotationX
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

        Vector3 moveDirection;

        // 카메라 기준 이동: 카메라의 전방/우측 벡터를 사용해 월드 좌표 이동 방향을 계산합니다.
        if (cameraController != null)
        {
            Vector3 forward = cameraController.CameraForwardOnPlane;
            Vector3 right = cameraController.CameraRightOnPlane;

            moveDirection = (right * input.x + forward * input.y);
            moveDirection.y = 0f;
            if (moveDirection.sqrMagnitude > 0.0001f)
            {
                moveDirection.Normalize();
            }
        }
        else
        {
            // 카메라 컨트롤러가 없으면 기존 탑다운 방식으로 폴백합니다.
            moveDirection = new Vector3(input.x, 0f, input.y).normalized;
        }

        if (moveDirection.sqrMagnitude < 0.0001f)
        {
            IsMoving = false;
            pendingMove = Vector3.zero;
            return;
        }

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

    /// <summary>
    /// 대시 이동을 시작합니다. DashAbilityData.Execute()에서 호출합니다.
    /// 대시 중에는 pendingMove(일반 이동)가 무시되고 대시 속도만 적용됩니다.
    /// </summary>
    /// <param name="direction">대시 방향 (XZ 평면)</param>
    /// <param name="speed">대시 이동 속도</param>
    /// <param name="distance">대시 총 이동 거리</param>
    /// <param name="damage">대시 중 피해량 (미사용, 향후 확장용)</param>
    public void ApplyDash(Vector3 direction, float speed, float distance, float damage = 0f)
    {
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.001f) return;

        dashVelocity = direction.normalized * speed;
        dashRemainingDistance = distance;

        // 대시 방향으로 즉시 회전
        transform.rotation = Quaternion.LookRotation(direction.normalized);
    }

    private void FixedUpdate()
    {
        // 넉백 감쇠
        knockbackVelocity = Vector3.MoveTowards(
            knockbackVelocity,
            Vector3.zero,
            knockbackDecay * Time.fixedDeltaTime
        );

        Vector3 totalMove = Vector3.zero;

        // 대시 중이면 일반 이동 무시, 대시 속도 적용
        if (dashRemainingDistance > 0f)
        {
            float step = dashVelocity.magnitude * Time.fixedDeltaTime;
            step = Mathf.Min(step, dashRemainingDistance);
            dashRemainingDistance -= step;
            totalMove += dashVelocity.normalized * step;
        }
        else
        {
            totalMove += pendingMove * Time.fixedDeltaTime;
        }

        totalMove += knockbackVelocity * Time.fixedDeltaTime;

        if (totalMove.sqrMagnitude > 0.001f)
        {
            rb.MovePosition(rb.position + totalMove);
        }
    }
}
