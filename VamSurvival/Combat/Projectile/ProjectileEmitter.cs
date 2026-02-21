using UnityEngine;

/// <summary>
/// 공용 투사체 발사 컴포넌트.
/// AnimationEventReceiver.onFireProjectile 등에서 직접 호출할 수 있습니다.
/// </summary>
public class ProjectileEmitter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ProjectileData projectileData;
    [SerializeField] private Transform firePoint;
    [SerializeField] private ProjectilePool projectilePool;

    [Header("Homing (Optional)")]
    [SerializeField] private Transform homingTarget;

    [Header("Owner")]
    [Tooltip("비어 있으면 이 컴포넌트가 붙은 오브젝트를 Source로 사용")]
    [SerializeField] private Object sourceOverride;

    private PlayerController playerController;
    private EnemyController enemyController;

    private void Awake()
    {
        if (firePoint == null)
        {
            firePoint = transform;
        }

        if (projectilePool == null)
        {
            projectilePool = ProjectilePool.Instance;
        }

        playerController = GetComponentInParent<PlayerController>();
        enemyController = GetComponentInParent<EnemyController>();
    }

    /// <summary>
    /// firePoint의 forward 방향으로 투사체를 발사합니다.
    /// </summary>
    public void Fire()
    {
        Vector3 direction = firePoint != null ? firePoint.forward : transform.forward;
        Fire(direction, homingTarget);
    }

    /// <summary>
    /// 지정 방향으로 투사체를 발사합니다.
    /// </summary>
    public void Fire(Vector3 direction, Transform target = null)
    {
        if (projectileData == null)
        {
            Debug.LogWarning("[ProjectileEmitter] projectileData가 비어 있습니다.");
            return;
        }

        if (projectilePool == null)
        {
            projectilePool = ProjectilePool.Instance;
            if (projectilePool == null)
            {
                Debug.LogWarning("[ProjectileEmitter] ProjectilePool 인스턴스를 찾을 수 없습니다.");
                return;
            }
        }

        Transform origin = firePoint != null ? firePoint : transform;
        object source = sourceOverride != null ? sourceOverride : gameObject;
        Transform finalTarget = target != null ? target : homingTarget;
        float finalDamage = CalculateRuntimeDamage();

        projectilePool.Spawn(
            projectileData,
            origin.position,
            origin.rotation,
            source,
            direction,
            finalTarget,
            finalDamage
        );
    }

    /// <summary>
    /// Homing 타겟을 런타임에 교체합니다.
    /// </summary>
    public void SetHomingTarget(Transform target)
    {
        homingTarget = target;
    }

    private float CalculateRuntimeDamage()
    {
        float damage = projectileData.damage;

        // Player 발사체: 공격/마법 배율 + 크리티컬 적용
        if (playerController != null && playerController.Stats != null)
        {
            PlayerStats stats = playerController.Stats;

            float scale = 1f;
            switch (projectileData.damageScaling)
            {
                case ProjectileDamageScaling.AttackPower:
                    scale = stats.AttackPower.Value;
                    break;
                case ProjectileDamageScaling.MagicPower:
                    scale = stats.MagicPower.Value;
                    break;
                case ProjectileDamageScaling.None:
                default:
                    scale = 1f;
                    break;
            }

            damage *= scale;

            float critChance = Mathf.Clamp01(stats.CritChance.Value);
            float critMultiplier = Mathf.Max(0f, stats.CritDamage.Value);
            if (critChance > 0f && critMultiplier > 0f && Random.value <= critChance)
            {
                damage *= critMultiplier;
            }

            return damage;
        }

        // Enemy 발사체: 요청 조건대로 공격/마법 배율 1, 크리티컬 확률 0, 크리 배율 0.
        if (enemyController != null)
        {
            const float enemyDamageScale = 1f;
            const float enemyCritChance = 0f;
            const float enemyCritMultiplier = 0f;

            damage *= enemyDamageScale;
            if (enemyCritChance > 0f && enemyCritMultiplier > 0f && Random.value <= enemyCritChance)
            {
                damage *= enemyCritMultiplier;
            }

            return damage;
        }

        // 소유자 미식별 시 데이터 기본값 사용
        return damage;
    }
}
