using UnityEngine;

/// <summary>
/// 피격 시 스킬 발동을 시도하는 트리거.
/// PlayerHealth.OnDamaged 이벤트를 사용합니다.
/// </summary>
public class OnDamagedSkillTrigger : ProjectileSkillTrigger
{
    [SerializeField] private PlayerHealth playerHealth;

    protected override void Awake()
    {
        base.Awake();

        if (playerHealth == null)
        {
            playerHealth = GetComponentInParent<PlayerHealth>();
        }
    }

    private void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnDamaged += HandleDamaged;
        }
    }

    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnDamaged -= HandleDamaged;
        }
    }

    private void HandleDamaged(DamageInfo _)
    {
        TriggerCast();
    }
}
