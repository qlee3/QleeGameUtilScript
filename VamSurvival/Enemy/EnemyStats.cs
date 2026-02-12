using UnityEngine;

/// <summary>
/// 적의 스탯을 보유하는 컴포넌트.
/// EnemyData SO의 기본값으로 초기화되며, 모디파이어를 통해 런타임에 변경 가능합니다.
/// 풀링 재사용 시 Initialize()로 스탯을 재설정합니다.
/// </summary>
public class EnemyStats : MonoBehaviour
{
    // ── 스탯 인스턴스 ──

    /// <summary>최대 체력.</summary>
    public Stat MaxHp { get; private set; }

    /// <summary>이동 속도.</summary>
    public Stat MoveSpeed { get; private set; }

    /// <summary>접촉 피해량.</summary>
    public Stat ContactDamage { get; private set; }

    /// <summary>
    /// EnemyData의 기본값으로 모든 스탯을 (재)초기화합니다.
    /// Awake가 아닌 이 메서드를 사용하는 이유:
    /// 오브젝트 풀에서 동일 프리팹이 다른 EnemyData로 재사용될 수 있기 때문입니다.
    /// </summary>
    public void Initialize(EnemyData data)
    {
        MaxHp = new Stat(data.maxHp);
        MoveSpeed = new Stat(data.moveSpeed);
        ContactDamage = new Stat(data.contactDamage);
    }
}
