/// <summary>
/// 스탯에 적용되는 단일 수정치.
/// Value, Type, Order, Source로 구성됩니다.
/// Source를 통해 특정 출처(아이템, 버프 등)의 모디파이어를 일괄 제거할 수 있습니다.
/// </summary>
public class StatModifier
{
    /// <summary>수정 수치. Flat: 고정값, Percent: 비율(0.2 = 20%)</summary>
    public readonly float Value;

    /// <summary>적용 방식 (Flat, PercentAdd, PercentMult)</summary>
    public readonly StatModifierType Type;

    /// <summary>같은 타입 내 적용 순서. 기본값은 Type의 int 값.</summary>
    public readonly int Order;

    /// <summary>이 모디파이어를 건 주체 (아이템, 버프, 스킬 등). 제거 시 식별자로 사용.</summary>
    public readonly object Source;

    public StatModifier(float value, StatModifierType type, int order, object source)
    {
        Value = value;
        Type = type;
        Order = order;
        Source = source;
    }

    /// <summary>Order를 Type 기본값으로 설정하는 간편 생성자.</summary>
    public StatModifier(float value, StatModifierType type, object source = null)
        : this(value, type, (int)type, source) { }
}
