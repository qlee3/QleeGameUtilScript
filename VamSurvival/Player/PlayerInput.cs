using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 플레이어 입력을 처리하는 컴포넌트.
/// New Input System의 InputActionAsset을 참조하여 입력 값을 읽고 외부에 제공합니다.
/// </summary>
public class PlayerInput : MonoBehaviour
{
    [Header("Input Action Asset")]
    [SerializeField] private InputActionAsset inputActions;

    private InputActionMap playerActionMap;
    private InputAction moveAction;
    private InputAction lookAction;

    /// <summary>이동 입력 (WASD / 스틱). 정규화되지 않은 원본 값.</summary>
    public Vector2 MoveInput { get; private set; }

    /// <summary>시선 입력 (마우스 델타 / 우측 스틱).</summary>
    public Vector2 LookInput { get; private set; }

    private void Awake()
    {
        playerActionMap = inputActions.FindActionMap("Player");
        moveAction = playerActionMap.FindAction("Move");
        lookAction = playerActionMap.FindAction("Look");
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
    }
}
