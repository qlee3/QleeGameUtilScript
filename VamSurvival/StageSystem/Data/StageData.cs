using UnityEngine;

/// <summary>
/// 한 스테이지의 데이터를 정의하는 ScriptableObject.
/// 여러 개의 서브스테이지를 순서대로 보유합니다.
/// </summary>
[CreateAssetMenu(fileName = "NewStageData", menuName = "VamSurvival/Stage Data")]
public class StageData : ScriptableObject
{
    [Tooltip("스테이지 표시 이름")]
    public string stageName;

    [Tooltip("이 스테이지에 포함된 서브스테이지 목록 (순서대로 진행)")]
    public SubStageConfig[] subStages;

    public int SubStageCount => subStages != null ? subStages.Length : 0;

    public SubStageConfig GetSubStage(int index)
    {
        if (subStages == null || index < 0 || index >= subStages.Length)
            return null;

        return subStages[index];
    }
}
