using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 공용 투사체 발사 컴포넌트.
/// 하나의 컴포넌트에서 다중 발사 슬롯을 관리합니다.
/// </summary>
public class ProjectileEmitter : MonoBehaviour
{
    [Serializable]
    public class ShotSlot
    {
        public string slotId = "Primary";
        public ProjectileData projectileData;
        public Transform firePoint;
        public Transform homingTarget;
    }

    [Header("Slots")]
    [SerializeField] private List<ShotSlot> shotSlots = new();
    [SerializeField] private string defaultSlotId = "Primary";

    [Header("Fallback (Legacy)")]
    [FormerlySerializedAs("projectileData")]
    [SerializeField] private ProjectileData fallbackProjectileData;
    [FormerlySerializedAs("firePoint")]
    [SerializeField] private Transform fallbackFirePoint;
    [FormerlySerializedAs("homingTarget")]
    [SerializeField] private Transform fallbackHomingTarget;

    [Header("References")]
    [SerializeField] private ProjectilePool projectilePool;

    [Header("Owner")]
    [Tooltip("비어 있으면 이 컴포넌트가 붙은 오브젝트를 Source로 사용")]
    [SerializeField] private UnityEngine.Object sourceOverride;

    private PlayerController playerController;
    private EnemyController enemyController;

    private void Awake()
    {
        if (projectilePool == null)
        {
            projectilePool = ProjectilePool.Instance;
        }

        if (fallbackFirePoint == null)
        {
            fallbackFirePoint = transform;
        }

        playerController = GetComponentInParent<PlayerController>();
        enemyController = GetComponentInParent<EnemyController>();
    }

    /// <summary>
    /// 기본 슬롯(defaultSlotId) 기준으로 발사합니다.
    /// AnimationEvent에서 직접 호출하는 진입점입니다.
    /// </summary>
    public void Fire()
    {
        FireSlot(defaultSlotId);
    }

    /// <summary>
    /// 슬롯 ID를 지정해 발사합니다.
    /// </summary>
    public bool FireSlot(string slotId)
    {
        if (!TryGetSlot(slotId, out ShotSlot slot))
        {
            return FireFallback();
        }

        Transform origin = slot.firePoint != null ? slot.firePoint : transform;
        Vector3 direction = origin.forward;
        return FireInternal(slot.projectileData, origin, direction, slot.homingTarget);
    }

    /// <summary>
    /// 슬롯 ID + 방향/타겟을 지정해 발사합니다.
    /// </summary>
    public bool FireSlot(string slotId, Vector3 direction, Transform target = null)
    {
        if (!TryGetSlot(slotId, out ShotSlot slot))
        {
            return FireFallback(direction, target);
        }

        Transform origin = slot.firePoint != null ? slot.firePoint : transform;
        Transform finalTarget = target != null ? target : slot.homingTarget;
        return FireInternal(slot.projectileData, origin, direction, finalTarget);
    }

    /// <summary>
    /// 런타임에서 특정 슬롯의 호밍 타겟을 교체합니다.
    /// </summary>
    public void SetSlotHomingTarget(string slotId, Transform target)
    {
        if (TryGetSlot(slotId, out ShotSlot slot))
        {
            slot.homingTarget = target;
        }
    }

    private bool FireFallback()
    {
        Transform origin = fallbackFirePoint != null ? fallbackFirePoint : transform;
        return FireInternal(fallbackProjectileData, origin, origin.forward, fallbackHomingTarget);
    }

    private bool FireFallback(Vector3 direction, Transform target)
    {
        Transform origin = fallbackFirePoint != null ? fallbackFirePoint : transform;
        Transform finalTarget = target != null ? target : fallbackHomingTarget;
        return FireInternal(fallbackProjectileData, origin, direction, finalTarget);
    }

    private bool FireInternal(ProjectileData projectileData, Transform origin, Vector3 direction, Transform target)
    {
        if (projectileData == null)
        {
            Debug.LogWarning("[ProjectileEmitter] projectileData가 비어 있습니다.");
            return false;
        }

        if (projectilePool == null)
        {
            projectilePool = ProjectilePool.Instance;
            if (projectilePool == null)
            {
                Debug.LogWarning("[ProjectileEmitter] ProjectilePool 인스턴스를 찾을 수 없습니다.");
                return false;
            }
        }

        object source = sourceOverride != null ? sourceOverride : gameObject;
        float finalDamage = CalculateRuntimeDamage(projectileData);
        int finalTargetLayerMask = ComposeRuntimeTargetLayerMask(projectileData);

        ProjectileBase projectile = projectilePool.Spawn(
            projectileData,
            origin.position,
            origin.rotation,
            source,
            direction,
            target,
            finalDamage,
            finalTargetLayerMask
        );

        projectile?.SpawnMuzzleEffect(origin.position, origin.rotation);
        return projectile != null;
    }

    private bool TryGetSlot(string slotId, out ShotSlot slot)
    {
        if (shotSlots != null)
        {
            for (int i = 0; i < shotSlots.Count; i++)
            {
                ShotSlot candidate = shotSlots[i];
                if (candidate == null) continue;
                if (string.IsNullOrWhiteSpace(candidate.slotId)) continue;

                if (string.Equals(candidate.slotId, slotId, StringComparison.OrdinalIgnoreCase))
                {
                    slot = candidate;
                    return true;
                }
            }
        }

        slot = null;
        return false;
    }

    private float CalculateRuntimeDamage(ProjectileData projectileData)
    {
        float damage = projectileData.damage;

        // Player 발사체: 공격/마법 배율 + 크리티컬 적용
        if (playerController != null && playerController.Stats != null)
        {
            PlayerStats stats = playerController.Stats;

            float scale = 1f;
            switch (projectileData.damageScaling)
            {
                case ProjectileDamageScaling.AttackPower:
                    scale = stats.AttackPower.Value;
                    break;
                case ProjectileDamageScaling.MagicPower:
                    scale = stats.MagicPower.Value;
                    break;
                case ProjectileDamageScaling.None:
                default:
                    scale = 1f;
                    break;
            }

            damage *= scale;

            float critChance = Mathf.Clamp01(stats.CritChance.Value);
            float critMultiplier = Mathf.Max(0f, stats.CritDamage.Value);
            if (critChance > 0f && critMultiplier > 0f && UnityEngine.Random.value <= critChance)
            {
                damage *= critMultiplier;
            }

            return damage;
        }

        // Enemy 발사체: 배율 1, 크리 확률 0, 크리 배율 0.
        if (enemyController != null)
        {
            const float enemyDamageScale = 1f;
            damage *= enemyDamageScale;
            return damage;
        }

        return damage;
    }

    private int ComposeRuntimeTargetLayerMask(ProjectileData projectileData)
    {
        int mask = projectileData.targetLayers.value;

        if (playerController != null)
        {
            int enemyLayer = LayerMask.NameToLayer("Enemy");
            if (enemyLayer >= 0)
            {
                mask |= 1 << enemyLayer;
            }
        }
        else if (enemyController != null)
        {
            int playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer >= 0)
            {
                mask |= 1 << playerLayer;
            }
        }

        return mask;
    }
}
