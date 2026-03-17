using UnityEngine;

/// <summary>
/// 한 서브스테이지에서 사용할 스폰 배치 데이터.
/// </summary>
[CreateAssetMenu(fileName = "NewSpawnLayout", menuName = "VamSurvival/Spawn Layout")]
public class SpawnLayoutData : ScriptableObject
{
    [Tooltip("이 레이아웃의 스폰 포인트 정의 목록")]
    public SpawnPointDefinition[] points;

    public int MaxWaveIndex
    {
        get
        {
            if (points == null || points.Length == 0)
                return -1;

            int max = -1;
            for (int i = 0; i < points.Length; i++)
            {
                if (points[i] != null && points[i].waveIndex > max)
                    max = points[i].waveIndex;
            }

            return max;
        }
    }

    public int TotalWaveCount => MaxWaveIndex + 1;
}
