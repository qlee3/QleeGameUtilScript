using System;
using UnityEngine;

/// <summary>
/// 적의 체력을 관리하는 컴포넌트.
/// IDamageable을 구현하여 투사체, 접촉 피해 등으로부터 피해를 받을 수 있습니다.
/// 사망 시 경험치 젬 스폰을 트리거합니다.
/// </summary>
public class EnemyHealth : MonoBehaviour, IDamageable
{
    private EnemyStats stats;
    private float currentHp;
    private bool isInvincible;

    /// <summary>현재 살아있는지 여부.</summary>
    public bool IsAlive => currentHp > 0f;

    /// <summary>현재 체력 비율 (0~1).</summary>
    public float HpRatio => stats != null ? Mathf.Clamp01(currentHp / stats.MaxHp.Value) : 0f;

    /// <summary>피해를 받았을 때 발생. (DamageInfo)</summary>
    public event Action<DamageInfo> OnDamaged;

    /// <summary>사망 시 발생.</summary>
    public event Action OnDeath;

    /// <summary>
    /// 체력을 최대치로 (재)초기화합니다.
    /// 풀링 재사용 시 호출됩니다.
    /// </summary>
    public void Initialize(EnemyStats enemyStats)
    {
        stats = enemyStats;
        currentHp = stats.MaxHp.Value;
        isInvincible = false;
    }

    /// <summary>
    /// 피해를 받습니다. 무적 상태이거나 이미 사망했으면 무시합니다.
    /// </summary>
    public void TakeDamage(DamageInfo damage)
    {
        if (!IsAlive || isInvincible) return;

        currentHp = Mathf.Max(0f, currentHp - damage.Amount);

        OnDamaged?.Invoke(damage);

        if (!IsAlive)
        {
            Die();
        }
    }

    /// <summary>무적 상태를 설정합니다. 스폰 연출 중 사용.</summary>
    public void SetInvincible(bool invincible)
    {
        isInvincible = invincible;
    }

    /// <summary>체력을 회복합니다 (패시브스킬형의 힐 오라 등).</summary>
    public void Heal(float amount)
    {
        if (!IsAlive) return;
        currentHp = Mathf.Min(currentHp + amount, stats.MaxHp.Value);
    }

    private void Die()
    {
        OnDeath?.Invoke();
    }
}
