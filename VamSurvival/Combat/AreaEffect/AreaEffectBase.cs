using System.Collections;
using UnityEngine;

/// <summary>
/// 기본 장판(범위 효과) 컴포넌트.
/// AreaAbilityData.Execute()에서 Instantiate 후 Initialize()를 호출합니다.
/// Physics.OverlapSphere로 반경 내 적을 감지하여 IDamageable.TakeDamage()를 호출합니다.
///
/// 프리팹 설정:
///  - enemyLayerMask: 적 레이어 지정 (Inspector)
///  - damageInterval = 0: 생성 즉시 1회 피해 후 소멸
///  - damageInterval > 0: duration 동안 interval마다 반복 피해 후 소멸
/// </summary>
public class AreaEffectBase : MonoBehaviour
{
    [Header("Layer")]
    [Tooltip("적 레이어 마스크. OverlapSphere 대상 필터링에 사용됩니다.")]
    [SerializeField] private LayerMask enemyLayerMask;

    private float radius;
    private float duration;
    private float damage;
    private float interval;

    /// <summary>
    /// 장판을 초기화합니다. Instantiate 직후 반드시 호출하세요.
    /// </summary>
    public void Initialize(float r, float dur, float dmg, float dmgInterval)
    {
        radius = r;
        duration = dur;
        damage = dmg;
        interval = dmgInterval;

        StartCoroutine(LifetimeRoutine());
    }

    private IEnumerator LifetimeRoutine()
    {
        if (interval <= 0f)
        {
            DealDamage();
            Destroy(gameObject);
            yield break;
        }

        float elapsed = 0f;
        float nextDamageTime = 0f;

        while (elapsed < duration)
        {
            if (elapsed >= nextDamageTime)
            {
                DealDamage();
                nextDamageTime += interval;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

    private void DealDamage()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radius, enemyLayerMask);
        foreach (var hit in hits)
        {
            if (!hit.TryGetComponent<IDamageable>(out var damageable)) continue;
            if (!damageable.IsAlive) continue;

            Vector3 hitDir = (hit.transform.position - transform.position).normalized;
            var info = new DamageInfo(damage, DamageType.Area, this);
            info.HitPoint = hit.transform.position;
            info.HitDirection = hitDir;
            damageable.TakeDamage(info);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.35f);
        Gizmos.DrawSphere(transform.position, radius);
    }
}
