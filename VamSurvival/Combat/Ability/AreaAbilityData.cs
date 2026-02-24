using UnityEngine;

/// <summary>
/// 장판(범위 효과) 어빌리티 데이터.
/// 플레이어 위치에서 areaOffset만큼 이동한 위치에 장판을 생성합니다.
/// areaOffset은 플레이어의 로컬 좌표(forward 방향 기준)로 적용됩니다.
/// </summary>
[CreateAssetMenu(menuName = "VamSurvival/Ability/Area")]
public class AreaAbilityData : PlayerAbilityData
{
    [Header("장판")]
    [Tooltip("생성할 장판 프리팹. AreaEffectBase 컴포넌트가 있어야 합니다.")]
    public GameObject areaEffectPrefab;

    [Tooltip("장판 반경")]
    public float areaRadius = 3f;

    [Tooltip("장판 지속 시간(초)")]
    public float areaDuration = 2f;

    [Tooltip("플레이어 로컬 기준 생성 위치 오프셋. forward = Z+")]
    public Vector3 areaOffset = Vector3.zero;

    [Tooltip("지속 시간 동안 반복 피해 간격(초). 0이면 생성 즉시 1회만 피해")]
    public float damageInterval = 0.5f;

    public override void Execute(PlayerController player)
    {
        if (areaEffectPrefab == null)
        {
            Debug.LogWarning($"[{name}] areaEffectPrefab이 설정되지 않았습니다.");
            return;
        }

        float damage = CalculateDamage(player);
        Vector3 worldOffset = player.transform.TransformDirection(areaOffset);
        Vector3 spawnPos = player.transform.position + worldOffset;

        GameObject go = Object.Instantiate(areaEffectPrefab, spawnPos, Quaternion.identity);
        if (go.TryGetComponent<AreaEffectBase>(out var area))
            area.Initialize(areaRadius, areaDuration, damage, damageInterval);
    }
}
