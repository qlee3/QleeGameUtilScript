using UnityEngine;

/// <summary>
/// 씬의 SpawnPoint들과 SpawnLayoutData를 연결하는 작성용 루트.
/// </summary>
public class SpawnLayoutAuthoringRoot : MonoBehaviour
{
    [Tooltip("이 루트에서 편집할 대상 스폰 레이아웃")]
    [SerializeField] private SpawnLayoutData targetLayout;

    public SpawnLayoutData TargetLayout => targetLayout;
}
