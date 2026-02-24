/// <summary>
/// 게임의 전체 흐름 상태를 정의하는 열거형.
/// GameFlowManager가 이 상태를 기반으로 게임 흐름을 제어합니다.
/// </summary>
public enum GameState
{
    /// <summary>스테이지 로딩 중.</summary>
    Loading,

    /// <summary>플레이 중 (전투 진행).</summary>
    Playing,

    /// <summary>서브스테이지 클리어 (다음 서브스테이지 전환 대기).</summary>
    SubStageClear,

    /// <summary>스테이지 전체 클리어.</summary>
    StageClear,

    /// <summary>플레이어 사망 (게임 오버).</summary>
    GameOver,

    /// <summary>모든 스테이지 클리어 (게임 승리).</summary>
    GameWin,
}
