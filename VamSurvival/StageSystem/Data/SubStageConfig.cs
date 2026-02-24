using System;
using UnityEngine;

/// <summary>
/// 서브스테이지 하나의 전체 설정을 정의합니다.
/// 맵 프리팹(장애물 + SpawnPoint 포함)과 웨이브 목록을 가집니다.
/// </summary>
[Serializable]
public class SubStageConfig
{
    [Tooltip("서브스테이지 유형 (Normal / Elite / Boss)")]
    public SubStageType type = SubStageType.Normal;

    [Tooltip("이 서브스테이지에서 사용할 맵 프리팹 (장애물, SpawnPoint 포함)")]
    public GameObject mapPrefab;

    [Tooltip("이 서브스테이지의 웨이브 목록 (순서대로 진행)")]
    public WaveConfig[] waves;
}
