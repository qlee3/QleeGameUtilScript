using System;
using UnityEngine;

[Serializable]
public class SpawnPointDefinition
{
    [Tooltip("스폰 위치")]
    public Vector3 position;

    [Tooltip("스폰 포인트 타입")]
    public SpawnPointType type = SpawnPointType.Normal;

    [Tooltip("몇 번째 웨이브에서 스폰할지 (0부터 시작)")]
    [Min(0)] public int waveIndex = 0;

    [Tooltip("스폰할 적 프리팹")]
    public EnemyController enemyPrefab;

    [Tooltip("Initialize에 전달할 EnemyData")]
    public EnemyData enemyData;

    [Tooltip("해당 포인트에서 스폰할 적 수")]
    [Min(1)] public int spawnCount = 1;

    [Tooltip("적 1마리씩 스폰하는 간격")]
    [Min(0f)] public float spawnInterval = 0.3f;
}
