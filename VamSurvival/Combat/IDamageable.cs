/// <summary>
/// 피해를 받을 수 있는 엔티티가 구현하는 인터페이스.
/// EnemyHealth, PlayerHealth 등에서 구현합니다.
/// 투사체, 접촉 피해 등은 이 인터페이스만 알면 되므로,
/// 피해를 주는 쪽과 받는 쪽이 완전히 분리됩니다.
/// </summary>
public interface IDamageable
{
    /// <summary>피해를 받습니다.</summary>
    void TakeDamage(DamageInfo damage);

    /// <summary>현재 살아있는지 여부.</summary>
    bool IsAlive { get; }
}
