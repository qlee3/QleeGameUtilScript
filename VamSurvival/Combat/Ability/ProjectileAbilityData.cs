using UnityEngine;

/// <summary>
/// 투사체 어빌리티 데이터.
/// 플레이어의 forward 방향으로 투사체를 발사합니다.
/// projectileCount > 1이면 spreadAngle 간격으로 부채꼴 발사합니다.
/// </summary>
[CreateAssetMenu(menuName = "VamSurvival/Ability/Projectile")]
public class ProjectileAbilityData : PlayerAbilityData
{
    [Header("투사체")]
    [Tooltip("발사할 투사체 프리팹. ProjectileBase 컴포넌트가 있어야 합니다.")]
    public GameObject projectilePrefab;

    [Tooltip("투사체 이동 속도")]
    public float projectileSpeed = 15f;

    [Tooltip("동시에 발사할 투사체 수")]
    public int projectileCount = 1;

    [Tooltip("projectileCount > 1일 때 발사체 간 각도 간격 (도)")]
    public float spreadAngle = 15f;

    [Header("자동 타겟팅")]
    [Tooltip("true면 가장 가까운 살아있는 EnemyHealth 방향으로 즉시 회전한 뒤 발사합니다.")]
    public bool useAutoTargeting;

    public bool TryGetAutoTargetDirection(PlayerController player, out Vector3 direction)
    {
        direction = Vector3.zero;

        if (!useAutoTargeting || player == null)
            return false;

        EnemyHealth[] enemies = Object.FindObjectsByType<EnemyHealth>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None);

        Transform originTransform = player.Combat != null && player.Combat.FirePoint != null
            ? player.Combat.FirePoint
            : player.transform;

        Vector3 origin = originTransform.position;
        EnemyHealth nearestEnemy = null;
        float nearestDistanceSqr = float.MaxValue;

        foreach (EnemyHealth enemy in enemies)
        {
            if (enemy == null || !enemy.IsAlive || !enemy.gameObject.activeInHierarchy)
                continue;

            Vector3 toEnemy = enemy.transform.position - origin;
            toEnemy.y = 0f;

            float distanceSqr = toEnemy.sqrMagnitude;
            if (distanceSqr < 0.001f || distanceSqr >= nearestDistanceSqr)
                continue;

            nearestDistanceSqr = distanceSqr;
            nearestEnemy = enemy;
            direction = toEnemy.normalized;
        }

        return nearestEnemy != null;
    }

    public override void Execute(PlayerController player)
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning($"[{name}] projectilePrefab이 설정되지 않았습니다.");
            return;
        }

        float damage = CalculateDamage(player);
        Transform fp = player.Combat.FirePoint;
        bool hasFirePoint = fp != null;
        Vector3 origin = hasFirePoint ? fp.position : player.transform.position;
        Vector3 forward = hasFirePoint ? fp.forward : player.transform.forward;

        if (TryGetAutoTargetDirection(player, out Vector3 autoTargetDirection))
            forward = autoTargetDirection;

        forward.y = 0f;

        if (projectileCount <= 1)
        {
            SpawnProjectile(origin, forward.normalized, damage);
            return;
        }

        float totalAngle = spreadAngle * (projectileCount - 1);
        float startAngle = -totalAngle / 2f;
        for (int i = 0; i < projectileCount; i++)
        {
            float angle = startAngle + spreadAngle * i;
            Vector3 dir = Quaternion.Euler(0f, angle, 0f) * forward.normalized;
            SpawnProjectile(origin, dir, damage);
        }
    }

    private void SpawnProjectile(Vector3 origin, Vector3 direction, float damage)
    {
        if (CombatPoolManager.Instance != null)
        {
            var proj = CombatPoolManager.Instance.GetProjectile(
                projectilePrefab, origin, Quaternion.LookRotation(direction));
            if (proj != null)
                proj.Initialize(direction, projectileSpeed, damage, origin);
        }
        else
        {
            GameObject go = Object.Instantiate(projectilePrefab, origin, Quaternion.LookRotation(direction));
            if (go.TryGetComponent<ProjectileBase>(out var proj))
                proj.Initialize(direction, projectileSpeed, damage, origin);
        }
    }
}
