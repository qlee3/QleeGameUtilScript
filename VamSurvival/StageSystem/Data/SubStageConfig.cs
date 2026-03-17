using System;
using UnityEngine;

/// <summary>
/// 서브스테이지 하나의 전체 설정을 정의합니다.
/// 맵 프리팹과 스폰 레이아웃을 가집니다.
/// </summary>
[Serializable]
public class SubStageConfig
{
    [Tooltip("서브스테이지 유형 (Normal / Elite / Boss)")]
    public SubStageType type = SubStageType.Normal;

    [Tooltip("이 서브스테이지에서 사용할 맵 프리팹")]
    public GameObject mapPrefab;

    [Tooltip("이 서브스테이지에서 사용할 스폰 레이아웃")]
    public SpawnLayoutData spawnLayout;
}
