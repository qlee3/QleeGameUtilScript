using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 시간 기반 스폰 엔트리 정의.
/// 지정 시간 구간에서 interval마다 countPerSpawn만큼 스폰을 시도합니다.
/// </summary>
[Serializable]
public class EnemySpawnEntry
{
    [Header("Spawn Target")]
    [Tooltip("스폰할 적 프리팹 (EnemyController 포함)")]
    public EnemyController enemyPrefab;

    [Tooltip("Initialize에 전달할 EnemyData")]
    public EnemyData enemyData;

    [Header("Time Window")]
    [Tooltip("이 엔트리가 활성화되는 시작 시간(초)")]
    [Min(0f)] public float startTime = 0f;

    [Tooltip("이 엔트리가 비활성화되는 종료 시간(초)")]
    [Min(0f)] public float endTime = 999f;

    [Header("Spawn Rules")]
    [Tooltip("스폰 간격(초)")]
    [Min(0.05f)] public float spawnInterval = 2f;

    [Tooltip("한 번에 스폰할 개수")]
    [Min(1)] public int countPerSpawn = 1;

    [Tooltip("이 엔트리로 동시에 살아있을 수 있는 최대 수")]
    [Min(1)] public int maxAlive = 25;

    [Tooltip("동시에 여러 엔트리가 스폰 가능할 때 선택 가중치")]
    [Min(0f)] public float weight = 1f;
}

/// <summary>
/// 적 스폰 웨이브 데이터.
/// EnemySpawnManager가 참조하여 시간 기반 스폰 규칙을 적용합니다.
/// </summary>
[CreateAssetMenu(fileName = "EnemyWaveData", menuName = "VamSurvival/Enemy Wave Data")]
public class EnemyWaveData : ScriptableObject
{
    [SerializeField] private List<EnemySpawnEntry> entries = new();

    public IReadOnlyList<EnemySpawnEntry> Entries => entries;
    public int Count => entries != null ? entries.Count : 0;

    public EnemySpawnEntry GetEntry(int index)
    {
        return entries[index];
    }
}
