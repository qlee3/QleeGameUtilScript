using UnityEngine;

/// <summary>
/// 모든 스테이지를 관리하는 레지스트리 ScriptableObject.
/// GameFlowManager가 참조하여 스테이지를 순서대로 진행합니다.
/// </summary>
[CreateAssetMenu(fileName = "StageDatabase", menuName = "VamSurvival/Stage Database")]
public class StageDatabase : ScriptableObject
{
    [Tooltip("게임에 포함된 모든 스테이지 목록 (순서대로 진행)")]
    [SerializeField] private StageData[] stages;

    public int StageCount => stages != null ? stages.Length : 0;

    public StageData GetStage(int index)
    {
        return stages[index];
    }
}
