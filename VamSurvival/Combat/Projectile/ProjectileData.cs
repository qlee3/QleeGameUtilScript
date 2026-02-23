using UnityEngine;

/// <summary>
/// 투사체 동작/피해 설정 데이터.
/// ScriptableObject로 만들어 플레이어/적 모두에서 공용 사용합니다.
/// </summary>
[CreateAssetMenu(fileName = "ProjectileData", menuName = "VamSurvival/Combat/Projectile Data")]
public class ProjectileData : ScriptableObject
{
    [Header("Prefab")]
    [Tooltip("이 데이터를 사용하는 투사체 프리팹")]
    public ProjectileBase projectilePrefab;

    [Header("Damage")]
    [Tooltip("기본 피해량")]
    public float damage = 10f;

    [Tooltip("피해를 줄 대상 레이어")]
    public LayerMask targetLayers;

    [Tooltip("관통 횟수 (0 = 1회 타격 후 종료, 1 = 2명 타격 가능)")]
    [Min(0)] public int penetrationCount = 0;

    [Tooltip("같은 대상에 중복 타격 허용 여부")]
    public bool allowMultiHitSameTarget;

    [Tooltip("플레이어 발사 시 적용할 공격력 스케일링")]
    public ProjectileDamageScaling damageScaling = ProjectileDamageScaling.AttackPower;

    [Header("Movement")]
    public ProjectileMovementType movementType = ProjectileMovementType.Straight;

    [Tooltip("이동 속도")]
    [Min(0.1f)] public float speed = 12f;

    [Tooltip("유도 회전 속도(도/초). Homing에서만 사용")]
    [Min(0f)] public float homingTurnRate = 360f;

    [Header("Lifetime")]
    [Tooltip("생존 시간(초)")]
    [Min(0.05f)] public float lifeTime = 3f;

    private void OnValidate()
    {
        // 기본 충돌 레이어는 Wall을 사용합니다.
        if (targetLayers.value != 0) return;

        int wallLayer = LayerMask.NameToLayer("Wall");
        if (wallLayer >= 0)
        {
            targetLayers = 1 << wallLayer;
        }
    }
}

public enum ProjectileDamageScaling
{
    None = 0,
    AttackPower = 1,
    MagicPower = 2,
}

public enum ProjectileMovementType
{
    Straight = 0,
    Homing = 1,
}
