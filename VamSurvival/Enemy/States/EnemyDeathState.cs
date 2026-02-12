using UnityEngine;

/// <summary>
/// 적 사망 상태.
/// 사망 연출을 수행하고, 경험치 젬을 스폰한 뒤, 풀로 반환합니다.
/// </summary>
public class EnemyDeathState : EntityState<EnemyController>
{
    public override int AnimStateId => -1; // 사망 애니메이션은 별도 트리거 또는 타입별 처리

    /// <summary>사망 연출 시간 (초). 0이면 즉시 비활성화.</summary>
    private const float DeathDuration = 0.3f;

    private bool hasDroppedLoot;

    protected override void OnEnter(EnemyController entity)
    {
        hasDroppedLoot = false;

        // 이동 정지
        entity.Movement.Stop();

        // 접촉 피해 비활성화
        if (entity.ContactDamage != null)
        {
            entity.ContactDamage.SetActive(false);
        }

        // 경험치 젬 스폰
        DropLoot(entity);
    }

    protected override void OnStep(EnemyController entity)
    {
        // 사망 연출 시간이 지나면 풀로 반환
        if (TimeSinceEntered >= DeathDuration)
        {
            entity.Deactivate();
        }
    }

    protected override void OnExit(EnemyController entity) { }

    private void DropLoot(EnemyController entity)
    {
        if (hasDroppedLoot) return;
        hasDroppedLoot = true;

        float expReward = entity.Data.expReward;
        if (expReward > 0f && ExpGemSpawner.Instance != null)
        {
            ExpGemSpawner.Instance.SpawnGems(entity.transform.position, expReward);
        }
    }
}
