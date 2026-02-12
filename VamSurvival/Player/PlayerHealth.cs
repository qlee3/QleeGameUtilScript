using System;
using UnityEngine;

/// <summary>
/// 플레이어의 체력을 관리하는 컴포넌트.
/// IDamageable을 구현하여 적 접촉 피해, 투사체 등으로부터 피해를 받을 수 있습니다.
/// Defense(방어력)과 Shield(방어막) 스탯을 적용합니다.
/// </summary>
public class PlayerHealth : MonoBehaviour, IDamageable
{
    private PlayerStats stats;
    private float currentHp;
    private float currentShield;
    private bool isInvincible;

    /// <summary>현재 살아있는지 여부.</summary>
    public bool IsAlive => currentHp > 0f;

    /// <summary>현재 체력 값.</summary>
    public float CurrentHp => currentHp;

    /// <summary>최대 체력 값.</summary>
    public float MaxHp => stats != null ? stats.MaxHp.Value : 0f;

    /// <summary>현재 체력 비율 (0~1).</summary>
    public float HpRatio => stats != null ? Mathf.Clamp01(currentHp / stats.MaxHp.Value) : 0f;

    /// <summary>피해를 받았을 때 발생. (DamageInfo)</summary>
    public event Action<DamageInfo> OnDamaged;

    /// <summary>체력 값이 변할 때 발생. (currentHp, maxHp)</summary>
    public event Action<float, float> OnHpChanged;

    /// <summary>사망 시 발생.</summary>
    public event Action OnDeath;

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();
    }

    private void Start()
    {
        currentHp = stats.MaxHp.Value;
        currentShield = stats.Shield.Value;
        NotifyHpChanged();
    }

    /// <summary>
    /// 피해를 받습니다. 무적 상태이거나 이미 사망했으면 무시합니다.
    /// 방어막 → 방어력 순으로 적용합니다.
    /// </summary>
    public void TakeDamage(DamageInfo damage)
    {
        if (!IsAlive || isInvincible) return;

        // 방어력 적용: Defense 100 = 100% 피해, Defense 200 = 50% 피해
        float rawDamage = damage.Amount * (100f / Mathf.Max(1f, stats.Defense.Value));

        // 방어막 먼저 흡수
        float shieldAbsorb = Mathf.Min(rawDamage, currentShield);
        currentShield -= shieldAbsorb;
        float hpDamage = rawDamage - shieldAbsorb;

        currentHp = Mathf.Max(0f, currentHp - hpDamage);

        OnDamaged?.Invoke(damage);
        NotifyHpChanged();

        if (!IsAlive)
        {
            Die();
        }
    }

    /// <summary>무적 상태를 설정합니다. 스폰, 대시 등에 사용.</summary>
    public void SetInvincible(bool invincible)
    {
        isInvincible = invincible;
    }

    /// <summary>체력을 회복합니다.</summary>
    public void Heal(float amount)
    {
        if (!IsAlive) return;
        if (amount <= 0f) return;
        currentHp = Mathf.Min(currentHp + amount, stats.MaxHp.Value);
        NotifyHpChanged();
    }

    /// <summary>방어막을 회복합니다.</summary>
    public void RestoreShield(float amount)
    {
        if (!IsAlive) return;
        currentShield = Mathf.Min(currentShield + amount, stats.Shield.Value);
    }

    private void Die()
    {
        OnDeath?.Invoke();
    }

    private void NotifyHpChanged()
    {
        OnHpChanged?.Invoke(currentHp, MaxHp);
    }
}
