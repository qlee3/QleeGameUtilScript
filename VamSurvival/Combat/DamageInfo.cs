using UnityEngine;

/// <summary>
/// 피해 유형을 정의하는 열거형.
/// </summary>
public enum DamageType
{
    /// <summary>접촉에 의한 피해.</summary>
    Contact,

    /// <summary>투사체에 의한 피해.</summary>
    Projectile,

    /// <summary>범위(장판)에 의한 피해.</summary>
    Area,

    /// <summary>대쉬 충돌에 의한 피해.</summary>
    Dash,
}

/// <summary>
/// 피해 정보를 담는 구조체.
/// 피해를 주는 쪽에서 생성하여 IDamageable.TakeDamage()에 전달합니다.
/// </summary>
public struct DamageInfo
{
    /// <summary>피해량.</summary>
    public float Amount;

    /// <summary>피해 유형.</summary>
    public DamageType Type;

    /// <summary>피해 출처 (적 인스턴스, 투사체 등). 출처 추적에 사용.</summary>
    public object Source;

    /// <summary>피격 지점 (월드 좌표).</summary>
    public Vector3 HitPoint;

    /// <summary>피격 방향 (정규화). 넉백 등에 활용.</summary>
    public Vector3 HitDirection;

    public DamageInfo(float amount, DamageType type, object source = null)
    {
        Amount = amount;
        Type = type;
        Source = source;
        HitPoint = Vector3.zero;
        HitDirection = Vector3.zero;
    }
}
