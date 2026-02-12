using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 접촉 피해를 처리하는 컴포넌트.
/// 적 GameObject에 부착하며, Trigger 콜라이더와 함께 사용합니다.
///
/// FSM 상태와 독립적으로 동작합니다:
/// - 기본: baseDamage로 접촉 피해
/// - 대쉬 중: damageMultiplier를 올려 증가된 피해
///
/// 대상은 IDamageable 인터페이스만 알면 되므로,
/// 플레이어/적 구분은 Physics Layer Collision Matrix로 처리합니다.
/// </summary>
public class ContactDamage : MonoBehaviour
{
    private float baseDamage;
    private float damageInterval;
    private float damageMultiplier = 1f;

    private readonly Dictionary<IDamageable, float> lastDamageTime = new();

    /// <summary>
    /// EnemyController.Initialize()에서 호출합니다.
    /// </summary>
    public void Initialize(float damage, float interval)
    {
        baseDamage = damage;
        damageInterval = interval;
        damageMultiplier = 1f;
        lastDamageTime.Clear();
    }

    /// <summary>
    /// 피해 배율을 설정합니다.
    /// DashState 진입 시 증가, 퇴장 시 1로 복원합니다.
    /// </summary>
    public void SetDamageMultiplier(float multiplier)
    {
        damageMultiplier = multiplier;
    }

    /// <summary>
    /// 접촉 피해를 활성화/비활성화합니다.
    /// 사망 시 비활성화하여 더 이상 피해를 주지 않도록 합니다.
    /// </summary>
    public void SetActive(bool active)
    {
        enabled = active;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!enabled) return;

        if (!other.TryGetComponent<IDamageable>(out var target)) return;
        if (!target.IsAlive) return;

        // 피해 간격 체크 (연속 피해 방지)
        if (lastDamageTime.TryGetValue(target, out float lastTime)
            && Time.time - lastTime < damageInterval)
        {
            return;
        }

        lastDamageTime[target] = Time.time;

        float finalDamage = baseDamage * damageMultiplier;
        DamageType type = damageMultiplier > 1f ? DamageType.Dash : DamageType.Contact;

        target.TakeDamage(new DamageInfo
        {
            Amount = finalDamage,
            Type = type,
            Source = this,
            HitPoint = other.ClosestPoint(transform.position),
            HitDirection = (other.transform.position - transform.position).normalized,
        });
    }

    /// <summary>
    /// Trigger 영역을 벗어난 대상의 타이머를 제거합니다.
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<IDamageable>(out var target))
        {
            lastDamageTime.Remove(target);
        }
    }
}
