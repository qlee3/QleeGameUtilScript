/// <summary>
/// 적의 행동 유형을 정의하는 열거형.
/// EnemyData SO에서 선택하며, EnemyController가 초기화 시 해당 타입에 맞는 상태 셋을 구성합니다.
/// </summary>
public enum EnemyType
{
    /// <summary>추격형: 플레이어를 지속 추격. 간헐적 정지. 접촉 시 피격. 가장 약함.</summary>
    Chaser,

    /// <summary>대쉬형: 준비 시간 후 플레이어 방향으로 고속 대쉬. 대쉬 패턴은 개체마다 다름.</summary>
    Dasher,

    /// <summary>원거리형: 사거리까지 접근 후 정지, 투사체 발사. 투사체 타입은 개체마다 다름.</summary>
    Ranged,

    /// <summary>무지성 공격형: 추격하면서 N초마다 정해진 스킬 사용.</summary>
    Mindless,

    /// <summary>소환형: 다른 적을 소환.</summary>
    Summoner,

    /// <summary>패시브스킬형: 상시 패시브 효과 발동 (회복, 감속 등).</summary>
    Passive,

    /// <summary>시간별 보스: 복수의 패턴을 보유한 보스 몬스터.</summary>
    Boss,
}
