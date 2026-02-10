using System;
using System.Collections.Generic;

/// <summary>
/// 단일 스탯을 표현하는 클래스.
/// 기본값(BaseValue)에 모디파이어 리스트를 적용하여 최종값(Value)을 계산합니다.
/// 계산 순서: (BaseValue + Flat 합계) × (1 + PercentAdd 합계) × PercentMult 곱
/// isDirty 플래그로 변경 시에만 재계산하여 성능을 최적화합니다.
/// </summary>
[Serializable]
public class Stat
{
    public float BaseValue;

    /// <summary>모디파이어가 적용된 최종 스탯 값.</summary>
    public float Value
    {
        get
        {
            if (isDirty)
            {
                cachedValue = CalculateFinalValue();
                isDirty = false;
            }
            return cachedValue;
        }
    }

    /// <summary>현재 적용 중인 모디파이어 수.</summary>
    public int ModifierCount => modifiers.Count;

    private float cachedValue;
    private bool isDirty = true;
    private readonly List<StatModifier> modifiers;

    public Stat(float baseValue = 0f)
    {
        BaseValue = baseValue;
        modifiers = new List<StatModifier>();
    }

    /// <summary>모디파이어를 추가합니다. Order 순서대로 정렬됩니다.</summary>
    public void AddModifier(StatModifier modifier)
    {
        modifiers.Add(modifier);
        modifiers.Sort((a, b) => a.Order.CompareTo(b.Order));
        isDirty = true;
    }

    /// <summary>특정 모디파이어를 제거합니다.</summary>
    public bool RemoveModifier(StatModifier modifier)
    {
        if (modifiers.Remove(modifier))
        {
            isDirty = true;
            return true;
        }
        return false;
    }

    /// <summary>특정 출처(Source)의 모든 모디파이어를 제거합니다.</summary>
    public bool RemoveAllModifiersFromSource(object source)
    {
        bool removed = false;
        for (int i = modifiers.Count - 1; i >= 0; i--)
        {
            if (modifiers[i].Source == source)
            {
                modifiers.RemoveAt(i);
                removed = true;
            }
        }

        if (removed)
        {
            isDirty = true;
        }
        return removed;
    }

    /// <summary>
    /// 최종 값을 계산합니다.
    /// 순서: (BaseValue + Flat) × (1 + PercentAdd 합) × PercentMult 곱
    /// </summary>
    private float CalculateFinalValue()
    {
        float finalValue = BaseValue;
        float percentAddSum = 0f;

        for (int i = 0; i < modifiers.Count; i++)
        {
            StatModifier mod = modifiers[i];

            switch (mod.Type)
            {
                case StatModifierType.Flat:
                    finalValue += mod.Value;
                    break;

                case StatModifierType.PercentAdd:
                    percentAddSum += mod.Value;

                    // 다음 모디파이어가 PercentAdd가 아니거나 마지막이면 합산 적용
                    if (i + 1 >= modifiers.Count || modifiers[i + 1].Type != StatModifierType.PercentAdd)
                    {
                        finalValue *= (1f + percentAddSum);
                        percentAddSum = 0f;
                    }
                    break;

                case StatModifierType.PercentMult:
                    finalValue *= (1f + mod.Value);
                    break;
            }
        }

        // 부동소수점 오차 보정 (소수점 4자리)
        return (float)Math.Round(finalValue, 4);
    }
}
