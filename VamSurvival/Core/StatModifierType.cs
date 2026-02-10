/// <summary>
/// 스탯 모디파이어의 적용 방식.
/// 값이 곧 적용 순서(Order 기본값)가 됩니다.
/// 계산 순서: Flat → PercentAdd → PercentMult
/// </summary>
public enum StatModifierType
{
    /// <summary>기본값에 고정 수치를 더합니다. (예: +5)</summary>
    Flat = 100,

    /// <summary>가산 퍼센트. 같은 타입끼리 합산 후 곱합니다. (예: +20% + 10% = +30%)</summary>
    PercentAdd = 200,

    /// <summary>승산 퍼센트. 각각 독립적으로 곱합니다. (예: ×1.2 × 1.3 = ×1.56)</summary>
    PercentMult = 300,
}
