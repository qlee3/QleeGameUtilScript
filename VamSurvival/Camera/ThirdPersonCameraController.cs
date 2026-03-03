using UnityEngine;

/// <summary>
/// 3인칭 카메라를 제어하는 컴포넌트.
/// 
/// - New Input System의 PlayerInput.LookInput(마우스/패드 시점 입력)을 사용해 yaw/pitch를 누적합니다.
/// - 자신(Transform)을 카메라 피벗으로 사용하며, 이 Transform을 CinemachineVirtualCamera의 Follow 대상으로 연결합니다.
/// - 실제 카메라 추적/충돌 등은 Cinemachine에 맡기고, 이 스크립트는 오직 회전(yaw/pitch)과 피벗 위치만 관리합니다.
///
/// [씬 세팅 가이드]
/// 1. Package Manager에서 Cinemachine을 설치합니다.
/// 2. 씬의 메인 카메라에 CinemachineBrain 컴포넌트를 추가합니다.
/// 3. 빈 GameObject를 만들고 이름을 예: "CameraRig" 로 지정한 후, 플레이어를 따라다닐 피벗으로 사용합니다.
///    - 이 스크립트(ThirdPersonCameraController)를 "CameraRig"에 붙입니다.
/// 4. CinemachineVirtualCamera를 하나 만들고:
///    - Follow = "CameraRig" Transform
///    - LookAt = 플레이어(또는 플레이어의 상체/머리) Transform
///    - Body = Third Person Follow (또는 원하는 바디 설정) 으로 구성합니다.
/// 5. 이 스크립트의 followTarget 필드에 플레이어 Transform을 할당합니다.
/// 6. playerInput 필드에 플레이어의 PlayerInput 컴포넌트를 할당합니다.
/// 
/// 이렇게 세팅하면, 이 스크립트는 Look 입력을 이용해 CameraRig을 회전시키고,
/// Cinemachine은 해당 피벗을 기준으로 카메라를 3인칭으로 배치합니다.
/// </summary>
public class ThirdPersonCameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform followTarget;     // 보통 플레이어 루트 Transform
    [SerializeField] private PlayerInput playerInput;    // 플레이어 입력(특히 LookInput)

    [Header("Rotation Settings")]
    [SerializeField] private float mouseSensitivityX = 120f;
    [SerializeField] private float mouseSensitivityY = 120f;
    [SerializeField] private float minPitch = -40f;
    [SerializeField] private float maxPitch = 70f;
    [SerializeField] private bool invertY = false;

    [Header("Cursor")]
    [SerializeField] private bool lockCursorOnEnable = true;

    private float yaw;   // 수평 회전 (Y축)
    private float pitch; // 수직 회전 (X축)

    /// <summary>
    /// 실제 렌더링 카메라(보통 메인 카메라)의 XZ 평면상 전방 벡터(정규화).
    /// PlayerMovement에서 이동 방향 계산에 사용합니다.
    /// Cinemachine FreeLook 등을 사용할 때는 플레이어 주변을 도는 것은 가상 카메라이므로,
    /// Transform.forward 대신 Camera.main.forward 를 사용하는 것이 더 정확합니다.
    /// </summary>
    public Vector3 CameraForwardOnPlane
    {
        get
        {
            Camera cam = Camera.main;
            Vector3 fwd;

            if (cam != null)
            {
                fwd = cam.transform.forward;
            }
            else
            {
                fwd = transform.forward;
            }

            fwd.y = 0f;
            if (fwd.sqrMagnitude < 0.0001f)
                return Vector3.forward;
            return fwd.normalized;
        }
    }

    /// <summary>
    /// 실제 렌더링 카메라의 XZ 평면상 오른쪽 벡터(정규화). PlayerMovement에서 이동 방향 계산에 사용합니다.
    /// </summary>
    public Vector3 CameraRightOnPlane
    {
        get
        {
            Camera cam = Camera.main;
            Vector3 right;

            if (cam != null)
            {
                right = cam.transform.right;
            }
            else
            {
                right = transform.right;
            }

            right.y = 0f;
            if (right.sqrMagnitude < 0.0001f)
                return Vector3.right;
            return right.normalized;
        }
    }

    private void Reset()
    {
        // 에디터에서 Add Component 했을 때, 기본적으로 자기 주변에서 참조를 찾아봅니다.
        TryAutoAssignReferences();
    }

    private void Awake()
    {
        TryAutoAssignReferences();

        // 현재 회전을 기준으로 yaw/pitch 초기화
        Vector3 euler = transform.rotation.eulerAngles;
        yaw = euler.y;
        pitch = euler.x;
    }

    private void OnEnable()
    {
        if (lockCursorOnEnable)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void LateUpdate()
    {
        if (followTarget != null)
        {
            // 카메라 피벗을 항상 플레이어 위치에 고정
            transform.position = followTarget.position;
        }

        if (playerInput == null)
            return;

        Vector2 look = playerInput.LookInput;
        if (look.sqrMagnitude > 0.0001f)
        {
            float deltaTime = Time.deltaTime;

            yaw += look.x * mouseSensitivityX * deltaTime;

            float yInput = look.y;
            if (!invertY)
            {
                // 일반적으로 마우스를 위로 올리면 화면은 위로 보는 느낌이므로,
                // pitch는 look.y의 부호를 반대로 적용합니다.
                yInput = -yInput;
            }

            pitch += yInput * mouseSensitivityY * deltaTime;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        Quaternion targetRotation = Quaternion.Euler(pitch, yaw, 0f);
        transform.rotation = targetRotation;
    }

    /// <summary>
    /// 인스펙터에서 비어 있을 경우, 주변에서 기본 참조를 찾아 채워줍니다.
    /// </summary>
    private void TryAutoAssignReferences()
    {
        if (followTarget == null)
        {
            // 가장 가까운 PlayerController 또는 PlayerMovement를 플레이어로 간주
            var player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                followTarget = player.transform;
            }
        }

        if (playerInput == null)
        {
            if (followTarget != null)
            {
                playerInput = followTarget.GetComponent<PlayerInput>();
            }

            if (playerInput == null)
            {
                playerInput = FindObjectOfType<PlayerInput>();
            }
        }
    }
}

