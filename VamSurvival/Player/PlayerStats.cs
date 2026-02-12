using UnityEngine;

/// <summary>
/// 스탯 표시에 필요한 정보를 담는 구조체.
/// </summary>
public struct StatDisplayInfo
{
    public string Name;
    public float BaseValue;
    public float CurrentValue;
    public bool HigherIsBetter;

    public StatDisplayInfo(string name, Stat stat, bool higherIsBetter)
    {
        Name = name;
        BaseValue = stat.BaseValue;
        CurrentValue = stat.Value;
        HigherIsBetter = higherIsBetter;
    }
}

/// <summary>
/// 플레이어의 모든 스탯을 보유하는 컴포넌트.
/// 각 스탯은 Stat 인스턴스로 관리되며, 아이템/버프 등에서 모디파이어를 추가/제거할 수 있습니다.
/// </summary>
public class PlayerStats : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] private float baseMaxHp = 100f;
    [SerializeField] private float baseMoveSpeed = 5f;
    [SerializeField] private float baseAttackPower = 1f;
    [SerializeField] private float baseDashSpeed = 10f;
    [SerializeField] private float baseDashEnergy = 1f;
    [SerializeField] private float baseMagicPower = 1f;
    [SerializeField] private float baseShield = 0f;
    [SerializeField] private float baseDefense = 100f;
    [SerializeField] private float baseAttackCooldown = 1f;
    [SerializeField] private float baseSkillCooldown = 1f;
    [SerializeField] private float baseCritChance = 0.05f;
    [SerializeField] private float baseCritDamage = 1.5f;
    [SerializeField] private float baseExpMultiplier = 1f;

    // ── 스탯 인스턴스 ──

    /// <summary>최대 체력.</summary>
    public Stat MaxHp { get; private set; }

    /// <summary>이동 속도.</summary>
    public Stat MoveSpeed { get; private set; }

    /// <summary>기본 공격력 배율.</summary>
    public Stat AttackPower { get; private set; }

    /// <summary>대시 속도.</summary>
    public Stat DashSpeed { get; private set; }

    /// <summary>대시 에너지 배율.</summary>
    public Stat DashEnergy { get; private set; }

    /// <summary>마법 공격력 배율.</summary>
    public Stat MagicPower { get; private set; }

    /// <summary>방어막.</summary>
    public Stat Shield { get; private set; }

    /// <summary>방어력 배율 (100 기준, 낮을수록 좋음).</summary>
    public Stat Defense { get; private set; }

    /// <summary>기본 공격 쿨타임 배율.</summary>
    public Stat AttackCooldown { get; private set; }

    /// <summary>특수 스킬 쿨타임 배율.</summary>
    public Stat SkillCooldown { get; private set; }

    /// <summary>치명타 확률 (0~1).</summary>
    public Stat CritChance { get; private set; }

    /// <summary>치명타 공격력 배율.</summary>
    public Stat CritDamage { get; private set; }

    /// <summary>경험치 획득 배율 (1 = 100%).</summary>
    public Stat ExpMultiplier { get; private set; }

    private void Awake()
    {
        MaxHp = new Stat(baseMaxHp);
        MoveSpeed = new Stat(baseMoveSpeed);
        AttackPower = new Stat(baseAttackPower);
        DashSpeed = new Stat(baseDashSpeed);
        DashEnergy = new Stat(baseDashEnergy);
        MagicPower = new Stat(baseMagicPower);
        Shield = new Stat(baseShield);
        Defense = new Stat(baseDefense);
        AttackCooldown = new Stat(baseAttackCooldown);
        SkillCooldown = new Stat(baseSkillCooldown);
        CritChance = new Stat(baseCritChance);
        CritDamage = new Stat(baseCritDamage);
        ExpMultiplier = new Stat(baseExpMultiplier);
    }

    /// <summary>
    /// 모든 스탯의 표시 정보를 배열로 반환합니다.
    /// PauseUI 등에서 스탯 목록을 순회하여 표시할 때 사용합니다.
    /// </summary>
    public StatDisplayInfo[] GetAllStatDisplayInfos()
    {
        return new[]
        {
            new StatDisplayInfo("체력", MaxHp, true),
            new StatDisplayInfo("이동속도", MoveSpeed, true),
            new StatDisplayInfo("공격력 배율", AttackPower, true),
            new StatDisplayInfo("대시 속도", DashSpeed, true),
            new StatDisplayInfo("대시 에너지", DashEnergy, true),
            new StatDisplayInfo("마법 공격력", MagicPower, true),
            new StatDisplayInfo("방어막", Shield, true),
            new StatDisplayInfo("방어력", Defense, false),          // 낮을수록 좋음
            new StatDisplayInfo("공격 쿨타임", AttackCooldown, false), // 낮을수록 좋음
            new StatDisplayInfo("스킬 쿨타임", SkillCooldown, false), // 낮을수록 좋음
            new StatDisplayInfo("치명타 확률", CritChance, true),
            new StatDisplayInfo("치명타 배율", CritDamage, true),
            new StatDisplayInfo("경험치 배율", ExpMultiplier, true),
        };
    }
}
