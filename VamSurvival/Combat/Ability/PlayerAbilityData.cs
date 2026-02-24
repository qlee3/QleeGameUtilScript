using UnityEngine;

/// <summary>
/// 플레이어 어빌리티(기본공격/스킬1/스킬2)의 추상 베이스 ScriptableObject.
/// 서브클래스(ProjectileAbilityData, AreaAbilityData, DashAbilityData 등)가
/// 고유 필드와 Execute 로직을 구현합니다.
///
/// PlayerCombat의 슬롯(attackData, skill1Data, skill2Data)은 이 타입으로 참조하므로
/// 어떤 슬롯에든 어떤 타입이든 자유롭게 장착할 수 있습니다.
/// </summary>
public abstract class PlayerAbilityData : ScriptableObject
{
    [Header("공통")]
    [Tooltip("기본 피해량. useAttackPowerStat에 따라 AttackPower 또는 MagicPower 배율이 곱해집니다.")]
    public float baseDamage = 10f;

    [Tooltip("쿨타임(초). Attack 슬롯은 AttackCooldown 스탯, Skill 슬롯은 SkillCooldown 스탯 배율이 곱해집니다.")]
    public float cooldown = 1f;

    [Tooltip("true = AttackPower 스탯 기반, false = MagicPower 스탯 기반으로 피해 계산")]
    public bool useAttackPowerStat = true;

    [Tooltip("true면 이동기. PlayerAbilityState에서 이동 입력을 차단하지 않습니다.")]
    public bool isMovementAbility;

    [Tooltip("HUD에 표시할 스킬 아이콘")]
    public Sprite icon;

    /// <summary>
    /// 어빌리티를 실행합니다.
    /// AnimationEventReceiver의 Execute 이벤트 타이밍에 PlayerCombat을 통해 호출됩니다.
    /// </summary>
    public abstract void Execute(PlayerController player);

    /// <summary>
    /// 스탯 배율과 크리티컬을 적용한 최종 피해량을 계산합니다.
    /// 서브클래스의 Execute()에서 호출하세요.
    /// </summary>
    protected float CalculateDamage(PlayerController player)
    {
        float power = useAttackPowerStat
            ? player.Stats.AttackPower.Value
            : player.Stats.MagicPower.Value;

        float damage = baseDamage * power;

        if (Random.value < player.Stats.CritChance.Value)
            damage *= player.Stats.CritDamage.Value;

        return damage;
    }
}
