using System;
using UnityEngine;

/// <summary>
/// 플레이어의 레벨과 경험치를 관리하는 컴포넌트.
/// 지수형 경험치 곡선을 사용하며 레벨 제한이 없습니다.
/// 경험치 획득 시 PlayerStats.ExpMultiplier가 자동 적용됩니다.
/// </summary>
public class PlayerLevel : MonoBehaviour
{
    [Header("Exp Curve")]
    [Tooltip("레벨 1→2에 필요한 기본 경험치")]
    [SerializeField] private float baseExpToLevel = 10f;

    [Tooltip("레벨당 필요 경험치 증가율 (지수형). 1.15 = 레벨마다 15% 증가")]
    [SerializeField] private float expGrowthRate = 1.15f;

    // ── 상태 ──

    /// <summary>현재 레벨 (1부터 시작).</summary>
    public int Level { get; private set; } = 1;

    /// <summary>현재 누적 경험치 (현재 레벨 내).</summary>
    public float CurrentExp { get; private set; }

    /// <summary>다음 레벨까지 필요한 총 경험치.</summary>
    public float ExpToNextLevel { get; private set; }

    /// <summary>경험치 진행률 (0~1). UI 바에 바인딩하세요.</summary>
    public float ExpRatio => ExpToNextLevel > 0f ? CurrentExp / ExpToNextLevel : 0f;

    // ── 이벤트 ──

    /// <summary>레벨업 시 호출됩니다. 파라미터: 새 레벨.</summary>
    public event Action<int> OnLevelUp;

    /// <summary>경험치 변동 시 호출됩니다. 파라미터: 진행률(0~1).</summary>
    public event Action<float> OnExpChanged;

    private PlayerStats stats;

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();
        ExpToNextLevel = CalculateExpForLevel(Level);
    }

    /// <summary>
    /// 경험치를 획득합니다.
    /// ExpMultiplier 스탯이 자동으로 곱해집니다.
    /// 한 번에 여러 레벨이 오를 수 있습니다.
    /// </summary>
    /// <param name="rawAmount">모디파이어 적용 전 원본 경험치량.</param>
    public void AddExp(float rawAmount)
    {
        if (rawAmount <= 0f) return;

        float amount = rawAmount * stats.ExpMultiplier.Value;
        CurrentExp += amount;

        // 연속 레벨업 처리
        while (CurrentExp >= ExpToNextLevel)
        {
            CurrentExp -= ExpToNextLevel;
            Level++;
            ExpToNextLevel = CalculateExpForLevel(Level);
            OnLevelUp?.Invoke(Level);
        }

        OnExpChanged?.Invoke(ExpRatio);
    }

    /// <summary>
    /// 지수형 경험치 곡선.
    /// 필요 경험치 = baseExpToLevel * expGrowthRate^(level - 1)
    /// </summary>
    private float CalculateExpForLevel(int level)
    {
        return baseExpToLevel * Mathf.Pow(expGrowthRate, level - 1);
    }
}
