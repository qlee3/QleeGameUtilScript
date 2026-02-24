/// <summary>
/// 서브스테이지의 유형을 정의하는 열거형.
/// StageWaveSpawner가 유형에 따라 스폰 포인트 선택 전략을 달리합니다.
/// </summary>
public enum SubStageType
{
    /// <summary>일반 전투 방.</summary>
    Normal,

    /// <summary>강화된 엘리트 적이 등장하는 방.</summary>
    Elite,

    /// <summary>보스가 등장하는 방.</summary>
    Boss,
}
