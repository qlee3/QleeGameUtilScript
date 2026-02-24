using System;
using UnityEngine;

/// <summary>
/// 한 웨이브에서 스폰할 적 그룹 하나를 정의합니다.
/// 하나의 웨이브는 여러 WaveConfig(적 그룹)로 구성될 수 있습니다.
/// </summary>
[Serializable]
public class WaveGroup
{
    [Tooltip("스폰할 적 프리팹 (EnemyController 포함)")]
    public EnemyController enemyPrefab;

    [Tooltip("Initialize에 전달할 EnemyData")]
    public EnemyData enemyData;

    [Tooltip("이 그룹에서 스폰할 적 수")]
    [Min(1)] public int spawnCount = 3;

    [Tooltip("한 마리씩 스폰하는 간격 (초)")]
    [Min(0f)] public float spawnInterval = 0.3f;
}

/// <summary>
/// 한 웨이브의 전체 구성을 정의합니다.
/// 웨이브 내 모든 적 그룹이 스폰되고 전멸해야 다음 웨이브로 진행합니다.
/// </summary>
[Serializable]
public class WaveConfig
{
    [Tooltip("이 웨이브에서 스폰할 적 그룹 목록")]
    public WaveGroup[] groups;

    [Tooltip("이전 웨이브 클리어 후 이 웨이브 시작까지 대기 시간 (초)")]
    [Min(0f)] public float delayBeforeStart = 1f;

    /// <summary>이 웨이브에서 스폰할 총 적 수.</summary>
    public int TotalSpawnCount
    {
        get
        {
            if (groups == null) return 0;
            int total = 0;
            for (int i = 0; i < groups.Length; i++)
                total += groups[i].spawnCount;
            return total;
        }
    }
}
