using UnityEngine;

/// <summary>
/// 대시/이동기 어빌리티 데이터.
/// 플레이어를 forward 방향으로 빠르게 이동시킵니다.
/// isMovementAbility = true이므로 PlayerAbilityState에서 이동 입력을 차단하지 않습니다.
/// </summary>
[CreateAssetMenu(menuName = "VamSurvival/Ability/Dash")]
public class DashAbilityData : PlayerAbilityData
{
    [Header("대시")]
    [Tooltip("대시 이동 거리")]
    public float dashDistance = 5f;

    [Tooltip("대시 이동 속도")]
    public float dashSpeed = 20f;

    [Tooltip("true면 대시 중 경로상 적에게도 피해를 줍니다")]
    public bool dealsDamageDuringDash;

    private void OnEnable()
    {
        isMovementAbility = true;
    }

    public override void Execute(PlayerController player)
    {
        float damage = dealsDamageDuringDash ? CalculateDamage(player) : 0f;
        player.Movement.ApplyDash(player.transform.forward, dashSpeed, dashDistance, damage);
    }
}
