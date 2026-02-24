using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 플레이어 입력을 처리하는 컴포넌트.
/// New Input System의 InputActionAsset을 참조하여 입력 값을 읽고 외부에 제공합니다.
///
/// InputActionAsset 설정:
///   "Player" 액션맵에 아래 액션이 필요합니다.
///   - Move (Vector2), Look (Vector2)
///   - Attack (Button): 기본공격 입력 (예: LMB, Gamepad South)
///   - Skill1 (Button): 스킬1 입력 (예: Q, Gamepad West)
///   - Skill2 (Button): 스킬2 입력 (예: E, Gamepad East)
///   Skill1/Skill2가 없으면 해당 입력은 무시됩니다.
/// </summary>
public class PlayerInput : MonoBehaviour
{
    [Header("Input Action Asset")]
    [SerializeField] private InputActionAsset inputActions;

    private InputActionMap playerActionMap;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction attackAction;
    private InputAction skill1Action;
    private InputAction skill2Action;

    /// <summary>이동 입력 (WASD / 스틱). 정규화되지 않은 원본 값.</summary>
    public Vector2 MoveInput { get; private set; }

    /// <summary>시선 입력 (마우스 델타 / 우측 스틱).</summary>
    public Vector2 LookInput { get; private set; }

    /// <summary>기본공격 버튼이 이 프레임에 눌렸는지 여부.</summary>
    public bool AttackPressed { get; private set; }

    /// <summary>스킬1 버튼이 이 프레임에 눌렸는지 여부.</summary>
    public bool Skill1Pressed { get; private set; }

    /// <summary>스킬2 버튼이 이 프레임에 눌렸는지 여부.</summary>
    public bool Skill2Pressed { get; private set; }

    private void Awake()
    {
        playerActionMap = inputActions.FindActionMap("Player");
        moveAction   = playerActionMap.FindAction("Move");
        lookAction   = playerActionMap.FindAction("Look");
        attackAction = playerActionMap.FindAction("Attack");
        skill1Action = playerActionMap.FindAction("Skill1");
        skill2Action = playerActionMap.FindAction("Skill2");
    }

    private void OnEnable()
    {
        playerActionMap?.Enable();
    }

    private void OnDisable()
    {
        playerActionMap?.Disable();
    }

    private void Update()
    {
        MoveInput = moveAction.ReadValue<Vector2>();
        LookInput = lookAction.ReadValue<Vector2>();

        AttackPressed = attackAction != null && attackAction.WasPressedThisFrame();
        Skill1Pressed = skill1Action != null && skill1Action.WasPressedThisFrame();
        Skill2Pressed = skill2Action != null && skill2Action.WasPressedThisFrame();
    }
}
